using Microsoft.UI.Xaml.Input;

namespace WinUICommunity.Common.ViewModel;

public class ShellViewModel : Observable
{
    private readonly KeyboardAccelerator altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu);

    private readonly KeyboardAccelerator backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack);

    private bool isBackEnabled;
    private bool enumerateMenuItemsOnItemInvoke;
    private IList<KeyboardAccelerator> keyboardAccelerators;
    private IEnumerable<NavigationViewItem> menuItems;
    private NavigationView navigationView;
    private Type settingsPage;
    private Type defaultPage;
    private NavigationViewItem selected;
    private AutoSuggestBox autoSuggestBox;
    private ICommand loadedCommand;
    private ICommand itemInvokedCommand;
    private ICommand autoSuggestBoxTextChangedCommand;
    private ICommand autoSuggestBoxQuerySubmittedCommand;

    public bool IsBackEnabled
    {
        get { return isBackEnabled; }
        set { Set(ref isBackEnabled, value); }
    }

    public NavigationViewItem Selected
    {
        get { return selected; }
        set { Set(ref selected, value); }
    }

    public ICommand LoadedCommand => loadedCommand ??= new RelayCommand(OnLoaded);
    public ICommand ItemInvokedCommand => itemInvokedCommand ??= new RelayCommand<NavigationViewItemInvokedEventArgs>(OnItemInvoked);
    public ICommand AutoSuggestBoxTextChangedCommand => autoSuggestBoxTextChangedCommand ??= new RelayCommand<AutoSuggestBoxTextChangedEventArgs>(OnAutoSuggestBoxTextChanged);
    public ICommand AutoSuggestBoxQuerySubmittedCommand => autoSuggestBoxQuerySubmittedCommand ??= new RelayCommand<AutoSuggestBoxQuerySubmittedEventArgs>(OnAutoSuggestBoxQuerySubmitted);

    private void InternalInitialize(Frame frame, NavigationView navigationView)
    {
        this.navigationView = navigationView;
        NavigationService.Frame = frame;
        NavigationService.NavigationFailed += Frame_NavigationFailed;
        NavigationService.Navigated += Frame_Navigated;
        this.navigationView.BackRequested += OnBackRequested;
    }

    /// <summary>
    /// Initialize ShellViewModel
    /// </summary>
    /// <param name="frame"></param>
    /// <param name="navigationView"></param>
    /// <returns></returns>
    public ShellViewModel InitializeNavigation(Frame frame, NavigationView navigationView)
    {
        InternalInitialize(frame, navigationView);
        return this;
    }

    /// <summary>
    /// Setting Page for NavigationView Setting item
    /// </summary>
    /// <param name="settingsPage"></param>
    /// <returns></returns>
    public ShellViewModel WithSettingsPage(Type settingsPage)
    {
        this.settingsPage = settingsPage;
        return this;
    }

    public ShellViewModel WithAutoSuggestBox(AutoSuggestBox autoSuggestBox)
    {
        this.autoSuggestBox = autoSuggestBox;
        return this;
    }

    /// <summary>
    /// Keyboard accelerators are added here to avoid showing 'Alt + left' tooltip on the page.
    /// More info on tracking issue https://github.com/Microsoft/microsoft-ui-xaml/issues/8
    /// </summary>
    /// <param name="keyboardAccelerators"></param>
    /// <returns></returns>
    public ShellViewModel WithKeyboardAccelerator(IList<KeyboardAccelerator> keyboardAccelerators)
    {
        this.keyboardAccelerators = keyboardAccelerators;
        return this;
    }

    public ShellViewModel WithDefaultPage(Type defaultPage)
    {
        this.defaultPage = defaultPage;
        return this;
    }

    public ShellViewModel WithEnumerateMenuItemsOnItemInvoke(bool enumerateMenuItemsOnItemInvoke = true)
    {
        this.enumerateMenuItemsOnItemInvoke = enumerateMenuItemsOnItemInvoke;
        return this;
    }

    private static KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, VirtualKeyModifiers? modifiers = null)
    {
        var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
        if (modifiers.HasValue)
        {
            keyboardAccelerator.Modifiers = modifiers.Value;
        }

        keyboardAccelerator.Invoked += OnKeyboardAcceleratorInvoked;
        return keyboardAccelerator;
    }

    private static void OnKeyboardAcceleratorInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        var result = NavigationService.GoBack();
        args.Handled = result;
    }

    public async void OnLoaded()
    {
        // Keyboard accelerators are added here to avoid showing 'Alt + left' tooltip on the page.
        // More info on tracking issue https://github.com/Microsoft/microsoft-ui-xaml/issues/8
        if (keyboardAccelerators != null)
        {
            keyboardAccelerators.Add(altLeftKeyboardAccelerator);
            keyboardAccelerators.Add(backKeyboardAccelerator);
        }
        await Task.CompletedTask.ConfigureAwait(false);

        if (defaultPage != null)
        {
            NavigationService.Navigate(defaultPage);
        }

        menuItems = GetAllMenuItems();
    }
    private IEnumerable<NavigationViewItem> GetAllMenuItems()
    {
        var _menuItems = EnumerateNavigationViewItem(navigationView.MenuItems);
        var footer = EnumerateNavigationViewItem(navigationView.FooterMenuItems);
        return _menuItems.Concat(footer);
    }
    public void OnItemInvoked(NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked == true && settingsPage != null)
        {
            NavigationService.Navigate(settingsPage);
        }
        else if (args.InvokedItemContainer != null)
        {
            if (enumerateMenuItemsOnItemInvoke)
            {
                menuItems = GetAllMenuItems();
            }

            var item = menuItems.FirstOrDefault(menuItem => (string)menuItem.Content == (string)args.InvokedItem);
            if (item != null)
            {
                var pageType = item.GetValue(NavHelper.NavigateToProperty) as Type;
                NavigationService.Navigate(pageType);
            }
        }
    }
    private IEnumerable<NavigationViewItem> EnumerateNavigationViewItem(IList<object> parent)
    {
        if (parent != null)
        {
            foreach (var g in parent)
            {
                yield return (NavigationViewItem)g;

                foreach (var sub in EnumerateNavigationViewItem(((NavigationViewItem)g).MenuItems))
                {
                    yield return sub;
                }
            }
        }
    }
    private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        NavigationService.GoBack();
    }

    private void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw e.Exception;
    }

    private void Frame_Navigated(object sender, NavigationEventArgs e)
    {
        IsBackEnabled = NavigationService.CanGoBack;
        if (e.SourcePageType == settingsPage)
        {
            Selected = (NavigationViewItem)navigationView.SettingsItem;
        }
        else if (e.SourcePageType != null)
        {
            Selected = navigationView.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(menuItem => IsMenuItemForPageType(menuItem, e.SourcePageType));
        }
    }

    private static bool IsMenuItemForPageType(NavigationViewItem menuItem, Type sourcePageType)
    {
        var pageType = menuItem.GetValue(NavHelper.NavigateToProperty) as Type;
        return pageType == sourcePageType;
    }

    public void OnAutoSuggestBoxTextChanged(AutoSuggestBoxTextChangedEventArgs args)
    {
        if (autoSuggestBox == null)
            throw new NullReferenceException("AutoSuggestBox is null, please initialize ShellViewModel with a AutoSuggestBox.");

        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var suggestions = new List<string>();
            var history = navigationView.MenuItems.OfType<NavigationViewItem>().ToList();

            var querySplit = autoSuggestBox.Text.Split(' ');
            var matchingItems = history.Where(
                item =>
                {
                    var flag = true;
                    foreach (var queryToken in querySplit)
                    {
                        if (item.Content.ToString().IndexOf(queryToken, StringComparison.CurrentCultureIgnoreCase) < 0)
                        {
                            flag = false;
                        }

                    }
                    return flag;
                });

            foreach (var item in matchingItems)
            {
                suggestions.Add(item.Content.ToString());
            }
            if (suggestions.Count > 0)
            {
                autoSuggestBox.ItemsSource = suggestions;
            }
            else
            {
                autoSuggestBox.ItemsSource = new string[] { "No result found" };
            }
        }
    }

    public void OnAutoSuggestBoxQuerySubmitted(AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion != null)
        {
            var item = args.ChosenSuggestion as string;
            Selected = navigationView.MenuItems
                        .OfType<NavigationViewItem>()
                        .FirstOrDefault(menuItem => (string)menuItem.Content == item);

            var pageType = Selected.GetValue(NavHelper.NavigateToProperty) as Type;
            NavigationService.Navigate(pageType);
        }
    }
}
