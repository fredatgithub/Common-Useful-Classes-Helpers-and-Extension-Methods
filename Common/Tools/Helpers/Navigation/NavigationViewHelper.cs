using Microsoft.UI.Xaml.Automation;
using WinUICommunity.Shared.DataModel;
using WinUICommunity.Shared.Navigation;

namespace WinUICommunity.Common.Helpers;

public class NavigationViewHelper
{
    private static NavigationViewHelper Instance { get; set; }
    private Action NavigationViewLoaded { get; set; }

    private RootFrameNavigationHelper _navHelper;
    private NavigationView NavigationViewControl;
    private Frame rootFrame;
    private AutoSuggestBox controlsSearchBox;
    private Type settingsPage;
    private Type defaultPage;
    private MenuFlyout menuFlyout;
    private IncludedInBuildMode includedInBuildMode;
    private string jsonFileRelativePath;
    private string itemNotFoundString;
    private string itemNotFoundImage;
    private bool hasInfoBadge = false;
    public static NavigationViewHelper GetCurrent()
    {
        if (Instance == null)
        {
            Instance = new NavigationViewHelper();
        }
        return Instance;
    }

    private Microsoft.UI.Xaml.Controls.NavigationView NavigationView
    {
        get { return NavigationViewControl; }
    }

    private void InternalInitialize(string JsonFileRelativePath, Frame frame, NavigationView navigationView)
    {
        NavigationViewControl = navigationView;
        rootFrame = frame;
        _navHelper = new RootFrameNavigationHelper(frame, navigationView);
        jsonFileRelativePath = JsonFileRelativePath;
        NavigationViewControl.Loaded += OnNavigationViewControlLoaded;
        NavigationViewControl.SelectionChanged += OnNavigationViewSelectionChanged;

        AddNavigationMenuItems();
    }

    /// <summary>
    /// Use This Method in Last Option
    /// </summary>
    /// <param name="JsonFileRelativePath">DataModel/ControlInfoData.json</param>
    /// <param name="frame">Frame</param>
    /// <param name="navigationView">NavigationView</param>
    /// <returns></returns>
    public NavigationViewHelper Build(string JsonFileRelativePath, Frame frame, NavigationView navigationView)
    {
        InternalInitialize(JsonFileRelativePath, frame, navigationView);
        return this;
    }

    /// <summary>
    /// You can Search items in AutoSuggestBox
    /// </summary>
    /// <param name="autoSuggestBox">AutoSuggestBox</param>
    /// <param name="itemNotFoundImage">Item Not Found Image. Optional</param>
    /// <param name="itemNotFoundString">Item Not Found String. Optional</param>
    /// <returns></returns>
    public NavigationViewHelper WithAutoSuggestBox(AutoSuggestBox autoSuggestBox, string itemNotFoundImage = null, string itemNotFoundString = "No results found")
    {
        controlsSearchBox = autoSuggestBox;
        this.itemNotFoundString = itemNotFoundString;
        if (itemNotFoundImage is null)
        {
            this.itemNotFoundImage = "null";
        }
        else
        {
            this.itemNotFoundImage = itemNotFoundImage;
        }
        controlsSearchBox.TextChanged += OnControlsSearchBoxTextChanged;
        return this;
    }

    /// <summary>
    /// Open a Page as a Default Page
    /// </summary>
    /// <param name="defaultPage">Page</param>
    /// <returns></returns>
    public NavigationViewHelper WithDefaultPage(Type defaultPage)
    {
        this.defaultPage = defaultPage;
        return this;
    }

    /// <summary>
    /// How Should we Check if Item exist? based on a json property? or Real Check?
    /// </summary>
    /// <param name="includedInBuildMode">IncludedInBuildMode enum</param>
    /// <returns></returns>
    public NavigationViewHelper WithIncludedInBuildMode(IncludedInBuildMode includedInBuildMode)
    {
        this.includedInBuildMode = includedInBuildMode;
        return this;
    }

