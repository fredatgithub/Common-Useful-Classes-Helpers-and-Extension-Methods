namespace SettingsUI.Tools;

public class LocalizerOptions
{
    public bool AddDefaultResourcesStringsFolder { get; set; } = true;

    public List<LocalizerResourcesStringsFolder> AdditionalResourcesStringsFolders { get; } = new();

    public List<LanguageDictionary> AdditionalLanguageDictionaries { get; } = new();

    public string DefaultLanguage { get; set; } = "en-US";
}
