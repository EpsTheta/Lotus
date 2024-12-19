using System;
using System.Linq;
using UnityEngine;
using Lotus.Options;
using Lotus.Extensions;
using System.Threading.Tasks;
using System.Collections.Generic;
using VentLib.Utilities.Attributes;
using VentLib.Utilities.Harmony.Attributes;

namespace Lotus.GUI.Patches;

[LoadStatic]
public class ChatDarkMode
{

    static ChatDarkMode()
    {
        PersistentAssetLoader.RegisterSprite("QuickChatIcon", "Lotus.assets.DarkTheme.DarkQuickChat.png", 100);
        PersistentAssetLoader.RegisterSprite("DarkKeyboard", "Lotus.assets.DarkTheme.DarkKeyboard.png", 100);
        PersistentAssetLoader.RegisterSprite("DarkReport", "Lotus.assets.DarkTheme.DarkReport.png", 100);
    }
    [QuickPostfix(typeof(ChatBubble), nameof(ChatBubble.SetName))]
    public static void BubbleSetNamePostfix(ChatBubble __instance, bool isDead, bool voted)
    {
        if (ClientOptions.VideoOptions.ChatDarkMode)
        {
            __instance.Background.color = new(0.1f, 0.1f, 0.1f, 1f);
            __instance.TextArea.color = Color.white;
            if (!__instance.playerInfo.Object.IsAlive() && LobbyBehaviour.Instance == null) __instance.Background.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        }
    }
    private static SpriteRenderer QuickChatIcon;
    private static SpriteRenderer OpenBanMenuIcon;
    private static SpriteRenderer OpenKeyboardIcon;


    [QuickPostfix(typeof(ChatController), nameof(ChatController.Update))]
    public static void ControllerUpdatePostfix(ChatController __instance)
    {
        var chatBubble = __instance.chatBubblePool.Prefab.Cast<ChatBubble>();
        chatBubble.TextArea.overrideColorTags = false;

        if (ClientOptions.VideoOptions.ChatDarkMode)
        {
            chatBubble.TextArea.color = Color.white;
            chatBubble.Background.color = new Color(0.1f, 0.1f, 0.1f, 1f);

            __instance.freeChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
            __instance.quickChatField.background.color = new Color32(40, 40, 40, byte.MaxValue);
            __instance.quickChatField.text.color = Color.white;

            if (QuickChatIcon == null)
                QuickChatIcon = GameObject.Find("QuickChatIcon")?.transform.GetComponent<SpriteRenderer>()!;
            else
                QuickChatIcon.sprite = PersistentAssetLoader.GetSprite("QuickChatIcon");

            if (OpenBanMenuIcon == null)
                OpenBanMenuIcon = GameObject.Find("OpenBanMenuIcon")?.transform.GetComponent<SpriteRenderer>()!;
            else
                OpenBanMenuIcon.sprite = PersistentAssetLoader.GetSprite("DarkReport");

            if (OpenKeyboardIcon == null)
                OpenKeyboardIcon = GameObject.Find("OpenKeyboardIcon")?.transform.GetComponent<SpriteRenderer>()!;
            else
                OpenKeyboardIcon.sprite = PersistentAssetLoader.GetSprite("DarkKeyboard");
        }
        else __instance.freeChatField.textArea.outputText.color = Color.black;
    }
}