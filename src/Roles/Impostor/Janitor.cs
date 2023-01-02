using AmongUs.GameOptions;
using TownOfHost.Extensions;
using TownOfHost.Interface;
using TownOfHost.ReduxOptions;
using UnityEngine;

namespace TownOfHost.Roles;

public class Janitor: Impostor
{
    private Cooldown cleanCooldown;

    [RoleAction(RoleActionType.AttemptKill)]
    public new bool TryKill(PlayerControl target)
    {
        cleanCooldown.Start(DesyncOptions.OriginalHostOptions.GetFloat(FloatOptionNames.KillCooldown));
        return base.TryKill(target);
    }

    [RoleAction(RoleActionType.SelfReportBody)]
    private void JanitorCleanBody(GameData.PlayerInfo target, ActionHandle handle)
    {
        if (cleanCooldown.NotReady()) return;
        handle.Cancel();
        cleanCooldown.Start();
        SyncOptions();

        foreach (DeadBody deadBody in Object.FindObjectsOfType<DeadBody>())
            if (deadBody.ParentId == target.Object.PlayerId)
            {
                Main.unreportableBodies.Add(target.PlayerId);
                Object.Destroy(deadBody.gameObject);
            }

        MyPlayer.RpcGuardAndKill(MyPlayer);
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(Override.KillCooldown, () => cleanCooldown.Duration * 2, () => cleanCooldown.NotReady());
}