    /// <summary>
    /// Define a SettingsPage so when user click on Setting icon will be navigated to desired page.
    /// </summary>
    /// <param name="settingsPage">Page</param>
    /// <returns></returns>
    public NavigationViewHelper WithSettingsPage(Type settingsPage)
    {
        this.settingsPage = settingsPage;
        return this;
    }

    /// <summary>
    /// Add a MenuFlyout when user Right-Click on NavigationViewItem
    /// </summary>
    /// <param name="menuFlyout"></param>
    /// <returns></returns>
    public NavigationViewHelper WithMenuFlyout(MenuFlyout menuFlyout)
    {
        this.menuFlyout = menuFlyout;
        return this;
    }

    /// <summary>
    /// Read InfoBadge Value from json and Show in NavigationViewItem
    /// </summary>
    /// <returns></returns>
    public NavigationViewHelper WithInfoBadge()
    {
        this.hasInfoBadge = true;
        return this;
    }

    // Wraps a call to rootFrame.Navigate to give the Page a way to know which NavigationRootPage is navigating.
    // Please call this function rather than rootFrame.Navigate to navigate the rootFrame.
    public void Navigate(
        Type pageType,
        object targetPageArguments = null,
        Microsoft.UI.Xaml.Media.Animation.NavigationTransitionInfo navigationTransitionInfo = null)
    {
        NavigationArgs args = new NavigationArgs();
        args.JsonRelativeFilePath = jsonFileRelativePath;
        args.IncludedInBuildMode = includedInBuildMode;
        args.NavigationView = NavigationView;
        args.Parameter = targetPageArguments;
        rootFrame.Navigate(pageType, args, navigationTransitionInfo);
    }

    public void OnNavigationViewSelectionChanged(NavigationViewSelectionChangedEventArgs args)
    {
        if (!args.IsSettingsSelected)
        {
            var selectedItem = (args.SelectedItem as NavigationViewItem).DataContext;
            var item = selectedItem as ControlInfoDataItem;
            var itemGroup = selectedItem as ControlInfoDataItem;
            Type pageType = null;
            if (selectedItem is ControlInfoDataItem)
            {
                Assembly assembly = Assembly.Load(item.ApiNamespace);
                if (assembly is not null)
                {
                    pageType = assembly.GetType(item.UniqueId);
                }
            }
            else
            {
                Assembly assembly = Assembly.Load(itemGroup.ApiNamespace);
                if (assembly is not null)
                {
                    pageType = assembly.GetType(itemGroup.UniqueId);
                }
            }

            Navigate(pageType);
        }
    }

    public void OnNavigationViewSelectionChanged(NavigationViewSelectionChangedEventArgs args, Type sectionPage)
    {
        if (!args.IsSettingsSelected)
        {
            var selectedItem = (args.SelectedItem as NavigationViewItem).DataContext;
            var item = selectedItem as ControlInfoDataItem;
            var itemGroup = selectedItem as ControlInfoDataItem;
            Type pageType = null;
            if (selectedItem is ControlInfoDataItem)
            {
                Assembly assembly = Assembly.Load(item.ApiNamespace);
                if (assembly is not null)
                {
                    pageType = assembly.GetType(item.UniqueId);
                    Navigate(pageType);
                }
            }
            else
            {
                Navigate(sectionPage, itemGroup.UniqueId);
            }
        }
    }

    /// <summary>
    /// Navigate to SectionPage and ItemPage
    /// </summary>
    /// <param name="args"></param>
    /// <param name="sectionPage">navigate to sectionPage if page is null</param>
    /// <param name="itemPage">navigate to itemPage if page is null</param>
    public void OnNavigationViewSelectionChanged(NavigationViewSelectionChangedEventArgs args, Type sectionPage, Type itemPage)
    {
        if (!args.IsSettingsSelected)
        {
            var selectedItem = args.SelectedItemContainer;
            if (selectedItem.DataContext is ControlInfoDataGroup)
            {
                var itemId = ((ControlInfoDataGroup)selectedItem.DataContext).UniqueId;
                Navigate(sectionPage, itemId);
            }
            else if (selectedItem.DataContext is ControlInfoDataItem)
            {
                var item = (ControlInfoDataItem)selectedItem.DataContext;
                Navigate(itemPage, item.UniqueId);
            }
        }
    }

