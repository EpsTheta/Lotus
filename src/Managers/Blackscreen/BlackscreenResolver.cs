using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Vanilla.Meetings;
using Lotus.Chat;
using Lotus.RPC;
using Lotus.Utilities;
using Lotus.Victory;
using Lotus.Extensions;
using Lotus.Logging;
using UnityEngine;
using VentLib.Networking.RPC;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using AmongUs.GameOptions;
using Lotus.Managers.Blackscreen.Interfaces;
using Sentry.Internal.Extensions;

namespace Lotus.Managers.Blackscreen;

internal class BlackscreenResolver : IBlackscreenResolver
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(BlackscreenResolver));

    private Dictionary<byte, Dictionary<byte, (bool isDead, bool isDisconnected)>> specificPlayerStates = null!;
    private Dictionary<byte, (bool isDead, bool isDisconnected)> playerStates = null!;
    private MeetingDelegate meetingDelegate;
    private HashSet<byte> unpatchable;

    private bool _patching;

    internal BlackscreenResolver(MeetingDelegate meetingDelegate)
    {
        this.meetingDelegate = meetingDelegate;

        Hooks.PlayerHooks.PlayerMessageHook.Bind(nameof(BlackscreenResolver), _ => BlockLavaChat(), true);
        Hooks.GameStateHooks.GameEndHook.Bind(nameof(BlackscreenResolver), _ => _patching = false, true);
    }

    public virtual bool Patching() => _patching;

    public virtual void OnMeetingDestroy()
    {
        if (!AmongUsClient.Instance.AmHost) return;
        _patching = true;

        specificPlayerStates = new();
        playerStates = new();

        // Store and save the data of each player.
        byte exiledPlayer = meetingDelegate.ExiledPlayer?.PlayerId ?? byte.MaxValue;
        NetworkedPlayerInfo[] playerInfos = GameData.Instance.AllPlayers.ToArray().Where(p => p != null).ToArray();
        playerInfos.FirstOrOptional(p => p.PlayerId == exiledPlayer).IfPresent(info => info.IsDead = true);

        playerStates = playerInfos.ToDict(i => i.PlayerId, i => (i.IsDead, i.Disconnected));
        specificPlayerStates = playerInfos.ToDict(i => i.PlayerId, i => AntiBlackoutLogic.PatchedData(i.Object, exiledPlayer));

        // Update and send new data for each player.
        foreach (PlayerControl player in Players.GetPlayers())
        {
            if (!specificPlayerStates.TryGetValue(player.PlayerId, out var myStates)) return;
            if (playerStates[player.PlayerId].isDisconnected) continue;
            int clientId = player.GetClientId();
            if (clientId == -1) continue;

            GameData.Instance.AllPlayers.ToArray().Where(i => i != null).ForEach(info =>
            {
                if (!myStates.TryGetValue(info.PlayerId, out var val)) return;
                info.IsDead = val.isDead;
                info.Disconnected = val.isDisconnected;
                try
                {
                    if (info.Object != null) info.PlayerName = info.Object.name;
                }
                catch { }
            });
            GeneralRPC.SendGameData(clientId);
        }
    }

    public virtual void FixBlackscreens(Action runOnFinish)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        Async.Schedule(() =>
        {
            _patching = false;
            GameData.Instance.AllPlayers.ToArray().Where(i => i != null).ForEach(info =>
            {
                if (!playerStates.TryGetValue(info.PlayerId, out var val)) return;
                info.IsDead = val.isDead;
                info.Disconnected = val.isDisconnected;
                try
                {
                    if (info.Object != null) info.PlayerName = info.Object.name;
                }
                catch { }
            });
            GeneralRPC.SendGameData();
            CheckEndGamePatch.Deferred = false;
            runOnFinish();
        }, 1f);
    }

    private void BlockLavaChat()
    {
        if (!_patching) return;
        if (Game.State is GameState.InLobby)
        {
            _patching = false;
            return;
        }
        ChatHandler chatHandler = ChatHandler.Of("", "- Lava Chat Fix -");
        foreach (PlayerControl player in Players.GetPlayers())
        {
            (bool isDead, bool isDisconnected) = playerStates[player.PlayerId];
            if (isDead || isDisconnected) continue;
            for (int i = 0; i < 20; i++) chatHandler.Send(player);
        }
    }
}