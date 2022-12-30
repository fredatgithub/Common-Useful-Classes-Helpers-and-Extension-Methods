namespace SettingsUI.Helpers;
public static class ApplicationHelper
{
    private const uint APPMODEL_ERROR_NO_PACKAGE = 15700;
    public static bool IsPackaged { get; } = GetCurrentPackageName() != null;

    public static string GetCurrentPackageName()
    {
        var length = 0u;
        NativeMethods.GetCurrentPackageFullName(ref length);

        var result = new StringBuilder((int)length);
        var error = NativeMethods.GetCurrentPackageFullName(ref length, result);

        if (error == APPMODEL_ERROR_NO_PACKAGE)
            return null;

        return result.ToString();
    }

    public static Windows.ApplicationModel.Package GetPackageDetails()
    {
        return Windows.ApplicationModel.Package.Current;
    }
    public static Windows.ApplicationModel.PackageVersion GetPackageVersion()
    {
        return GetPackageDetails().Id.Version;
    }

    public static string GetFullPathToExe()
    {
        var path = AppDomain.CurrentDomain.BaseDirectory;
        var pos = path.LastIndexOf("\\");
        return path.Substring(0, pos);
    }

    public static string GetFullPathToAsset(string assetName)
    {
        return GetFullPathToExe() + "\\Assets\\" + assetName;
    }
}

