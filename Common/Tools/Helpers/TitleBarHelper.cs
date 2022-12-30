namespace SettingsUI.Helpers;

public static class TitleBarHelper
{
    private static ColumnDefinition _LeftPaddingColumn { get; set; }
    private static ColumnDefinition _IconColumn { get; set; }
    private static ColumnDefinition _TitleColumn { get; set; }
    private static ColumnDefinition _LeftDragColumn { get; set; }
    private static ColumnDefinition _SearchColumn { get; set; }
    private static ColumnDefinition _RightDragColumn { get; set; }
    private static ColumnDefinition _RightPaddingColumn { get; set; }
    private static Grid _AppTitleBar { get; set; }
    private static TextBlock _TitleTextBlock { get; set; }
    private static Window _MainWindowObject { get; set; }
    private static AppWindow m_AppWindow { get; set; }

    public static void Initialize(Window MainWindowObject, TextBlock TitleTextBlock, Grid AppTitleBar, ColumnDefinition LeftPaddingColumn, ColumnDefinition IconColumn, ColumnDefinition TitleColumn, ColumnDefinition LeftDragColumn, ColumnDefinition SearchColumn, ColumnDefinition RightDragColumn, ColumnDefinition RightPaddingColumn)
    {
        _LeftPaddingColumn = LeftPaddingColumn;
        _IconColumn = IconColumn;
        _TitleColumn = TitleColumn;
        _LeftDragColumn = LeftDragColumn;
        _SearchColumn = SearchColumn;
        _RightDragColumn = RightDragColumn;
        _RightPaddingColumn = RightPaddingColumn;
        _AppTitleBar = AppTitleBar;
        _TitleTextBlock = TitleTextBlock;
        _MainWindowObject = MainWindowObject;

        m_AppWindow = WindowHelper.GetAppWindowForCurrentWindow(_MainWindowObject);

        // Check to see if customization is supported.
        // Currently only supported on Windows 11.
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            var titleBar = m_AppWindow.TitleBar;
            titleBar.ExtendsContentIntoTitleBar = true;
            AppTitleBar.Loaded += AppTitleBar_Loaded;
            AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
        }
        else
        {
            // Title bar customization using these APIs is currently
            // supported only on Windows 11. In other cases, hide
            // the custom title bar element.
            AppTitleBar.Visibility = Visibility.Collapsed;

            // Show alternative UI for any functionality in
            // the title bar, such as search.
        }
    }

    private static void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
    {
        if (AppWindowTitleBar.IsCustomizationSupported())
        {
            SetDragRegionForCustomTitleBar(m_AppWindow);
        }
    }

    private static void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (AppWindowTitleBar.IsCustomizationSupported()
            && m_AppWindow.TitleBar.ExtendsContentIntoTitleBar)
        {
            // Update drag region if the size of the title bar changes.
            SetDragRegionForCustomTitleBar(m_AppWindow);
        }
    }

    public static double GetScaleAdjustment()
    {
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(_MainWindowObject);
        var wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var displayArea = DisplayArea.GetFromWindowId(wndId, DisplayAreaFallback.Primary);
        var hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

        // Get DPI.
        var result = NativeMethods.GetDpiForMonitor(hMonitor, NativeMethods.Monitor_DPI_Type.MDT_Default, out var dpiX, out var _);
        if (result != 0)
        {
            throw new Exception("Could not get DPI for monitor.");
        }

        var scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
        return scaleFactorPercent / 100.0;
    }
    public static void SetDragRegionForCustomTitleBar(AppWindow appWindow)
    {
        if (AppWindowTitleBar.IsCustomizationSupported()
            && appWindow.TitleBar.ExtendsContentIntoTitleBar)
        {
            var scaleAdjustment = GetScaleAdjustment();

            _RightPaddingColumn.Width = new GridLength(appWindow.TitleBar.RightInset / scaleAdjustment);
            _LeftPaddingColumn.Width = new GridLength(appWindow.TitleBar.LeftInset / scaleAdjustment);

            List<Windows.Graphics.RectInt32> dragRectsList = new();

            Windows.Graphics.RectInt32 dragRectL;
            dragRectL.X = (int)((_LeftPaddingColumn.ActualWidth) * scaleAdjustment);
            dragRectL.Y = 0;
            dragRectL.Height = (int)(_AppTitleBar.ActualHeight * scaleAdjustment);
            dragRectL.Width = (int)((_IconColumn.ActualWidth
                                    + _TitleColumn.ActualWidth
                                    + _LeftDragColumn.ActualWidth) * scaleAdjustment);
            dragRectsList.Add(dragRectL);

            Windows.Graphics.RectInt32 dragRectR;
            dragRectR.X = (int)((_LeftPaddingColumn.ActualWidth
                                + _IconColumn.ActualWidth
                                + _TitleTextBlock.ActualWidth
                                + _LeftDragColumn.ActualWidth
                                + _SearchColumn.ActualWidth) * scaleAdjustment);
            dragRectR.Y = 0;
            dragRectR.Height = (int)(_AppTitleBar.ActualHeight * scaleAdjustment);
            dragRectR.Width = (int)(_RightDragColumn.ActualWidth * scaleAdjustment);
            dragRectsList.Add(dragRectR);

            var dragRects = dragRectsList.ToArray();

            appWindow.TitleBar.SetDragRectangles(dragRects);
        }
    }
}
