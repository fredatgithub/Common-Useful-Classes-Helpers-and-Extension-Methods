namespace WinUICommunity.Common.Tools
{
    public partial class ThemeManager
    {
        private Window builderWindow = null;
        private BackdropType backdropType = BackdropType.DefaultColor;
        private ElementTheme theme = ElementTheme.Default;

        private bool hasBackdrop = false;
        private bool hasTheme = false;

        /// <summary>
        /// Sets the Window to be used by the ThemeManager during initialization.
        /// </summary>
        /// <param name="window">The Window to use.</param>
        /// <returns>The ThemeManager instance.</returns>
        /// <remarks>
        /// This method sets the Window to be used by the ThemeManager during initialization. If this method is not called, the ThemeManager will use the application's activeWindow (if one exists) as the default Window. also you should use BuildWithoutWindow instead of Build Method.
        /// </remarks>
        public ThemeManager UseWindow(Window window)
        {
            this.builderWindow = window;
            return this;
        }

        /// <summary>
        /// Sets the BackdropType to be used by the ThemeManager during initialization.
        /// </summary>
        /// <param name="backdropType">The BackdropType to use.</param>
        /// <returns>The ThemeManager instance.</returns>
        /// <remarks>
        /// This method sets the BackdropType to be used by the ThemeManager during initialization.
        /// </remarks>
        public ThemeManager UseBackdrop(BackdropType backdropType)
        {
            this.backdropType = backdropType;
            this.hasBackdrop = true;
            return this;
        }

        /// <summary>
        /// Sets the ElementTheme to be used by the ThemeManager during initialization.
        /// </summary>
        /// <param name="theme">The ElementTheme to use.</param>
        /// <returns>The ThemeManager instance.</returns>
        /// <remarks>
        /// This method sets the ElementTheme to be used by the ThemeManager during initialization. If this method is not called, the ThemeManager will use the default ElementTheme for the system.
        /// </remarks>
        public ThemeManager UseTheme(ElementTheme theme)
        {
            this.theme = theme;
            this.hasTheme = true;
            return this;
        }

        /// <summary>
        /// Builds the ThemeManager instance using the specified settings without Window.
        /// </summary>
        /// <returns>The ThemeManager instance.</returns>
        public ThemeManager BuildWithoutWindow()
        {
            builderWindow = null;
            return InternalBuild();
        }

        /// <summary>
        /// Builds the ThemeManager instance using the specified settings.
        /// </summary>
        /// <returns>The ThemeManager instance.</returns>
        /// <remarks>
        /// This method builds the ThemeManager instance using the specified settings. If no settings were specified, the ThemeManager will use the default settings for the system.
        /// </remarks>
        public ThemeManager Build()
        {
            return InternalBuild();
        }

        private ThemeManager InternalBuild()
        {
            if (builderWindow == null && !hasBackdrop && !hasTheme)
            {
                return new ThemeManager();
            }
            else if (builderWindow == null && hasBackdrop && !hasTheme)
            {
                return new ThemeManager(null, backdropType);
            }
            else if (builderWindow == null && !hasBackdrop && hasTheme)
            {
                return new ThemeManager(null, theme);
            }
            else if (builderWindow == null && hasBackdrop && hasTheme)
            {
                return new ThemeManager(null, theme, backdropType);
            }
            else if (builderWindow != null && !hasBackdrop && !hasTheme)
            {
                return new ThemeManager(builderWindow);
            }
            else if (builderWindow != null && hasBackdrop && !hasTheme)
            {
                return new ThemeManager(builderWindow, backdropType);
            }
            else if (builderWindow != null && !hasBackdrop && hasTheme)
            {
                return new ThemeManager(builderWindow, theme);
            }
            else if (builderWindow != null && hasBackdrop && hasTheme)
            {
                return new ThemeManager(builderWindow, theme, backdropType);
            }
            return this;
        }
    }
}
