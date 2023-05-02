using System.Collections.Generic;
using TOHTOR.Options.General;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Attributes;

namespace TOHTOR.Options;

[Localized("Options")]
[LoadStatic]
public class GeneralOptions
{
    public static AdminOptions AdminOptions;
    public static DebugOptions DebugOptions;
    public static GameplayOptions GameplayOptions;
    public static MayhemOptions MayhemOptions;
    public static MiscellaneousOptions MiscellaneousOptions;
    public static SabotageOptions SabotageOptions;
    public static VoteOptions VoteOptions;

    public static List<GameOption> AllOptions = new();

    static GeneralOptions()
    {
        AdminOptions = new AdminOptions();
        AllOptions.AddRange(AdminOptions.AllOptions);

        GameplayOptions = new GameplayOptions();
        AllOptions.AddRange(GameplayOptions.AllOptions);

        SabotageOptions = new SabotageOptions();
        AllOptions.AddRange(SabotageOptions.AllOptions);

        MayhemOptions = new MayhemOptions();
        AllOptions.AddRange(MayhemOptions.AllOptions);

        MiscellaneousOptions = new MiscellaneousOptions();
        AllOptions.AddRange(MiscellaneousOptions.AllOptions);

        DebugOptions = new DebugOptions();
        AllOptions.AddRange(DebugOptions.AllOptions);
    }
}