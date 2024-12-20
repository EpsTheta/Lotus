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
        var trueActivityState = "";
        var DiscordMessage = "Project: Lotus v" + (ProjectLotus.DevVersion ? $"{ProjectLotus.VisibleVersion}.{ProjectLotus.BuildNumber}" : ProjectLotus.VisibleVersion); // if i use devversionstr then code will go off screen
        activity.Details = DiscordMessage;
        if (DataManager.Settings.Gameplay.StreamerMode) return;

        if (activity.State is not "In Menus" and not "In Freeplay")
        {
            gameCode = GameCode.IntToGameName(AmongUsClient.Instance.GameId);
            gameRegion = ServerManager.Instance.CurrentRegion.Name;

            if (AmongUsClient.Instance.GameId == 32) return;

            if (gameCode != "" && gameRegion != "")
            {
                var stateMessage = "";
                string currentMap = GameManager.Instance.LogicOptions.MapId switch
                {
                    0 => "The Skeld",
                    1 => "MIRA HQ",
                    2 => "Polus",
                    4 => "Airship",
                    5 => "The Fungle",
                    6 => "Submerged",
                    _ => "An Unknown Map"
                };

                stateMessage = Game.State switch // only updates on inintro, and inlobby.
                {
                    GameState.None => "(Idle)",
                    GameState.InIntro => $"(Roaming {currentMap})",
                    GameState.InMeeting => "(In Meeting)",
                    GameState.InLobby => "Waiting in Lobby",
                    GameState.Roaming => $"(Roaming {currentMap})",
                    _ => "(Idle)",
                };
                if (Game.State == GameState.InIntro) stateMessage += $"({GameData.Instance.PlayerCount}/{GameManager.Instance.LogicOptions.MaxPlayers})";
                DiscordMessage = "Lotus v" + (ProjectLotus.DevVersion ? ProjectLotus.VisibleVersion + ".2073" : "v" + ProjectLotus.VisibleVersion) + $" - ({gameCode}) | ({gameRegion})";
                trueActivityState = stateMessage;
            }
        }
        activity.Details = DiscordMessage;
        activity.State = trueActivityState;
    }
}