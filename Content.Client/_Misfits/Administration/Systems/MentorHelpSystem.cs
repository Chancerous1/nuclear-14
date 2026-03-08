// #Misfits Change - Client-side mentor help system
using Content.Client._Misfits.UserInterface.Systems.MentorHelp;
using Content.Shared._Misfits.Administration;
using JetBrains.Annotations;
using Robust.Client.Audio;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Client._Misfits.Administration.Systems;

[UsedImplicitly]
public sealed class MentorHelpSystem : SharedMentorHelpSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public event EventHandler<MentorHelpTextMessage>? OnMentorHelpTextMessageReceived;
    private (TimeSpan Timestamp, bool Typing) _lastTypingUpdateSent;

    protected override void OnMentorHelpTextMessage(MentorHelpTextMessage message, EntitySessionEventArgs eventArgs)
    {
        OnMentorHelpTextMessageReceived?.Invoke(this, message);
    }

    public void Send(NetUserId channelId, string text, bool playSound)
    {
        _audio.PlayGlobal(MentorHelpUIController.MHelpSendSound, Filter.Local(), false);
        RaiseNetworkEvent(new MentorHelpTextMessage(channelId, channelId, text, playSound: playSound));
        SendInputTextUpdated(channelId, false);
    }

    public void SendInputTextUpdated(NetUserId channel, bool typing)
    {
        if (_lastTypingUpdateSent.Typing == typing &&
            _lastTypingUpdateSent.Timestamp + TimeSpan.FromSeconds(1) > _timing.RealTime)
            return;

        _lastTypingUpdateSent = (_timing.RealTime, typing);
        RaiseNetworkEvent(new MentorHelpClientTypingUpdated(channel, typing));
    }
}
