namespace WinUICommunity.Common.Tools;

public partial class ThemeManager
{
    private const string SelectedAppThemeKey = "SelectedAppThemeV2";
    private Window CurrentApplicationWindow;
    private SystemBackdropsHelper systemBackdropsHelper;
    private Dictionary<Window, SystemBackdropsHelper> systemBackdropsHelperDic = new Dictionary<Window, SystemBackdropsHelper>();

    public delegate void ActualThemeChangedEventHandler(FrameworkElement sender, object args);
    public event ActualThemeChangedEventHandler ActualThemeChanged;

    /// <summary>
    /// Gets the current ThemeManager instance.
    /// </summary>
    /// <remarks>
    /// This property returns the current ThemeManager instance, which is used to manage the application's theme and UI settings. The ThemeManager is a singleton class, meaning that only one instance of it can exist at any given time. The Instance property provides access to the current instance of the ThemeManager, which can be used to change themes or perform other operations related to the application's visual style and appearance.
    /// </remarks>
    /// <returns>The current ThemeManager instance.</returns>

    private static ThemeManager instance;
    public static ThemeManager Instance
    {
        get { return instance; }
    }

    /// <summary>
    /// Gets the current actual theme of the app based on the requested theme of the
    /// root element, or if that value is Default, the requested theme of the Application.
    /// </summary>
    public ElementTheme ActualTheme
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
    public ElementTheme RootTheme
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

    /// <summary>
    /// Sets the preferred app mode based on the specified element theme.
    /// </summary>
    /// <param name="theme">The element theme to set the preferred app mode to.</param>
    /// <remarks>
    /// This method sets the preferred app mode based on the specified element theme. If the "theme" parameter is set to "Dark", it sets the preferred app mode to "ForceDark", forcing the app to use a dark theme. If the "theme" parameter is set to "Light", it sets the preferred app mode to "ForceLight", forcing the app to use a light theme. Otherwise, it sets the preferred app mode to "Default", using the system default theme. After setting the preferred app mode, the method flushes the menu themes to ensure that any changes take effect immediately. 
    /// </remarks>
    public void SetPreferredAppMode(ElementTheme theme)
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
    /// Sets the system backdrop type for Windows.
    /// </summary>
    /// <param name="backdropType">The type of backdrop to set.</param>
    /// <remarks>
    /// This method changes the system backdrop type for Windows. The "backdropType" parameter specifies the type of backdrop to set, such as "Acrylic" or "Mica". The backdrop type can affect the visual appearance of the application. 
    /// </remarks>
    public void SetSystemBackdrop(BackdropType backdropType)
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
    /// Retrieves the current system backdrop type for the specified window.
    /// </summary>
    /// <param name="activeWindow">The window to retrieve the backdrop type for.</param>
    /// <returns>The current system backdrop type for the specified window.</returns>
    /// <remarks>
    /// This method retrieves the current system backdrop type for the specified window. The backdrop type can affect the visual appearance of the application, depending on the system settings and the app's design. The method returns the current backdrop type as a "BackdropType" enumeration, such as "Acrylic" or "Mica". 
    /// </remarks>
    public BackdropType GetSystemBackdrop(Window activeWindow)
    {
        var currentWindow = systemBackdropsHelperDic.FirstOrDefault(x => x.Key == activeWindow);
        if (currentWindow.Value != null)
        {
            return currentWindow.Value.GetSystemBackdropType();
        }
        return BackdropType.DefaultColor;
    }

    /// <summary>
    /// Retrieves the current system backdrop type.
    /// </summary>
    /// <returns>The current system backdrop type.</returns>
    /// <remarks>
    /// This method retrieves the current system backdrop type, which can affect the visual appearance of the application, depending on the system settings and the app's design. The method returns the current backdrop type as a "BackdropType" enumeration, such as "Acrylic" or "Mica". 
    /// </remarks>
    public BackdropType GetSystemBackdrop()
    {
        if (systemBackdropsHelper != null)
        {
            return systemBackdropsHelper.GetSystemBackdropType();
        }
        return BackdropType.DefaultColor;
    }

    private void OnActualThemeChanged(FrameworkElement sender, object args)
    {
        ActualThemeChanged?.Invoke(sender, args);
        SetPreferredAppMode(sender.ActualTheme);
        UpdateSystemCaptionButtonColors();
    }

