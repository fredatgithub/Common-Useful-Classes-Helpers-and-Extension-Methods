namespace WinUICommunity.Common.Tools;

public class EmptyLocalizer : ILocalizer
{
    public void InitializeWindow(FrameworkElement Root, UIElement Content)
    {
    }

    public static readonly ILocalizer Instance = new EmptyLocalizer();

    public event EventHandler<LanguageChangedEventArgs>? LanguageChanged;

    public IEnumerable<string> GetAvailableLanguages() => Enumerable.Empty<string>();

    public string GetCurrentLanguage() => string.Empty;

    public IEnumerable<string> GetLocalizedStrings(string key) => Enumerable.Empty<string>();

    public void SetLanguage(string language)
    {
    }

    public void RegisterRootElement(FrameworkElement rootElement, bool runLocalization = false)
    {
    }

    public void RunLocalizationOnRegisteredRootElements()
    {
    }

    public void RunLocalization(FrameworkElement rootElement)
    {
    }

    public bool TryGetLanguageDictionary(string language, out LanguageDictionary? languageDictionary)
    {
        languageDictionary = null;
        return false;
    }

    public bool TryRegisterUIElementChildrenGetters(Type type, Func<DependencyObject, IEnumerable<DependencyObject>> func) => false;
}
