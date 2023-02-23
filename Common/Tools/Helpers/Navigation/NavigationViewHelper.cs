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
    private Type @this;
    private AutoSuggestBox controlsSearchBox;
    private Type settingsPage;
    private Type defaultPage;
    private MenuFlyout menuFlyout;
    private IncludedInBuildMode includedInBuildMode;
    private string jsonFileRelativePath;
    private string itemNotFoundString;
    private string itemNotFoundImage;

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

    private void InternalInitialize(string JsonFileRelativePath, Type currentPage, Frame frame, NavigationView navigationView)
    {
        NavigationViewControl = navigationView;
        rootFrame = frame;
        @this = currentPage;
        _navHelper = new RootFrameNavigationHelper(frame, navigationView);
        jsonFileRelativePath = JsonFileRelativePath;
        NavigationViewControl.Loaded += OnNavigationViewControlLoaded;
        NavigationViewControl.SelectionChanged += OnNavigationViewSelectionChanged;

        AddNavigationMenuItems();
    }

    public NavigationViewHelper Initialize(string JsonFileRelativePath, Type currentPage, Frame frame, NavigationView navigationView)
    {
        InternalInitialize(JsonFileRelativePath, currentPage, frame, navigationView);
        return this;
    }

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
    public NavigationViewHelper WithDefaultPage(Type defaultPage)
    {
        this.defaultPage = defaultPage;
        return this;
    }
    public NavigationViewHelper WithIncludedInBuildMode(IncludedInBuildMode includedInBuildMode)
    {
        this.includedInBuildMode = includedInBuildMode;
        return this;
    }
    public NavigationViewHelper WithSettingsPage(Type settingsPage)
    {
        this.settingsPage = settingsPage;
        return this;
    }

    public NavigationViewHelper WithMenuFlyout(MenuFlyout menuFlyout)
    {
        this.menuFlyout = menuFlyout;
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
    private async void AddNavigationMenuItems()
    {
        await ControlInfoDataSource.Instance.GetGroupsAsync(jsonFileRelativePath);

        foreach (var group in ControlInfoDataSource.Instance.Groups.OrderBy(i => i.Title).Where(i => !i.IsSpecialSection))
        {
            var itemGroup = new NavigationViewItem() { Content = group.Title, Tag = group.UniqueId, DataContext = group, Icon = GetIcon(group.ImageIconPath) };

            if (menuFlyout != null)
            {
                itemGroup.ContextFlyout = menuFlyout;
            }

            AutomationProperties.SetName(itemGroup, group.Title);

            foreach (var item in group.Items)
            {
                var itemInGroup = new NavigationViewItem() { IsEnabled = item.IncludedInBuild, Content = item.Title, Tag = item.UniqueId, DataContext = item };

                if (menuFlyout != null)
                {
                    itemInGroup.ContextFlyout = menuFlyout;
                }
                itemGroup.MenuItems.Add(itemInGroup);
                AutomationProperties.SetName(itemInGroup, item.Title);
            }

            NavigationViewControl.MenuItems.Add(itemGroup);
        }
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
        if (defaultPage != null)
        {
            Navigate(defaultPage);
        }
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
                var noResultsItem = new ControlInfoDataItem("", itemNotFoundString, "", "", "", itemNotFoundImage, "", "", "", "", false, false, false, false);

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
