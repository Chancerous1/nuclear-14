// Misfits Change - System to suppress idle sounds during combat
using Content.Shared.Sound;
using Content.Shared.Sound.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Server._Misfits.Sound;

/// <summary>
/// Temporarily disables <see cref="SpamEmitSoundComponent"/> when an entity with
/// <see cref="IdleSoundComponent"/> performs an attack, then re-enables it after a cooldown.
/// </summary>
public sealed class IdleSoundSystem : EntitySystem
{
    [Dependency] private readonly SharedEmitSoundSystem _emitSound = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IdleSoundComponent, MeleeAttackEvent>(OnMeleeAttack);
    }

    private void OnMeleeAttack(Entity<IdleSoundComponent> entity, ref MeleeAttackEvent args)
    {
        Suppress(entity);
    }

    private void Suppress(Entity<IdleSoundComponent> entity)
    {
        entity.Comp.CooldownRemaining = entity.Comp.CooldownDuration;

        if (entity.Comp.Suppressed)
            return;

        entity.Comp.Suppressed = true;
        _emitSound.SetEnabled((entity.Owner, (SpamEmitSoundComponent?) null), false);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<IdleSoundComponent>();
        while (query.MoveNext(out var uid, out var idle))
        {
            if (!idle.Suppressed)
                continue;

            idle.CooldownRemaining -= frameTime;

            if (idle.CooldownRemaining > 0f)
                continue;

            idle.Suppressed = false;
            _emitSound.SetEnabled((uid, (SpamEmitSoundComponent?) null), true);
        }
    }
}
