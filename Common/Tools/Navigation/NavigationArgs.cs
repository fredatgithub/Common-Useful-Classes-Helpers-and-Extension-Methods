using WinUICommunity.Shared.DataModel;

namespace WinUICommunity.Common.Helpers;

public class NavigationArgs
{
    public NavigationView NavigationView;
    public object Parameter;
    public string JsonRelativeFilePath;
    public IncludedInBuildMode IncludedInBuildMode;
}