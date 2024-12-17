using HarmonyLib;
using Discord;
using InnerNet;
using Lotus.API.Odyssey;
using AmongUs.Data;

namespace Lotus.Discord.Patches;

[HarmonyPatch(typeof(ActivityManager), nameof(ActivityManager.UpdateActivity))]
public class DiscordPatch
{
    private static string gameCode = "";
    private static string gameRegion = "";

    public static void Prefix(ref Activity activity)
    {
        var DiscordMessage = "Project: Lotus v" + (ProjectLotus.DevVersion ? ProjectLotus.VisibleVersion + "." + ProjectLotus.BuildNumber : "v" + ProjectLotus.VisibleVersion);
        activity.Details = DiscordMessage;

        if (DataManager.Settings.Gameplay.StreamerMode) return;

        if (activity.State != "In Menus")
        {
            gameCode = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            gameRegion = ServerManager.Instance.CurrentRegion.Name;

            if (AmongUsClient.Instance.GameId == 32) return;

            if (gameCode != "" && gameRegion != "")
            {
                string stateMessage = "";
                string currentMap = GameManager.Instance.LogicOptions.MapId switch
                {
                    0 => "The Skeld",
                    1 => "MIRA HQ",
                    2 => "Polus",
                    4 => "Airship",
                    5 => "The Fungle",
                    _ => "An Unknown Map"
                };

                stateMessage = Game.State switch
                {
                    GameState.None => "(Idle)",
                    GameState.InIntro => $"(Roaming {currentMap})", // same as roaming since it got stuck on inintro
                    GameState.InMeeting => "(In Meeting)",
                    GameState.InLobby => "", // presence already has stuff for when in lobby
                    GameState.Roaming => $"(Roaming {currentMap})",
                    _ => "Unknown State",
                };
                DiscordMessage = "Project: Lotus v" + (ProjectLotus.DevVersion ? ProjectLotus.VisibleVersion + "." + ProjectLotus.BuildNumber : "v" + ProjectLotus.VisibleVersion) + $" - ({gameCode}) | ({gameRegion}) • {stateMessage}";
            }
        }
        activity.Details = DiscordMessage;
    }
}