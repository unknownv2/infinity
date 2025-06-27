namespace NoDev.Infinity.Binding
{
    internal class IndexBindingAttribute : BindingAttribute
    {
        internal readonly string Title;
        internal readonly object[] Items;

        internal IndexBindingAttribute(string title, params object[] items)
        {
            Title = title;
            Items = items;
        }
    }
}
