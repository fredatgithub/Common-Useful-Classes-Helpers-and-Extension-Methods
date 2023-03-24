namespace WinUICommunity.Common.Tools
{
    public partial class ThemeManager
    {
        public ThemeManager() { }

        /// <summary>
        /// Gets the current instance of the ThemeManager.
        /// </summary>
        /// <returns>The current instance of the ThemeManager.</returns>
        /// <remarks>
        /// This method retrieves the current instance of the ThemeManager, which is used to manage the application's theme and UI settings. It is a static method that can be called from anywhere in the application to access the current ThemeManager instance. The ThemeManager can be used to change the application theme, update UI elements, and perform other operations related to the application's visual style and appearance. 
        /// </remarks>
        public static ThemeManager GetCurrent()
        {
            if (Instance == null)
            {
                instance = new ThemeManager();
            }
            return Instance;
        }
        private void InternalInitialize()
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

        private void InternalInitializeSystemBackdrops(Window window, BackdropType backdropType)
        {
            CurrentApplicationWindow = window;
            InternalInitialize();

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

        #region Initialize Window
        private void InternalInitialize(Window window)
        {
            CurrentApplicationWindow = window;
            InternalInitialize();
        }

        /// <summary>
        /// Initializes a new instance of the ThemeManager class with the specified window.
        /// </summary>
        /// <param name="window">The window to apply the theme to.</param>
        /// <remarks>
        /// This constructor creates a new instance of the ThemeManager class with the specified window. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified window is used as the target for theme changes and updates. 
        /// </remarks>
        public ThemeManager(Window window)
        {
            InternalInitialize(window);
        }

        /// <summary>
        /// Initializes the ThemeManager instance.
        /// </summary>
        /// <returns>The initialized ThemeManager instance.</returns>
        /// <remarks>
        /// This method initializes the ThemeManager instance and returns it. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance. 
        /// </remarks>
        public static ThemeManager Initialize()
        {
            if (Instance == null)
            {
                instance = new ThemeManager(null);
            }
            else
            {
                instance.InternalInitialize(null);
            }
            return Instance;
        }

        /// <summary>
        /// Initializes the ThemeManager instance with the specified window.
        /// </summary>
        /// <param name="window">The window to apply the theme to.</param>
        /// <returns>The initialized ThemeManager instance.</returns>
        /// <remarks>
        /// This method initializes the ThemeManager instance with the specified window and returns it. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified window is used as the target for theme changes and updates. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance. 
        /// </remarks>
        public static ThemeManager Initialize(Window window)
        {
            if (Instance == null)
            {
                instance = new ThemeManager(window);
            }
            else
            {
                instance.InternalInitialize(window);
            }
            return Instance;
        }

        #endregion

        #region Initialize Window/ElementTheme

        private void InternalInitialize(Window window, ElementTheme theme)
        {
            CurrentApplicationWindow = window;
            InternalInitialize();
            ChangeTheme(theme);
        }

        /// <summary>
        /// Initializes a new instance of the ThemeManager class with the specified window and theme.
        /// </summary>
        /// <param name="window">The window to apply the theme to.</param>
        /// <param name="theme">The initial theme to apply to the window.</param>
        /// <remarks>
        /// This constructor creates a new instance of the ThemeManager class with the specified window and initial theme. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified window is used as the target for theme changes and updates, and the specified theme is applied as the initial theme for the window. 
        /// </remarks>
        public ThemeManager(Window window, ElementTheme theme)
        {
            InternalInitialize(window, theme);
        }

        /// <summary>
        /// Initializes the ThemeManager instance with the specified theme.
        /// </summary>
        /// <param name="theme">The initial theme to apply to the application.</param>
        /// <returns>The initialized ThemeManager instance.</returns>
        /// <remarks>
        /// This method initializes the ThemeManager instance with the specified initial theme and returns it. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified theme is applied as the initial theme for the application. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance.
        /// </remarks>
        public static ThemeManager Initialize(ElementTheme theme)
        {
            if (Instance == null)
            {
                instance = new ThemeManager(null, theme);
            }
            else
            {
                instance.InternalInitialize(null, theme);
            }
            return Instance;
        }

        /// <summary>
        /// Initializes the ThemeManager instance with the specified window and theme.
        /// </summary>
        /// <param name="window">The window to apply the theme to.</param>
        /// <param name="theme">The initial theme to apply to the window.</param>
        /// <returns>The initialized ThemeManager instance.</returns>
        /// <remarks>
        /// This method initializes the ThemeManager instance with the specified window and initial theme, and returns it. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified window is used as the target for theme changes and updates, and the specified theme is applied as the initial theme for the window. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance. 
        /// </remarks>
        public static ThemeManager Initialize(Window window, ElementTheme theme)
        {
            if (Instance == null)
            {
                instance = new ThemeManager(window, theme);
            }
            else
            {
                instance.InternalInitialize(window, theme);
            }
            return Instance;
        }

        #endregion

        #region Initialize Window/BackdropType

        /// <summary>
        /// Initializes a new instance of the ThemeManager class with the specified window and backdrop type.
        /// </summary>
        /// <param name="window">The window to apply the backdrop type to.</param>
        /// <param name="backdropType">The initial backdrop type to apply to the window.</param>
        /// <remarks>
        /// This constructor creates a new instance of the ThemeManager class with the specified window and initial backdrop type. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified window is used as the target for backdrop type changes and updates, and the specified backdrop type is applied as the initial backdrop type for the window. 
        /// </remarks>
        public ThemeManager(Window window, BackdropType backdropType)
        {
            InternalInitializeSystemBackdrops(window, backdropType);
        }

        /// <summary>
        /// Initializes the ThemeManager instance with the specified backdrop type.
        /// </summary>
        /// <param name="backdropType">The initial backdrop type to apply to the application.</param>
        /// <returns>The initialized ThemeManager instance.</returns>
        /// <remarks>
        /// This method initializes the ThemeManager instance with the specified initial backdrop type and returns it. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified backdrop type is applied as the initial backdrop type for the application. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance. 
        /// </remarks>
        public static ThemeManager Initialize(BackdropType backdropType)
        {
            if (Instance == null)
            {
                instance = new ThemeManager(null, backdropType);
            }
            else
            {
                instance.InternalInitializeSystemBackdrops(null, backdropType);
            }
            return Instance;
        }

        /// <summary>
        /// Initializes the ThemeManager instance with the specified window and backdrop type.
        /// </summary>
        /// <param name="window">The window to apply the backdrop type to.</param>
        /// <param name="backdropType">The initial backdrop type to apply to the window.</param>
        /// <returns>The initialized ThemeManager instance.</returns>
        /// <remarks>
        /// This method initializes the ThemeManager instance with the specified window and initial backdrop type, and returns it. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified window is used as the target for backdrop type changes and updates, and the specified backdrop type is applied as the initial backdrop type for the window. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance. 
        /// </remarks>
        public static ThemeManager Initialize(Window window, BackdropType backdropType)
        {
            if (Instance == null)
            {
                instance = new ThemeManager(window, backdropType);
            }
            else
            {
                instance.InternalInitializeSystemBackdrops(window, backdropType);
            }
            return Instance;
        }

        #endregion

        #region Initialize Window/ElementTheme/BackdropType

        private void InternalInitialize(Window window, ElementTheme theme, BackdropType backdropType)
        {
            InternalInitializeSystemBackdrops(window, backdropType);
            ChangeTheme(theme);
        }

        /// <summary>
        /// Initializes the ThemeManager instance with the specified window, theme, and backdrop type.
        /// </summary>
        /// <param name="window">The window to apply the theme and backdrop type to.</param>
        /// <param name="theme">The initial theme to apply to the window.</param>
        /// <param name="backdropType">The initial backdrop type to apply to the window.</param>
        /// <remarks>
        /// This method initializes the ThemeManager instance with the specified window, initial theme, and initial backdrop type. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified window is used as the target for theme and backdrop type changes and updates, and the specified theme and backdrop type are applied as the initial theme and backdrop type for the window. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance. 
        /// </remarks>
        public ThemeManager(Window window, ElementTheme theme, BackdropType backdropType)
        {
            InternalInitialize(window, theme, backdropType);
        }

        /// <summary>
        /// Initializes the ThemeManager instance with the specified initial theme and backdrop type.
        /// </summary>
        /// <param name="theme">The initial theme to apply to the window.</param>
        /// <param name="backdropType">The initial backdrop type to apply to the window.</param>
        /// <returns>The initialized ThemeManager instance.</returns>
        /// <remarks>
        /// This method initializes the ThemeManager instance with the specified initial theme and initial backdrop type, and returns it. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified theme and backdrop type are applied as the initial theme and backdrop type for the entire application. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance. 
        /// </remarks>
        public static ThemeManager Initialize(ElementTheme theme, BackdropType backdropType)
        {
            if (Instance == null)
            {
                instance = new ThemeManager(null, theme, backdropType);
            }
            else
            {
                instance.InternalInitialize(null, theme, backdropType);
            }
            return Instance;
        }

        /// <summary>
        /// Initializes the ThemeManager instance with the specified window, initial theme, and backdrop type.
        /// </summary>
        /// <param name="window">The window to apply the theme and backdrop type to.</param>
        /// <param name="theme">The initial theme to apply to the window.</param>
        /// <param name="backdropType">The initial backdrop type to apply to the window.</param>
        /// <returns>The initialized ThemeManager instance.</returns>
        /// <remarks>
        /// This method initializes the ThemeManager instance with the specified window, initial theme, and initial backdrop type, and returns it. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified window is used as the target for theme and backdrop type changes and updates, and the specified theme and backdrop type are applied as the initial theme and backdrop type for the window. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance. 
        /// </remarks>
        public static ThemeManager Initialize(Window window, ElementTheme theme, BackdropType backdropType)
        {
            if (Instance == null)
            {
                instance = new ThemeManager(window, theme, backdropType);
            }
            else
            {
                instance.InternalInitialize(window, theme, backdropType);
            }
            return Instance;
        }

        #endregion

    }
}
