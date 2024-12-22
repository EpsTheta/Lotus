using Lotus.GUI;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.RoleGroups.NeutralKilling;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using Lotus.Options;
using Lotus.Extensions;
using VentLib.Options.UI;
using Lotus.GameModes.Colorwars.Factions;

namespace Lotus.Roles.RoleGroups.Colorwars;

public class Painter : NeutralKillingBase
{
    private Cooldown gracePeriod = null!;

    [UIComponent(GUI.Name.UI.Text)]
    public string GracePeriodText() => gracePeriod.IsReady() ? "" : Color.gray.Colorize(Translations.GracePeriodText).Formatted(gracePeriod + "s");

    protected override void PostSetup()
    {
        KillCooldown = ExtraGamemodeOptions.ColorwarsOptions.KillCooldown;
        if (ExtraGamemodeOptions.ColorwarsOptions.ConvertColorMode) KillCooldown *= 2; // Because we rpc mark players. we need to multiply the kill cd by 2.
        base.PostSetup();
    }

    [RoleAction(LotusActionType.RoundStart)]
    private void BeginGracePeriod()
    {
        gracePeriod.Start(ExtraGamemodeOptions.ColorwarsOptions.GracePeriod);
    }

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (gracePeriod.NotReady()) return false;
        int myColor = MyPlayer.cosmetics.bodyMatProperties.ColorId;
        if (myColor == target.cosmetics.bodyMatProperties.ColorId) return false;

        if (!ExtraGamemodeOptions.ColorwarsOptions.ConvertColorMode) return base.TryKill(target);

        target.RpcSetColor((byte)myColor);
        target.PrimaryRole().RoleColor = (Color)Palette.PlayerColors[MyPlayer.cosmetics.bodyMatProperties.ColorId];
        MyPlayer.RpcMark(target);
        return false;
    }
    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier)
        .RoleFlags(RoleFlag.DontRegisterOptions | RoleFlag.Hidden)
        .RoleAbilityFlags(RoleAbilityFlag.CannotSabotage)
        .IntroSound(AmongUs.GameOptions.RoleTypes.Shapeshifter)
        .VanillaRole(AmongUs.GameOptions.RoleTypes.Impostor)
        .Faction(ColorFaction.Instance)
        .RoleColor(Color.white)
        .CanVent(true);

    [Localized(nameof(Painter))]
    public static class Translations
    {
        [Localized(nameof(GracePeriodText))]
        public static string GracePeriodText = "No-Kill Grace Period: {0}";
    }
}