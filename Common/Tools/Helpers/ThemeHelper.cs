using Windows.Foundation.Metadata;

namespace WinUICommunity.Common.Helpers;

[Obsolete("The ThemeHelper is deprecated, Please use the new ThemeManager class, which has newer and better features", false)]
public static class ThemeHelper
{
    private const string SelectedAppThemeKey = "SelectedAppTheme";
    private static Window CurrentApplicationWindow;
    private static SystemBackdropsHelper systemBackdropsHelper;
    private static Dictionary<Window, SystemBackdropsHelper> systemBackdropsHelperDic = new Dictionary<Window, SystemBackdropsHelper>();

    /// <summary>
    /// Gets the current actual theme of the app based on the requested theme of the
    /// root element, or if that value is Default, the requested theme of the Application.
    /// </summary>
    public static ElementTheme ActualTheme
    {
        get
        {
            foreach (Window window in WindowHelper.ActiveWindows)
            {
                if (window.Content is FrameworkElement rootElement)
                {
                    if (rootElement.RequestedTheme != ElementTheme.Default)
                    {
                        return rootElement.RequestedTheme;
                    }
                }
            }

            if (CurrentApplicationWindow != null && CurrentApplicationWindow.Content is FrameworkElement element)
            {
                if (element.RequestedTheme != ElementTheme.Default)
                {
                    return element.RequestedTheme;
                }
            }
            return GeneralHelper.GetEnum<ElementTheme>(Application.Current.RequestedTheme.ToString());
        }
    }

    /// <summary>
    /// Gets or sets (with LocalSettings persistence) the RequestedTheme of the root element.
    /// </summary>
    public static ElementTheme RootTheme
    {
        get
        {
            foreach (Window window in WindowHelper.ActiveWindows)
            {
                if (window.Content is FrameworkElement rootElement)
                {
                    return rootElement.RequestedTheme;
                }
            }
            if (CurrentApplicationWindow != null && CurrentApplicationWindow.Content is FrameworkElement element)
            {
                return element.RequestedTheme;
            }
            return ElementTheme.Default;
        }
        set
        {
            foreach (Window window in WindowHelper.ActiveWindows)
            {
                if (window.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = value;
                }
            }

            if (CurrentApplicationWindow != null && CurrentApplicationWindow.Content is FrameworkElement element)
            {
                element.RequestedTheme = value;
            }

            if (ApplicationHelper.IsPackaged)
            {
                ApplicationData.Current.LocalSettings.Values[SelectedAppThemeKey] = value.ToString();
            }
            else
            {
                Internal.UnPackagedSetting.SaveTheme(value.ToString());
            }
        }
    }
    public static void SetPreferredAppMode(ElementTheme theme)
    {
        if (theme == ElementTheme.Dark)
        {
            NativeMethods.SetPreferredAppMode(NativeMethods.PreferredAppMode.ForceDark);
        }
        else if (theme == ElementTheme.Light)
        {
            NativeMethods.SetPreferredAppMode(NativeMethods.PreferredAppMode.ForceLight);
        }
        else
        {
            NativeMethods.SetPreferredAppMode(NativeMethods.PreferredAppMode.Default);
        }
        NativeMethods.FlushMenuThemes();
    }

    /// <summary>
    /// Change SystemBackdrop Type for Windows
    /// </summary>
    /// <param name="backdropType"></param>
    public static void SetSystemBackdropType(BackdropType backdropType)
    {
        foreach (var helper in systemBackdropsHelperDic.Values)
        {
            helper.SetBackdrop(backdropType);
        }

        if (systemBackdropsHelper != null)
        {
            systemBackdropsHelper.SetBackdrop(backdropType);
        }
    }

    /// <summary>
    /// Get SystemBackdrop Type for WindowHelper.ActiveWindows
    /// </summary>
    /// <param name="activeWindow"></param>
    /// <returns></returns>
    public static BackdropType GetSystemBackdropType(Window activeWindow)
    {
        var currentWindow = systemBackdropsHelperDic.FirstOrDefault(x => x.Key == activeWindow);
        if (currentWindow.Value != null)
        {
            return currentWindow.Value.GetSystemBackdropType();
        }
        return BackdropType.DefaultColor;
    }

    /// <summary>
    /// Get SystemBackdrop Type for Current Window
    /// </summary>
    /// <returns></returns>
    public static BackdropType GetSystemBackdropType()
    {
        if (systemBackdropsHelper != null)
        {
            return systemBackdropsHelper.GetSystemBackdropType();
        }
        return BackdropType.DefaultColor;
    }

    /// <summary>
    /// Initialize ThemeHelper
    /// </summary>
    /// <param name="window">Default Window</param>
    public static void Initialize(Window window)
    {
        CurrentApplicationWindow = window;
        Initialize();
    }

    /// <summary>
    /// Initialize ThemeHelper with Default Theme
    /// </summary>
    /// <param name="window">Default Window</param>
    /// <param name="theme">Default Theme Dark/Light/Default</param>
    public static void Initialize(Window window, ElementTheme theme)
    {
        CurrentApplicationWindow = window;
        Initialize();
        ChangeTheme(theme);
    }

