using Nucs.JsonSettings.Modulation;
using Nucs.JsonSettings;
using Nucs.JsonSettings.Modulation.Recovery;
using Nucs.JsonSettings.Fluent;

namespace WinUICommunity.Common.Internal;
internal class CommonSettings : JsonSettings, IVersionable
{
    public static readonly string AppName = ApplicationHelper.GetProjectNameAndVersion();
    public static readonly string RootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
    public static readonly string AppConfigPath = Path.Combine(RootPath, "CommonAppConfig.json");

    [EnforcedVersion("1.1.0.0")]
    public Version Version { get; set; } = new Version(1, 1, 0, 0);
    public override string FileName { get; set; } = AppConfigPath;

    
    public ElementTheme ElementTheme { get; set; }
    public BackdropType BackdropType { get; set; }

    public static CommonSettings Settings = Configure<CommonSettings>()
                               .WithRecovery(RecoveryAction.RenameAndLoadDefault)
                               .WithVersioning(VersioningResultAction.RenameAndLoadDefault)
                               .LoadNow();
}
