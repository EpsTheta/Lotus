using System;
using HarmonyLib;
using UnityEngine;
using VentLib.Utilities.Extensions;

namespace Lotus.Patches.Hud;

[HarmonyPatch(typeof (HudManager), "SetMapAndInfoButtonsEnabled")]
public class MatchInfoGuidePatch
{

    public static void Postfix(HudManager __instance, bool enabled)
    {
        if (!__instance.MatchInfoButton) return;
        __instance.MatchInfoButton.gameObject.SetActive(false);
        __instance.MatchInfoButton.gameObject.FindChildOrEmpty<GameObject>("Inactive", true).IfPresent(go => go.SetActive(false));
        __instance.MatchInfoButton.enabled = false;
    }
}
