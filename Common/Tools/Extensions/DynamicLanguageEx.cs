namespace WinUICommunity.Common.Extensions;

public static class DynamicLanguageEx
{
    internal static IEnumerable<UIElement> GetChildren(this UIElement parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            if (VisualTreeHelper.GetChild(parent, i) is UIElement child)
            {
                yield return child;
            }
        }
    }

    internal static IEnumerable<Type> GetHierarchyFromUIElement(this Type element)
    {
        if (element.GetTypeInfo().IsSubclassOf(typeof(UIElement)) is not true)
        {
            yield break;
        }

        Type? current = element;

        while (current is not null && current != typeof(UIElement))
        {
            yield return current;
            current = current.GetTypeInfo().BaseType;
        }
    }
}
