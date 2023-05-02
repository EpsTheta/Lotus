using TOHTOR.API.Odyssey;
using VentLib.Utilities.Harmony.Attributes;

namespace TOHTOR.Patches;

public class BasicWrapperPatches
{
    [QuickPrefix(typeof(PlayerControl), nameof(PlayerControl.RawSetName))]
    public static bool WrapSetNamePatch(PlayerControl __instance, string name)
    {
        __instance.cosmetics.SetName(name);
        __instance.cosmetics.SetNameMask(true);
        if (Game.State is GameState.InLobby) __instance.name = name;
        return false;
    }
}