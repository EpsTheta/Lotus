using System.Collections.Generic;
using TOHTOR.Roles.Events;
using TOHTOR.Roles.Interactions;
using TOHTOR.Roles.Internals.Attributes;
using TOHTOR.Roles.RoleGroups.Vanilla;
using VentLib.Options.Game;

namespace TOHTOR.Roles.RoleGroups.Crew;

public class Bastion: Engineer
{
    // Here we can use the vent button as cooldown
    [NewOnSetup]
    private HashSet<int> bombedVents;

    [RoleAction(RoleActionType.AnyEnterVent)]
    private void EnterVent(Vent vent, PlayerControl player)
    {
        bool isBombed = bombedVents.Remove(vent.Id);
        if (isBombed) MyPlayer.InteractWith(player, CreateInteraction(player));
        else if (player.PlayerId == MyPlayer.PlayerId) bombedVents.Add(vent.Id);
    }

    [RoleAction(RoleActionType.RoundEnd)]
    private void ClearVents() => bombedVents.Clear();

    private IndirectInteraction CreateInteraction(PlayerControl deadPlayer)
    {
        return new IndirectInteraction(new FatalIntent(true, () => new BombedEvent(deadPlayer, MyPlayer)), this);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Plant Bomb Cooldown")
                .BindFloat(v => VentCooldown = v)
                .AddFloatRange(2, 120, 2.5f, 8, "s")
                .Build());


    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor("#524f4d")
            .CanVent(false);
}