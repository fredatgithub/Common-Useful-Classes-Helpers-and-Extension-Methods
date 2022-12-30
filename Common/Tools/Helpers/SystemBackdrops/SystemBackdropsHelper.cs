using WinRT; // required to support Window.As<ICompositionSupportsSystemBackdrop>()

namespace SettingsUI.Helpers;
public class SystemBackdropsHelper
{
    private Window window;
    private WindowsSystemDispatcherQueueHelper m_wsdqHelper;
    private BackdropType m_currentBackdrop;
    private Microsoft.UI.Composition.SystemBackdrops.MicaController m_micaController;
    private Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController m_acrylicController;
    private Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration m_configurationSource;
    public SystemBackdropsHelper(Window window)
    {
        this.window = window;
        ((FrameworkElement) this.window.Content).RequestedTheme = ThemeHelper.RootTheme;

        m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
        m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

        SetBackdrop(BackdropType.Mica);
    }

    /// <summary>
    /// Reset to default color. If the requested type is supported, we'll update to that.
    /// Note: This completely removes any previous controller to reset to the default state.
    /// </summary>
    /// <param name="type"></param>
    public void SetBackdrop(BackdropType type)
    {
        m_currentBackdrop = BackdropType.DefaultColor;
        if (m_micaController != null)
        {
            m_micaController.Dispose();
            m_micaController = null;
        }
        if (m_acrylicController != null)
        {
            m_acrylicController.Dispose();
            m_acrylicController = null;
        }
        this.window.Activated -= Window_Activated;
        this.window.Closed -= Window_Closed;
        ((FrameworkElement) this.window.Content).ActualThemeChanged -= Window_ThemeChanged;
        m_configurationSource = null;

        if (type == BackdropType.Mica)
        {
            if (TrySetMicaBackdrop())
            {
                m_currentBackdrop = type;
            }
            else
            {
                // Mica isn't supported. Try Acrylic.
                type = BackdropType.DesktopAcrylic;
            }
        }
        if (type == BackdropType.MicaAlt)
        {
            if (TrySetMicaAltBackdrop())
            {
                m_currentBackdrop = type;
            }
            else
            {
                // Mica isn't supported. Try Acrylic.
                type = BackdropType.DesktopAcrylic;
            }
        }
        if (type == BackdropType.DesktopAcrylic)
        {
            if (TrySetAcrylicBackdrop())
            {
                m_currentBackdrop = type;
            }
            else
            {
                // Acrylic isn't supported, so take the next option, which is DefaultColor, which is already set.
            }
        }
    }
    private bool TrySetMicaBackdrop(bool isMicaAlt)
    {
        if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
        {
            // Hooking up the policy object
            m_configurationSource = new Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration();
            this.window.Activated += Window_Activated;
            this.window.Closed += Window_Closed;
            ((FrameworkElement) this.window.Content).ActualThemeChanged += Window_ThemeChanged;

            // Initial configuration state.
            m_configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            m_micaController = new Microsoft.UI.Composition.SystemBackdrops.MicaController();

            if (isMicaAlt)
            {
                m_micaController.Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt;
            }
            else
            {
                m_micaController.Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.Base;
            }

            // Enable the system backdrop.
            // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
            m_micaController.AddSystemBackdropTarget(this.window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
            return true; // succeeded
        }

        return false; // Mica is not supported on this system
    }
    public bool TrySetMicaAltBackdrop()
    {
        return TrySetMicaBackdrop(true);
    }
    public bool TrySetMicaBackdrop()
    {
        return TrySetMicaBackdrop(false);
    }

    public bool TrySetAcrylicBackdrop()
    {
        if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
        {
            // Hooking up the policy object
            m_configurationSource = new Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration();
            this.window.Activated += Window_Activated;
            this.window.Closed += Window_Closed;
            ((FrameworkElement) this.window.Content).ActualThemeChanged += Window_ThemeChanged;

            // Initial configuration state.
            m_configurationSource.IsInputActive = true;
            SetConfigurationSourceTheme();

            m_acrylicController = new Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController();

            // Enable the system backdrop.
            // Note: Be sure to have "using WinRT;" to support the Window.As<...>() call.
            m_acrylicController.AddSystemBackdropTarget(this.window.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
            m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
            return true; // succeeded
        }

        return false; // Acrylic is not supported on this system
    }

    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        // Make sure any Mica/Acrylic controller is disposed so it doesn't try to
        // use this closed window.
        if (m_micaController != null)
        {
            m_micaController.Dispose();
            m_micaController = null;
        }
        if (m_acrylicController != null)
        {
            m_acrylicController.Dispose();
            m_acrylicController = null;
        }
        this.window.Activated -= Window_Activated;
        m_configurationSource = null;
    }

    private void Window_ThemeChanged(FrameworkElement sender, object args)
    {
        if (m_configurationSource != null)
        {
            SetConfigurationSourceTheme();
        }
    }

    public void SetConfigurationSourceTheme()
    {
        switch (((FrameworkElement) this.window.Content).ActualTheme)
        {
            case ElementTheme.Dark: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark; break;
            case ElementTheme.Light: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light; break;
            case ElementTheme.Default: m_configurationSource.Theme = Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Default; break;
        }
    }

    public void ChangeSystemBackdropType()
    {
        BackdropType newType;
        switch (m_currentBackdrop)
        {
            case BackdropType.Mica: newType = BackdropType.MicaAlt; break;
            case BackdropType.MicaAlt: newType = BackdropType.DesktopAcrylic; break;
            case BackdropType.DesktopAcrylic: newType = BackdropType.DefaultColor; break;
            default:
            case BackdropType.DefaultColor: newType = BackdropType.Mica; break;
        }
        SetBackdrop(newType);
    }

    public BackdropType GetSystemBackdropType()
    {
        return m_currentBackdrop;
    }
}