    /// <summary>
    /// Initialize ThemeHelper with SystemBackdrop
    /// </summary>
    /// <param name="window">Default Window</param>
    /// <param name="backdropType">Default Backdrop Mica/MicaAlt/Acrylic</param>
    public static void Initialize(Window window, BackdropType backdropType)
    {
        CurrentApplicationWindow = window;
        Initialize();

        foreach (Window _window in WindowHelper.ActiveWindows)
        {
            var _backdropsHelper = new SystemBackdropsHelper(_window);
            systemBackdropsHelperDic.Add(_window, _backdropsHelper);
            _backdropsHelper.SetBackdrop(backdropType);
        }

        if (CurrentApplicationWindow != null)
        {
            var backdropsHelper = new SystemBackdropsHelper(CurrentApplicationWindow);
            systemBackdropsHelper = backdropsHelper;
            backdropsHelper.SetBackdrop(backdropType);
        }
    }

    /// <summary>
    /// Initialize ThemeHelper with Default Theme and SystemBackdrops
    /// </summary>
    /// <param name="window">Default Window</param>
    /// <param name="theme">Default Theme Dark/Light/Default</param>
    /// <param name="backdropType">Default Backdrop Mica/MicaAlt/Acrylic</param>
    public static void Initialize(Window window, ElementTheme theme, BackdropType backdropType)
    {
        Initialize(window, backdropType);
        ChangeTheme(theme);
    }

    private static void Initialize()
    {
        foreach (Window window in WindowHelper.ActiveWindows)
        {
            if (window.Content is FrameworkElement rootElement)
            {
                SetPreferredAppMode(rootElement.ActualTheme);
                rootElement.ActualThemeChanged += OnActualThemeChanged;
            }
        }

        if (CurrentApplicationWindow != null && CurrentApplicationWindow.Content is FrameworkElement element)
        {
            SetPreferredAppMode(element.ActualTheme);
            element.ActualThemeChanged += OnActualThemeChanged;
        }

        string savedTheme = string.Empty;

        if (ApplicationHelper.IsPackaged)
        {
            savedTheme = ApplicationData.Current.LocalSettings.Values[SelectedAppThemeKey]?.ToString();
        }
        else
        {
            savedTheme = Internal.UnPackagedSetting.ReadTheme();
        }
        if (savedTheme != null)
        {
            RootTheme = GeneralHelper.GetEnum<ElementTheme>(savedTheme);
        }
        UpdateSystemCaptionButtonColors();
    }

    private static void OnActualThemeChanged(FrameworkElement sender, object args)
    {
        SetPreferredAppMode(sender.ActualTheme);
        UpdateSystemCaptionButtonColors();
    }

    public static bool IsDarkTheme()
    {
        if (RootTheme == ElementTheme.Default)
        {
            return Application.Current.RequestedTheme == ApplicationTheme.Dark;
        }
        return RootTheme == ElementTheme.Dark;
    }

    private static void UpdateSystemCaptionButton(AppWindow appWindow)
    {
        var titleBar = appWindow.TitleBar;
        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        if (IsDarkTheme())
        {
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonInactiveForegroundColor = Colors.White;
        }
        else
        {
            titleBar.ButtonForegroundColor = Colors.Black;
            titleBar.ButtonInactiveForegroundColor = Colors.Black;
        }
    }

    /// <summary>
    /// Update System Caption Buttons Background and Foreground for Dark/Light Theme.
    /// </summary>
    public static void UpdateSystemCaptionButtonColors()
    {
        foreach (Window window in WindowHelper.ActiveWindows)
        {
            var appWindow = WindowHelper.GetAppWindowForCurrentWindow(window);
            UpdateSystemCaptionButton(appWindow);
        }

        if (CurrentApplicationWindow != null)
        {
            var appWindow = WindowHelper.GetAppWindowForCurrentWindow(CurrentApplicationWindow);
            UpdateSystemCaptionButton(appWindow);
        }
    }

    /// <summary>
    /// Change RootTheme
    /// </summary>
    /// <param name="elementTheme"></param>
    public static void ChangeTheme(ElementTheme elementTheme)
    {
        RootTheme = elementTheme;
    }

    /// <summary>
    /// Use This Method in RadioButton Checked event
    /// </summary>
    /// <param name="sender"></param>
    public static void OnRadioButtonChecked(object sender)
    {
        var selectedTheme = ((RadioButton)sender)?.Tag?.ToString();
        if (selectedTheme != null)
        {
            RootTheme = GeneralHelper.GetEnum<ElementTheme>(selectedTheme);
        }
    }

    /// <summary>
    /// Use This Method in Loaded event of a Page, if you want to Set RadioButton default value.
    /// </summary>
    /// <param name="ThemePanel">The panel (Grid/StackPanel) that contains the RadioButton</param>
    public static void SetRadioButtonDefaultItem(Panel ThemePanel)
    {
        var currentTheme = RootTheme.ToString();
        (ThemePanel.Children.Cast<RadioButton>().FirstOrDefault(c => c?.Tag?.ToString() == currentTheme)).IsChecked = true;
    }

    /// <summary>
    /// Use This Method in ComboBox SelectionChanged event
    /// </summary>
    /// <param name="sender"></param>
    public static void ComboBoxSelectionChanged(object sender)
    {
        var cmb = (ComboBox)sender;
        var selectedTheme = (cmb?.SelectedItem as ComboBoxItem)?.Tag?.ToString();
        if (selectedTheme != null)
        {
            RootTheme = GeneralHelper.GetEnum<ElementTheme>(selectedTheme);
        }
    }

    /// <summary>
    /// Use This Method in Loaded event of a Page, if you want to Set ComboBox default value.
    /// </summary>
    /// <param name="themeComboBox"></param>
    public static void SetComboBoxDefaultItem(ComboBox themeComboBox)
    {
        var currentTheme = RootTheme.ToString();
        var item = (themeComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(c => c?.Tag?.ToString() == currentTheme));
        themeComboBox.SelectedItem = item;
    }
}