    /// <summary>
    /// Determines whether the current theme is a dark theme.
    /// </summary>
    /// <returns>True if the current theme is a dark theme, otherwise false.</returns>
    /// <remarks>
    /// This method determines whether the current theme is a dark theme. It checks the system settings and returns true if the current theme is set to a dark theme, otherwise false. This can be used to customize the user interface based on the current theme, such as adjusting the color scheme or selecting different icons or images. 
    /// </remarks>
    public bool IsDarkTheme()
    {
        if (RootTheme == ElementTheme.Default)
        {
            return Application.Current.RequestedTheme == ApplicationTheme.Dark;
        }
        return RootTheme == ElementTheme.Dark;
    }

    private void UpdateSystemCaptionButton(AppWindow appWindow)
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
    /// Updates the system caption button colors, only if you are using a AppWindow.TitleBar and TitleBar.ExtendsContentIntoTitleBar = true
    /// </summary>
    /// <remarks>
    /// This method updates the system caption button colors, which are the colors used for the minimize, maximize, and close buttons on the window caption bar. The method uses the current system color scheme to update the colors, ensuring that they match the user's preferences. This can be useful for applications that use custom caption buttons or need to ensure that the caption buttons are visible and accessible to users. 
    /// </remarks>
    public void UpdateSystemCaptionButtonColors()
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
    /// Changes the application theme to the specified element theme.
    /// </summary>
    /// <param name="elementTheme">The element theme to change the application theme to.</param>
    /// <remarks>
    /// This method changes the application theme to the specified element theme, such as "Dark" or "Light". It sets the requested theme as the current theme for the application's UI, including the window background, foreground, and other visual elements. This can be useful for customizing the user interface based on the user's preferences or the application's requirements. 
    /// </remarks>
    public void ChangeTheme(ElementTheme elementTheme)
    {
        RootTheme = elementTheme;
    }

    /// <summary>
    /// Event handler for when a radio button is checked.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <remarks>
    /// This method is an event handler for when a radio button is checked. It takes the sender object as a parameter and performs the required actions based on the checked state of the radio button. The specific actions performed by this method depend on the requirements of the application and the UI design, and can include updating the user interface, setting application preferences, or performing other operations.
    /// </remarks>
    public void OnRadioButtonChecked(object sender)
    {
        var selectedTheme = ((RadioButton)sender)?.Tag?.ToString();
        if (selectedTheme != null)
        {
            RootTheme = GeneralHelper.GetEnum<ElementTheme>(selectedTheme);
        }
    }

    /// <summary>
    /// Sets the default radio button item for the specified panel.
    /// </summary>
    /// <param name="ThemePanel">The panel to set the default radio button item for. StackPanel/Grid</param>
    /// <remarks>
    /// This method sets the default radio button item for the specified panel. It is typically used to ensure that one of the radio buttons in the panel is selected by default when the panel is displayed to the user. 
    /// </remarks>
    public void SetRadioButtonDefaultItem(Panel ThemePanel)
    {
        var currentTheme = RootTheme.ToString();
        ThemePanel.Children.Cast<RadioButton>().FirstOrDefault(c => c?.Tag?.ToString() == currentTheme).IsChecked = true;
    }

    /// <summary>
    /// Event handler for when the selection is changed in a combobox.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <remarks>
    /// This method is an event handler for when the selection is changed in a combobox. It takes the sender object as a parameter and performs the required actions based on the selected item in the combobox.
    /// </remarks>
    public void OnComboBoxSelectionChanged(object sender)
    {
        var cmb = (ComboBox)sender;
        var selectedTheme = (cmb?.SelectedItem as ComboBoxItem)?.Tag?.ToString();
        if (selectedTheme != null)
        {
            RootTheme = GeneralHelper.GetEnum<ElementTheme>(selectedTheme);
        }
    }

    /// <summary>
    /// Sets the default item for the specified combobox.
    /// </summary>
    /// <param name="themeComboBox">The combobox to set the default item for.</param>
    /// <remarks>
    /// This method sets the default item for the specified combobox. It is typically used to ensure that one of the items in the combobox is selected by default when the combo box is displayed to the user.
    /// </remarks>
    public void SetComboBoxDefaultItem(ComboBox themeComboBox)
    {
        var currentTheme = RootTheme.ToString();
        var item = themeComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(c => c?.Tag?.ToString() == currentTheme);
        themeComboBox.SelectedItem = item;
    }
}
