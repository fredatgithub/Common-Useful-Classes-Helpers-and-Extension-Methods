namespace WinUICommunity.Common.Tools;

public static class LocalizerExtensions
{
    public static string GetLocalizedString(this string uid) => Localizer.Get().GetLocalizedString(uid);
}