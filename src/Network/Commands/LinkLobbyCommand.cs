using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using Lotus.Chat;
using Lotus.Network.PrivacyPolicy;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ProBuilder;
using VentLib.Commands;
using VentLib.Commands.Attributes;
using VentLib.Commands.Interfaces;
using VentLib.Localization.Attributes;
using VentLib.Networking;
using VentLib.Utilities.Extensions;
using VentLib.Utilities;
using System.Collections.Generic;



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
    [Localized(nameof(APIOff))] public static string APIOff = "One or more of your API settings are off, and thus your lobby cannot be linked.";
    [Localized(nameof(SuccessfulLink))] public static string SuccessfulLink = "Your lobby has been successfully linked and your forum thread will now recieve updates about when you're in lobby, etc.";
    [Localized(nameof(AlreadyLinked))] public static string AlreadyLinked = "This lobby is already linked, there is no need to run this command again.";
    [Localized(nameof(didNotRunLinkFirst))] public static string didNotRunLinkFirst = "Pssst... You need to run <b>/link</b> first."; // anger
    [Localized(nameof(couldNotLink))] public static string couldNotLink = "An error occurred, and we couldn't link your lobby.\nTry again in a few minutes or see if our lobby backend is down at <b>status.lotusau.top</b>";

    private static readonly List<byte> lobbyLinkInit = new();
    private static readonly List<byte> lobbyLinkConfirm = new();

    public void Receive(PlayerControl source, CommandContext context)
    {
        string message = string.Join(" ", context.Args);
        int gameId = AmongUsClient.Instance.GameId;

        if (gameId.ToString() == "32") return; // local lobby

        if (lobbyLinkConfirm.Contains(source.PlayerId))
        {
            ChatHandlers.NotPermitted(AlreadyLinked).Send(source); return;
        }

        if (PrivacyPolicyInfo.Instance != null)
        {

            if (!PrivacyPolicyInfo.Instance.ConnectWithAPI)
            {
                ChatHandlers.NotPermitted(APIOff).Send(source);
                return;
            }
            if (!NetworkRules.AllowRoomDiscovery)
            {
                ChatHandlers.NotPermitted(APIOff).Send(source);
                return;
            }
        }
        if (!source.IsHost())
        {
            ChatHandlers.NotPermitted(CommandHostOnly).Send(source);
            return;
        }


        if (source == PlayerControl.LocalPlayer && message == "confirm")
        {
            if (!lobbyLinkInit.Contains(source.PlayerId))
            {
                ChatHandlers.NotPermitted(didNotRunLinkFirst).Send(source); return;
            }

            ChatHandler.Of(SuccessfulLink).Send(source);
            Async.Execute(SendLinkConfirmation(source, context));


        }
        else if (source == PlayerControl.LocalPlayer && message == "")
        {
            if (lobbyLinkInit.Contains(source.PlayerId))
            {
                ChatHandlers.NotPermitted(WaitingOnLink).Send(source);
                return;
            }
            GUIUtility.systemCopyBuffer = gameId.ToString(); //copy lobby id
            Log.PrintToConsole("Copied Lobby ID: " + gameId);

            ChatHandler.Of(CopiedLobbyId).Send(source);
            Async.Execute(SendLinkRequest(source, context));

        }
        else return;

    }
    private IEnumerator SendLinkRequest(PlayerControl source, CommandContext context)
    {
        string stringifiedGameId = AmongUsClient.Instance.GameId.ToString();
        UnityWebRequest linkRequest = UnityWebRequest.Post(NetConstants.LobbyHost + "linklobby/" + stringifiedGameId, "");

        linkRequest.SetRequestHeader("Content-Type", "application/json");

        linkRequest.SetRequestHeader("link-type", "1");
        linkRequest.SetRequestHeader("game-id", stringifiedGameId);
        linkRequest.SetRequestHeader("host-name", source.name);
        linkRequest.SetRequestHeader("region", ServerManager.Instance.CurrentRegion.Name);
        linkRequest.SetRequestHeader("mod-version", ProjectLotus.VisibleVersion);
        linkRequest.SetRequestHeader("Authorization", source.FriendCode); // until i think of a better way to auth people, itll just use friendcode -> this should be fine with the idea I have.

        yield return linkRequest.SendWebRequest();

        switch (linkRequest.result)
        {
            case UnityWebRequest.Result.Success:
                lobbyLinkInit.Add(source.PlayerId);
                break;
            default:
                ChatHandler.Of(couldNotLink).Send(source);
                break;
        }
    }
    private IEnumerator SendLinkConfirmation(PlayerControl source, CommandContext context)
    {
        return null;

    }

}