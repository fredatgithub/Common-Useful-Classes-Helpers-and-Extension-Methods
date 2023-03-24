namespace WinUICommunity.Common.Internal;
internal static class UnPackagedSetting
{
    public static readonly string AppName = "CommonV3.0";
    public static readonly string RootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
    public static readonly string AppConfigPath = Path.Combine(RootPath, "AppConfig.txt");

    public static void SaveTheme(string value)
    {
        if (!Directory.Exists(RootPath))
        {
            Directory.CreateDirectory(RootPath);
        }

        File.WriteAllText(AppConfigPath, value);
    }

    public static string ReadTheme()
    {
        if (File.Exists(AppConfigPath))
        {
            return File.ReadAllText(AppConfigPath);
        }
        return null;
    }
}
