namespace SettingsUI.Tools;

public interface ILocalizer
{
    void InitializeWindow(FrameworkElement Root, UIElement Content);

    event EventHandler<LanguageChangedEventArgs>? LanguageChanged;

    IEnumerable<string> GetAvailableLanguages();

    string GetCurrentLanguage();

    IEnumerable<string> GetLocalizedStrings(string key);

    void SetLanguage(string language);

    void RegisterRootElement(FrameworkElement rootElement, bool runLocalization = false);

    void RunLocalizationOnRegisteredRootElements();

    void RunLocalization(FrameworkElement rootElement);

    bool TryGetLanguageDictionary(string language, out LanguageDictionary? languageDictionary);

    bool TryRegisterUIElementChildrenGetters(Type type, Func<DependencyObject, IEnumerable<DependencyObject>> func);
}
