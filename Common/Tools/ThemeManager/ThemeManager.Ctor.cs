using WinUICommunity.Common.Internal;

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

            RootTheme = CommonSettings.Settings.ElementTheme;
            UpdateSystemCaptionButtonColors();
        }

        private void InternalInitializeSystemBackdrops(Window window, BackdropType backdropType, bool forceBackdrop)
        {
            CurrentApplicationWindow = window;
            InternalInitialize();

            foreach (Window _window in WindowHelper.ActiveWindows)
            {
                var _backdropsHelper = new SystemBackdropsHelper(_window);
                systemBackdropsHelperDic.Add(_window, _backdropsHelper);
                _backdropsHelper.SetBackdrop(LoadCurrentSystemBackdrop(backdropType, forceBackdrop));
            }

            if (CurrentApplicationWindow != null)
            {
                var backdropsHelper = new SystemBackdropsHelper(CurrentApplicationWindow);
                systemBackdropsHelper = backdropsHelper;
                backdropsHelper.SetBackdrop(LoadCurrentSystemBackdrop(backdropType, forceBackdrop));
            }
        }

        private BackdropType LoadCurrentSystemBackdrop(BackdropType backdropType, bool ForceBackdrop)
        {
            var currentBackdrop = CommonSettings.Settings.BackdropType;
            if (ForceBackdrop)
            {
                return backdropType;
            }
            else
            {
                return currentBackdrop;
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
        private ElementTheme LoadCurrentTheme(ElementTheme theme, bool forceTheme)
        {
            var currentTheme = CommonSettings.Settings.ElementTheme;
            if (forceTheme)
            {
                return theme;
            }
            else
            {
                return currentTheme;
            }
        }
        private void InternalInitialize(Window window, ElementTheme theme, bool forceTheme)
        {
            CurrentApplicationWindow = window;
            InternalInitialize();
            ChangeTheme(LoadCurrentTheme(theme, forceTheme));
        }

        /// <summary>
        /// Initializes a new instance of the ThemeManager class with the specified window and theme.
        /// </summary>
        /// <param name="window">The window to apply the theme to.</param>
        /// <param name="theme">The initial theme to apply to the window.</param>
        /// <param name="forceTheme">force theme, saved theme will be ignored.</param>
        /// <remarks>
        /// This constructor creates a new instance of the ThemeManager class with the specified window and initial theme. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified window is used as the target for theme changes and updates, and the specified theme is applied as the initial theme for the window. 
        /// </remarks>
        public ThemeManager(Window window, ElementTheme theme, bool forceTheme = false)
        {
            InternalInitialize(window, theme, forceTheme);
        }

        /// <summary>
        /// Initializes the ThemeManager instance with the specified theme.
        /// </summary>
        /// <param name="theme">The initial theme to apply to the application.</param>
        /// <param name="forceTheme">force theme, saved theme will be ignored.</param>
        /// <returns>The initialized ThemeManager instance.</returns>
        /// <remarks>
        /// This method initializes the ThemeManager instance with the specified initial theme and returns it. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified theme is applied as the initial theme for the application. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance.
        /// </remarks>
        public static ThemeManager Initialize(ElementTheme theme, bool forceTheme = false)
        {
            if (Instance == null)
            {
                instance = new ThemeManager(null, theme, forceTheme);
            }
            else
            {
                instance.InternalInitialize(null, theme, forceTheme);
            }
            return Instance;
        }

        /// <summary>
        /// Initializes the ThemeManager instance with the specified window and theme.
        /// </summary>
        /// <param name="window">The window to apply the theme to.</param>
        /// <param name="theme">The initial theme to apply to the window.</param>
        /// <param name="forceTheme">force theme, saved theme will be ignored.</param>
        /// <returns>The initialized ThemeManager instance.</returns>
        /// <remarks>
        /// This method initializes the ThemeManager instance with the specified window and initial theme, and returns it. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified window is used as the target for theme changes and updates, and the specified theme is applied as the initial theme for the window. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance. 
        /// </remarks>
        public static ThemeManager Initialize(Window window, ElementTheme theme, bool forceTheme = false)
        {
            if (Instance == null)
            {
                instance = new ThemeManager(window, theme, forceTheme);
            }
            else
            {
                instance.InternalInitialize(window, theme, forceTheme);
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
        /// <param name="forceBackdrop">force backdrop type, saved backdrop will be ignored.</param>
        /// <remarks>
        /// This constructor creates a new instance of the ThemeManager class with the specified window and initial backdrop type. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified window is used as the target for backdrop type changes and updates, and the specified backdrop type is applied as the initial backdrop type for the window. 
        /// </remarks>
        public ThemeManager(Window window, BackdropType backdropType, bool forceBackdrop = false)
        {
            InternalInitializeSystemBackdrops(window, backdropType, forceBackdrop);
        }

        /// <summary>
        /// Initializes the ThemeManager instance with the specified backdrop type.
        /// </summary>
        /// <param name="backdropType">The initial backdrop type to apply to the application.</param>
        /// <param name="forceBackdrop">force backdrop type, saved backdrop will be ignored.</param>
        /// <returns>The initialized ThemeManager instance.</returns>
        /// <remarks>
        /// This method initializes the ThemeManager instance with the specified initial backdrop type and returns it. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified backdrop type is applied as the initial backdrop type for the application. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance. 
        /// </remarks>
        public static ThemeManager Initialize(BackdropType backdropType, bool forceBackdrop = false)
        {
            if (Instance == null)
            {
                instance = new ThemeManager(null, backdropType, forceBackdrop);
            }
            else
            {
                instance.InternalInitializeSystemBackdrops(null, backdropType, forceBackdrop);
            }
            return Instance;
        }

        /// <summary>
        /// Initializes the ThemeManager instance with the specified window and backdrop type.
        /// </summary>
        /// <param name="window">The window to apply the backdrop type to.</param>
        /// <param name="backdropType">The initial backdrop type to apply to the window.</param>
        /// <param name="forceBackdrop">force backdrop type, saved backdrop will be ignored.</param>
        /// <returns>The initialized ThemeManager instance.</returns>
        /// <remarks>
        /// This method initializes the ThemeManager instance with the specified window and initial backdrop type, and returns it. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified window is used as the target for backdrop type changes and updates, and the specified backdrop type is applied as the initial backdrop type for the window. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance. 
        /// </remarks>
        public static ThemeManager Initialize(Window window, BackdropType backdropType, bool forceBackdrop = false)
        {
            if (Instance == null)
            {
                instance = new ThemeManager(window, backdropType, forceBackdrop);
            }
            else
            {
                instance.InternalInitializeSystemBackdrops(window, backdropType, forceBackdrop);
            }
            return Instance;
        }

        #endregion

        #region Initialize Window/ElementTheme/BackdropType

        private void InternalInitialize(Window window, ElementTheme theme, BackdropType backdropType, bool forceBackdrop)
        {
            InternalInitializeSystemBackdrops(window, backdropType, forceBackdrop);
            ChangeTheme(theme);
        }

        /// <summary>
        /// Initializes the ThemeManager instance with the specified window, theme, and backdrop type.
        /// </summary>
        /// <param name="window">The window to apply the theme and backdrop type to.</param>
        /// <param name="theme">The initial theme to apply to the window.</param>
        /// <param name="backdropType">The initial backdrop type to apply to the window.</param>
        /// <param name="forceBackdrop">force backdrop type, saved backdrop will be ignored.</param>
        /// <remarks>
        /// This method initializes the ThemeManager instance with the specified window, initial theme, and initial backdrop type. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified window is used as the target for theme and backdrop type changes and updates, and the specified theme and backdrop type are applied as the initial theme and backdrop type for the window. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance. 
        /// </remarks>
        public ThemeManager(Window window, ElementTheme theme, BackdropType backdropType, bool forceBackdrop = false)
        {
            InternalInitialize(window, theme, backdropType, forceBackdrop);
        }

        /// <summary>
        /// Initializes the ThemeManager instance with the specified initial theme and backdrop type.
        /// </summary>
        /// <param name="theme">The initial theme to apply to the window.</param>
        /// <param name="backdropType">The initial backdrop type to apply to the window.</param>
        /// <param name="forceBackdrop">force backdrop type, saved backdrop will be ignored.</param>
        /// <returns>The initialized ThemeManager instance.</returns>
        /// <remarks>
        /// This method initializes the ThemeManager instance with the specified initial theme and initial backdrop type, and returns it. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified theme and backdrop type are applied as the initial theme and backdrop type for the entire application. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance. 
        /// </remarks>
        public static ThemeManager Initialize(ElementTheme theme, BackdropType backdropType, bool forceBackdrop = false)
        {
            if (Instance == null)
            {
                instance = new ThemeManager(null, theme, backdropType, forceBackdrop);
            }
            else
            {
                instance.InternalInitialize(null, theme, backdropType, forceBackdrop);
            }
            return Instance;
        }

        /// <summary>
        /// Initializes the ThemeManager instance with the specified window, initial theme, and backdrop type.
        /// </summary>
        /// <param name="window">The window to apply the theme and backdrop type to.</param>
        /// <param name="theme">The initial theme to apply to the window.</param>
        /// <param name="backdropType">The initial backdrop type to apply to the window.</param>
        /// <param name="forceBackdrop">force backdrop type, saved backdrop will be ignored.</param>
        /// <returns>The initialized ThemeManager instance.</returns>
        /// <remarks>
        /// This method initializes the ThemeManager instance with the specified window, initial theme, and initial backdrop type, and returns it. The ThemeManager is used to manage the application's theme and UI settings, and can be used to apply themes to specific windows or to the entire application. The specified window is used as the target for theme and backdrop type changes and updates, and the specified theme and backdrop type are applied as the initial theme and backdrop type for the window. The Initialize method must be called before the ThemeManager can be used to change themes or perform other operations related to the application's visual style and appearance. 
        /// </remarks>
        public static ThemeManager Initialize(Window window, ElementTheme theme, BackdropType backdropType, bool forceBackdrop = false)
        {
            if (Instance == null)
            {
                instance = new ThemeManager(window, theme, backdropType, forceBackdrop);
            }
            else
            {
                instance.InternalInitialize(window, theme, backdropType, forceBackdrop);
            }
            return Instance;
        }

        #endregion

    }
}
