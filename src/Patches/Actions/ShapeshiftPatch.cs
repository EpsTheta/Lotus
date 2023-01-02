using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using TownOfHost.Extensions;
using TownOfHost.Roles;
using UnityEngine;

namespace TownOfHost.Patches.Actions;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
public class ShapeshiftPatch
{
    public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        Logger.Info($"{__instance?.GetNameWithRole()} => {target?.GetNameWithRole()}", "Shapeshift");
        if (!AmongUsClient.Instance.AmHost) return;

        var shapeshifter = __instance;
        var shapeshifting = shapeshifter.PlayerId != target.PlayerId;

        Main.CheckShapeshift[shapeshifter.PlayerId] = shapeshifting;
        Main.ShapeshiftTarget[shapeshifter.PlayerId] = target.PlayerId;

        if (!shapeshifting) Camouflage.RpcSetSkin(__instance);

        if (shapeshifter.Is(Warlock.Ref<Warlock>()))
        {
            if (Main.CursedPlayers[shapeshifter.PlayerId] != null)//呪われた人がいるか確認
            {
                if (shapeshifting && !Main.CursedPlayers[shapeshifter.PlayerId].Data.IsDead)//変身解除の時に反応しない
                {
                    var cp = Main.CursedPlayers[shapeshifter.PlayerId];
                    Vector2 cppos = cp.transform.position;//呪われた人の位置
                    Dictionary<PlayerControl, float> cpdistance = new();
                    float dis;
                    foreach (PlayerControl p in PlayerControl.AllPlayerControls)
                    {
                        if (!p.Data.IsDead && p != cp)
                        {
                            dis = Vector2.Distance(cppos, p.transform.position);
                            cpdistance.Add(p, dis);
                            Logger.Info($"{p?.Data?.PlayerName}の位置{dis}", "Warlock");
                        }
                    }
                    var min = cpdistance.OrderBy(c => c.Value).FirstOrDefault();//一番小さい値を取り出す
                    PlayerControl targetw = min.Key;
                    targetw.SetRealKiller(shapeshifter);
                    Logger.Info($"{targetw.GetNameWithRole()}was killed", "Warlock");
                    cp.RpcMurderPlayerV2(targetw);//殺す
                    shapeshifter.RpcGuardAndKill(shapeshifter);
                    Main.isCurseAndKill[shapeshifter.PlayerId] = false;
                }
                Main.CursedPlayers[shapeshifter.PlayerId] = null;
            }
        }
        if (shapeshifter.Is(EvilTracker.Ref<EvilTracker>())) EvilTrackerOLD.Shapeshift(shapeshifter, target, shapeshifting);

        if (shapeshifter.CanMakeMadmate() && shapeshifting)
        {//変身したとき一番近い人をマッドメイトにする処理
            Vector2 shapeshifterPosition = shapeshifter.transform.position;//変身者の位置
            Dictionary<PlayerControl, float> mpdistance = new();
            float dis;
            foreach (PlayerControl p in PlayerControl.AllPlayerControls)
            {
                if (!p.Data.IsDead && p.Data.Role.Role != RoleTypes.Shapeshifter && !p.Is(Roles.RoleType.Impostor) && !p.Is(SKMadmate))
                {
                    dis = Vector2.Distance(shapeshifterPosition, p.transform.position);
                    mpdistance.Add(p, dis);
                }
            }
            if (mpdistance.Count() != 0)
            {
                var min = mpdistance.OrderBy(c => c.Value).FirstOrDefault();//一番値が小さい
                PlayerControl targetm = min.Key;
                targetm.RpcSetCustomRole(SKMadmate);
                Logger.Info($"Make SKMadmate:{targetm.name}", "Shapeshift");
                Main.SKMadmateNowCount++;
                Utils.MarkEveryoneDirtySettings();
                Utils.NotifyRoles();
            }
        }
        if (shapeshifter.Is(FireWorks.Ref<FireWorks>())) FireWorksOLD.ShapeShiftState(shapeshifter, shapeshifting);
        if (shapeshifter.Is(Sniper.Ref<Sniper>())) SniperOLD.ShapeShiftCheck(shapeshifter, shapeshifting);

        //変身解除のタイミングがずれて名前が直せなかった時のために強制書き換え
        if (!shapeshifting)
        {
            new DTask(() =>
            {
                Utils.NotifyRoles(NoCache: true);
            },
            1.2f, "ShapeShiftNotify");
        }
    }
}