// #Misfits Change - Server-side mentor help system
using System.Linq;
using Content.Server.Administration.Managers;
using Content.Server.Afk;
using Content.Server.GameTicking;
using Content.Server.Players.RateLimiting;
using Content.Shared._Misfits.Administration;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.Players.RateLimiting;
using JetBrains.Annotations;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._Misfits.Administration.Systems;

[UsedImplicitly]
public sealed class MentorHelpSystem : SharedMentorHelpSystem
{
    private const string RateLimitKey = "MentorHelp";

    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IAfkManager _afkManager = default!;
    [Dependency] private readonly PlayerRateLimitManager _rateLimit = default!;

    private ISawmill _sawmill = default!;
    private readonly Dictionary<NetUserId, (TimeSpan Timestamp, bool Typing)> _typingUpdateTimestamps = new();
    private readonly Dictionary<NetUserId, DateTime> _activeConversations = new();

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = IoCManager.Resolve<ILogManager>().GetSawmill("MHELP");
        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;

        SubscribeNetworkEvent<MentorHelpClientTypingUpdated>(OnClientTypingUpdated);

        _rateLimit.Register(
            RateLimitKey,
            new RateLimitRegistration(
                CCVars.AhelpRateLimitPeriod,
                CCVars.AhelpRateLimitCount,
                PlayerRateLimitedAction)
        );
    }

    private void PlayerRateLimitedAction(ICommonSession obj)
    {
        RaiseNetworkEvent(
            new MentorHelpTextMessage(obj.UserId, default, "Rate limited, please wait before sending another message.", playSound: false),
            obj.Channel);
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus != SessionStatus.Connected && e.NewStatus != SessionStatus.Disconnected)
            return;

        if (!_activeConversations.ContainsKey(e.Session.UserId))
            return;

        var message = e.NewStatus switch
        {
            SessionStatus.Connected => "reconnected.",
            SessionStatus.Disconnected => "disconnected.",
            _ => null
        };

        if (message == null)
            return;

        var color = e.NewStatus == SessionStatus.Connected ? Color.Green.ToHex() : Color.Yellow.ToHex();
        var inGameMessage = $"[color={color}]{e.Session.Name} {message}[/color]";

        var msg = new MentorHelpTextMessage(
            userId: e.Session.UserId,
            trueSender: SystemUserId,
            text: inGameMessage,
            sentAt: DateTime.Now,
            playSound: false
        );

        var mentors = GetTargetMentors();
        foreach (var mentor in mentors)
        {
            RaiseNetworkEvent(msg, mentor);
        }
    }

    private void OnClientTypingUpdated(MentorHelpClientTypingUpdated msg, EntitySessionEventArgs args)
    {
        if (_typingUpdateTimestamps.TryGetValue(args.SenderSession.UserId, out var tuple) &&
            tuple.Typing == msg.Typing &&
            tuple.Timestamp + TimeSpan.FromSeconds(1) > _timing.RealTime)
        {
            return;
        }

        _typingUpdateTimestamps[args.SenderSession.UserId] = (_timing.RealTime, msg.Typing);

        var isMentor = _adminManager.GetAdminData(args.SenderSession)?.HasFlag(AdminFlags.Mentorhelp) ?? false;
        var channel = isMentor ? msg.Channel : args.SenderSession.UserId;
        var update = new MentorHelpPlayerTypingUpdated(channel, args.SenderSession.Name, msg.Typing);

        foreach (var mentor in GetTargetMentors())
        {
            if (mentor.UserId == args.SenderSession.UserId)
                continue;

            RaiseNetworkEvent(update, mentor);
        }
    }

    protected override void OnMentorHelpTextMessage(MentorHelpTextMessage message, EntitySessionEventArgs eventArgs)
    {
        base.OnMentorHelpTextMessage(message, eventArgs);

        var senderSession = eventArgs.SenderSession;

        var personalChannel = senderSession.UserId == message.UserId;
        var senderAdmin = _adminManager.GetAdminData(senderSession);
        var senderMentor = senderAdmin?.HasFlag(AdminFlags.Mentorhelp) ?? false;
        var authorized = personalChannel || senderMentor;
        if (!authorized)
            return;

        if (_rateLimit.CountAction(eventArgs.SenderSession, RateLimitKey) != RateLimitStatus.Allowed)
            return;

        _activeConversations[message.UserId] = DateTime.Now;

        var escapedText = FormattedMessage.EscapeText(message.Text);
        var mentorColor = "#9B59B6"; // Purple for mentors
        var senderText = $"{senderSession.Name}";

        if (senderAdmin is not null && senderMentor)
        {
            senderText = $"[color={mentorColor}]{senderSession.Name}[/color]";
        }

        senderText = $"{(message.PlaySound ? "" : "(S) ")}{senderText}: {escapedText}";

        var playSound = senderAdmin == null || message.PlaySound;
        var msg = new MentorHelpTextMessage(message.UserId, senderSession.UserId, senderText, playSound: playSound);

        // Notify all mentors
        var mentors = GetTargetMentors();
        foreach (var channel in mentors)
        {
            RaiseNetworkEvent(msg, channel);
        }

        // Notify the player if they are not a mentor
        if (_playerManager.TryGetSessionById(message.UserId, out var session))
        {
            if (!mentors.Contains(session.Channel))
            {
                RaiseNetworkEvent(msg, session.Channel);
            }
        }

        if (mentors.Count != 0)
            return;

        // No mentors online
        if (senderSession.Channel != null)
        {
            var systemText = "[color=red]No mentors are currently online to receive your message. Please try again later or use admin help (F2).[/color]";
            var noMentorsMsg = new MentorHelpTextMessage(message.UserId, SystemUserId, systemText);
            RaiseNetworkEvent(noMentorsMsg, senderSession.Channel);
        }
    }

    private IList<INetChannel> GetTargetMentors()
    {
        return _adminManager.ActiveAdmins
            .Where(p => _adminManager.GetAdminData(p)?.HasFlag(AdminFlags.Mentorhelp) ?? false)
            .Select(p => p.Channel)
            .ToList();
    }
}
