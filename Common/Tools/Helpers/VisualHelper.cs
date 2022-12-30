namespace WinUICommunity.Common.Helpers;

public static class VisualHelper
{
    public static T GetListViewItem<T>(FrameworkElement frameworkElement) where T : class
    {
        var listViewItem = GetAncestorOfType<ListViewItem>(frameworkElement);
        return listViewItem.Content as T;
    }

    public static T GetAncestorOfType<T>(FrameworkElement child) where T : FrameworkElement
    {
        var parent = VisualTreeHelper.GetParent(child);
        if (parent != null && parent is not T)
            return (T)GetAncestorOfType<T>((FrameworkElement)parent);
        return (T)parent;
    }

    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj != null)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                if (child != null && child is T)
                    yield return (T)child;

                foreach (var childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }
    }
}
