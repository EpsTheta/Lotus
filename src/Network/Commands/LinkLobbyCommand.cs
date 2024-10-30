
using Lotus.Chat;
using Lotus.Network.PrivacyPolicy;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Localization.Attributes;
using VentLib.Networking;


namespace Lotus.Network.Commands;

[Localized("Commands.LinkLobby")]
[Command("link")]

//<summary>
//* User runs /link to get their lobby id, and it's copied to their clipboard
//* User then takes lobby id to Discord, and runs /lobby link with Lilypad
//* User comes back to game and runs '/link confirm' to confirm the link (?)
//</summary>

public class LinkLobbyCommand : ICommandReceiver
{

    [Localized(nameof(CopiedLobbyId))] public static string CopiedLobbyId = "Successfully copied your Lobby ID! Run <b>/lobby link</b> in the Discord then follow the instructions.";
    [Localized(nameof(WaitingOnLink))] public static string WaitingOnLink = "We're waiting for you to link your lobby to Discord, run /lobby link in your forum thread!";
    [Localized(nameof(CommandHostOnly))] public static string CommandHostOnly = "This command can only be run by the host of the lobby.";
    [Localized(nameof(SuccessfulLink))] public static string SuccessfulLink = "Your lobby has been successfully linked and your forum thread will now recieve updates about when you're in lobby, etc.";
    [Localized(nameof(AlreadyLinked))] public static string AlreadyLinked = "This lobby is already linked, there is no need to run this command again.";

    public void Receive(PlayerControl source, CommandContext context)
    {
        if (PrivacyPolicyInfo.Instance != null)
        {
            if (!PrivacyPolicyInfo.Instance.ConnectWithAPI) return;
            if (!NetworkRules.AllowRoomDiscovery) return; // i think this is the right option?
        }
        int gameId = AmongUsClient.Instance.GameId;
        GUIUtility.systemCopyBuffer = gameId.ToString(); // copy gameid to clipboard
        ChatHandler.Of(CopiedLobbyId).Send(source);
    }

}