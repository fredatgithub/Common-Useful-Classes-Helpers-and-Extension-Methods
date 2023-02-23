namespace WinUICommunity.Common.Helpers;

public static partial class WindowHelper
{
    /// <summary>
    /// allow the app to find the Window that contains an
    /// arbitrary UIElement (GetWindowForElement).  To do this, we keep track
    /// of all active Windows.  The app code must call WindowHelper.CreateWindow
    /// rather than "new Window" so we can keep track of all the relevant windows.
    /// </summary>
    public static List<Window> ActiveWindows { get { return _activeWindows; } }

    private static List<Window> _activeWindows = new();

    /// <summary>
    /// Get AppWindow For a Window
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static AppWindow GetAppWindowForCurrentWindow(object target)
    {
        return AppWindow.GetFromWindowId(GetWindowIdFromCurrentWindow(target));
    }

    /// <summary>
    /// Get WindowHandle for a Window
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static IntPtr GetWindowHandleForCurrentWindow(object target)
    {
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(target);
        return hWnd;
    }

    /// <summary>
    /// Get WindowId from Window
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static WindowId GetWindowIdFromCurrentWindow(object target)
    {
        var wndId = Win32Interop.GetWindowIdFromWindow(GetWindowHandleForCurrentWindow(target));
        return wndId;
    }

    /// <summary>
    /// allow the app to find the Window that contains an
    /// arbitrary UIElement (GetWindowForElement).  To do this, we keep track
    /// of all active Windows.  The app code must call WindowHelper.CreateWindow
    /// rather than "new Window" so we can keep track of all the relevant windows.
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    public static Window GetWindowForElement(UIElement element)
    {
        if (element.XamlRoot != null)
        {
            foreach (var window in _activeWindows)
            {
                if (element.XamlRoot == window.Content.XamlRoot)
                {
                    return window;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Create a new Window
    /// </summary>
    /// <returns></returns>
    public static Window CreateWindow()
    {
        var newWindow = new Window();
        TrackWindow(newWindow);
        return newWindow;
    }

    /// <summary>
    /// track of all active Windows.  The app code must call WindowHelper.CreateWindow
    /// rather than "new Window" so we can keep track of all the relevant windows.
    /// </summary>
    /// <param name="window"></param>
    public static void TrackWindow(Window window)
    {
        window.Closed += (sender, args) =>
        {
            _activeWindows.Remove(window);
        };
        _activeWindows.Add(window);
    }

    public static void SwitchToThisWindow(object target)
    {
        if (target != null)
        {
            NativeMethods.SwitchToThisWindow(GetWindowHandleForCurrentWindow(target), true);
        }
    }
}