    public void AutoSuggestBoxQuerySubmitted(AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion != null && args.ChosenSuggestion is ControlInfoDataItem)
        {
            var infoDataItem = args.ChosenSuggestion as ControlInfoDataItem;
            var hasChangedSelection = EnsureItemIsVisibleInNavigation(infoDataItem.Title);

            // In case the menu selection has changed, it means that it has triggered
            // the selection changed event, that will navigate to the page already
            if (!hasChangedSelection)
            {
                string pageString = infoDataItem.UniqueId;
                Type page = null;
                Assembly assembly = Assembly.Load(infoDataItem.ApiNamespace);
                if (assembly is not null)
                {
                    page = assembly.GetType(pageString);
                    Navigate(page);
                }
            }
        }
    }

    /// <summary>
    /// Navigate to ItemPage 
    /// </summary>
    /// <param name="args"></param>
    /// <param name="itemPage">navigate to itemPage if page is null</param>
    public void AutoSuggestBoxQuerySubmitted(AutoSuggestBoxQuerySubmittedEventArgs args, Type itemPage)
    {
        if (args.ChosenSuggestion != null && args.ChosenSuggestion is ControlInfoDataItem)
        {
            var infoDataItem = args.ChosenSuggestion as ControlInfoDataItem;
            var hasChangedSelection = EnsureItemIsVisibleInNavigation(infoDataItem.Title);

            // In case the menu selection has changed, it means that it has triggered
            // the selection changed event, that will navigate to the page already
            if (!hasChangedSelection)
            {
                Navigate(itemPage, infoDataItem.UniqueId);
            }
        }
    }

    private async void AddNavigationMenuItems()
    {
        await ControlInfoDataSource.Instance.GetGroupsAsync(jsonFileRelativePath);
        
        foreach (var group in ControlInfoDataSource.Instance.Groups.OrderBy(i => i.Title).Where(i => !i.IsSpecialSection && !i.HideGroup))
        {
            if (ControlInfoDataSource.Instance.Groups.Count == 1 && group.IsSingleGroup)
            {
                foreach (var item in group.Items.Where(i => !i.HideNavigationViewItem))
                {
                    var itemInGroup = new NavigationViewItem() { IsEnabled = item.IncludedInBuild, Icon = GetIcon(item.ImageIconPath), Content = item.Title, Tag = item.UniqueId, DataContext = item };

                    if (menuFlyout != null)
                    {
                        itemInGroup.ContextFlyout = menuFlyout;
                    }
                    
                    itemInGroup.InfoBadge = GetInfoBadge(item);
                    NavigationViewControl.MenuItems.Add(itemInGroup);
                    AutomationProperties.SetName(itemInGroup, item.Title);
                }
            }
            else
            {
                var itemGroup = new NavigationViewItem() { Content = group.Title, IsExpanded = group.IsExpanded, Tag = group.UniqueId, DataContext = group, Icon = GetIcon(group.ImageIconPath) };

                if (menuFlyout != null)
                {
                    itemGroup.ContextFlyout = menuFlyout;
                }

                AutomationProperties.SetName(itemGroup, group.Title);

                itemGroup.InfoBadge = GetInfoBadge(group);
                foreach (var item in group.Items.Where(i => !i.HideNavigationViewItem))
                {
                    var itemInGroup = new NavigationViewItem() { IsEnabled = item.IncludedInBuild, Icon = GetIcon(item.ImageIconPath), Content = item.Title, Tag = item.UniqueId, DataContext = item };

                    if (menuFlyout != null)
                    {
                        itemInGroup.ContextFlyout = menuFlyout;
                    }
                    
                    itemInGroup.InfoBadge = GetInfoBadge(item);
                    itemGroup.MenuItems.Add(itemInGroup);
                    AutomationProperties.SetName(itemInGroup, item.Title);
                }

                NavigationViewControl.MenuItems.Add(itemGroup);
            }
        }

        if (defaultPage != null)
        {
            Navigate(defaultPage);
        }
    }
    private InfoBadge GetInfoBadge(dynamic controlInfoData)
    {
        if (controlInfoData.InfoBadge is not null)
        {
            bool hideNavigationViewItemBadge = controlInfoData.InfoBadge.HideNavigationViewItemBadge;
            string value = controlInfoData.InfoBadge.BadgeValue;
            string style = controlInfoData.InfoBadge.BadgeStyle;
            bool hasValue = !string.IsNullOrEmpty(value);
            if (style.Contains("Dot", StringComparison.OrdinalIgnoreCase) || style.Contains("Icon", StringComparison.OrdinalIgnoreCase))
            {
                hasValue = true;
            }
            if (hasInfoBadge && !hideNavigationViewItemBadge && hasValue)
            {
                int badgeValue = Convert.ToInt32(controlInfoData.InfoBadge.BadgeValue);
                int width = controlInfoData.InfoBadge.BadgeWidth;
                int height = controlInfoData.InfoBadge.BadgeHeight;

                InfoBadge infoBadge = new InfoBadge
                {
                    Style = Application.Current.Resources[style] as Style
                };
                switch (style.ToLower())
                {
                    case string s when s.Contains("value"):
                        infoBadge.Value = badgeValue;
                        break;
                    case string s when s.Contains("icon"):
                        infoBadge.IconSource = GetIconSource(controlInfoData.InfoBadge);
                        break;
                }

                if (width > 0 && height > 0)
                {
                    infoBadge.Width = width;
                    infoBadge.Height = height;
                }

                return infoBadge;
            }
        }
        return null;
    }

    private IconSource GetIconSource(ControlInfoBadge infoBadge)
    {
        string symbol = infoBadge.BadgeSymbolIcon;
        string image = infoBadge.BadgeBitmapIcon;
        string glyph = infoBadge.BadgeFontIconGlyph;
        string fontName = infoBadge.BadgeFontIconFontName;
        
        if (!string.IsNullOrEmpty(symbol))
        {
            return new SymbolIconSource 
            { 
                Symbol = GeneralHelper.GetEnum<Symbol>(symbol),
                Foreground = Application.Current.Resources["SystemControlForegroundAltHighBrush"] as Brush,
            };
        }

        if (!string.IsNullOrEmpty(image))
        {
            return new BitmapIconSource {
                UriSource = new Uri(image),
                ShowAsMonochrome = false
            };
        }

        if (!string.IsNullOrEmpty(glyph))
        {
            var fontIcon = new FontIconSource 
            { 
                Glyph = GeneralHelper.GetGlyph(glyph), 
                Foreground = Application.Current.Resources["SystemControlForegroundAltHighBrush"] as Brush,
            };
            if (!string.IsNullOrEmpty(fontName))
            {
                fontIcon.FontFamily = new FontFamily(fontName);
            }
            return fontIcon;
        }
        return null;
    }
    private void OnNavigationViewSelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            if (rootFrame.CurrentSourcePageType != settingsPage && settingsPage != null)
            {
                Navigate(settingsPage);
            }
        }
    }

    public IconElement GetIcon(string imagePath)
    {
        return imagePath.ToLowerInvariant().EndsWith(".png") ?
                    new BitmapIcon() { UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute), ShowAsMonochrome = false } :
                    new FontIcon()
                    {
                        Glyph = imagePath
                    };
    }

    private void OnNavigationViewControlLoaded(object sender, RoutedEventArgs e)
    {
        // Delay necessary to ensure NavigationView visual state can match navigation
        Task.Delay(500).ContinueWith(_ => NavigationViewLoaded?.Invoke(), TaskScheduler.FromCurrentSynchronizationContext());
    }
    private void OnControlsSearchBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var suggestions = new List<ControlInfoDataItem>();

            var querySplit = sender.Text.Split(" ");
            foreach (var group in ControlInfoDataSource.Instance.Groups)
            {
                var matchingItems = group.Items.Where(
                    item =>
                    {
                        // Idea: check for every word entered (separated by space) if it is in the name, 
                        // e.g. for query "split button" the only result should "SplitButton" since its the only query to contain "split" and "button"
                        // If any of the sub tokens is not in the string, we ignore the item. So the search gets more precise with more words
                        bool flag = item.IncludedInBuild;
                        foreach (string queryToken in querySplit)
                        {
                            // Check if token is not in string
                            if (item.Title.IndexOf(queryToken, StringComparison.CurrentCultureIgnoreCase) < 0)
                            {
                                // Token is not in string, so we ignore this item.
                                flag = false;
                            }
                        }
                        return flag;
                    });
                foreach (var item in matchingItems)
                {
                    suggestions.Add(item);
                }
            }
            if (suggestions.Count > 0)
            {
                controlsSearchBox.ItemsSource = suggestions.OrderByDescending(i => i.Title.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase)).ThenBy(i => i.Title);
            }
            else
            {
                // Create a single "No results found" item
                var noResultsItem = new ControlInfoDataItem("", itemNotFoundString, "", "", "", itemNotFoundImage, itemNotFoundImage, "", "", "", false, false, false, false, false, false, null);

                // Add the item to a new list of suggestions
                var noResultsList = new List<ControlInfoDataItem>();
                noResultsList.Add(noResultsItem);

                // Set the ItemsSource of the AutoSuggestBox to the list of suggestions
                controlsSearchBox.ItemsSource = noResultsList;
            }
        }
    }

    public void EnsureNavigationSelection(string id)
    {
        foreach (object rawGroup in this.NavigationView.MenuItems)
        {
            if (rawGroup is NavigationViewItem group)
            {
                foreach (object rawItem in group.MenuItems)
                {
                    if (rawItem is NavigationViewItem item)
                    {
                        if ((string)item.Tag == id)
                        {
                            group.IsExpanded = true;
                            NavigationView.SelectedItem = item;
                            item.IsSelected = true;
                            return;
                        }
                        else if (item.MenuItems.Count > 0)
                        {
                            foreach (var rawInnerItem in item.MenuItems)
                            {
                                if (rawInnerItem is NavigationViewItem innerItem)
                                {
                                    if ((string)innerItem.Tag == id)
                                    {
                                        group.IsExpanded = true;
                                        item.IsExpanded = true;
                                        NavigationView.SelectedItem = innerItem;
                                        innerItem.IsSelected = true;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public bool EnsureItemIsVisibleInNavigation(string name)
    {
        bool changedSelection = false;
        foreach (object rawItem in NavigationView.MenuItems)
        {
            // Check if we encountered the separator
            if (!(rawItem is NavigationViewItem))
            {
                // Skipping this item
                continue;
            }

            var item = rawItem as NavigationViewItem;

            // Check if we are this category
            if ((string)item.Content == name)
            {
                NavigationView.SelectedItem = item;
                changedSelection = true;
            }
            // We are not :/
            else
            {
                // Maybe one of our items is?
                if (item.MenuItems.Count != 0)
                {
                    foreach (NavigationViewItem child in item.MenuItems)
                    {
                        if ((string)child.Content == name)
                        {
                            // We are the item corresponding to the selected one, update selection!

                            // Deal with differences in displaymodes
                            if (NavigationView.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
                            {
                                // In Topmode, the child is not visible, so set parent as selected
                                // Everything else does not work unfortunately
                                NavigationView.SelectedItem = item;
                                item.StartBringIntoView();
                            }
                            else
                            {
                                // Expand so we animate
                                item.IsExpanded = true;
                                // Ensure parent is expanded so we actually show the selection indicator
                                NavigationView.UpdateLayout();
                                // Set selected item
                                NavigationView.SelectedItem = child;
                                child.StartBringIntoView();
                            }
                            // Set to true to also skip out of outer for loop
                            changedSelection = true;
                            // Break out of child iteration for loop
                            break;
                        }
                    }
                }
            }
            // We updated selection, break here!
            if (changedSelection)
            {
                break;
            }
        }
        return changedSelection;
    }
}
