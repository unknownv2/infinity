namespace NoDev.Infinity.Binding
{
    internal class BoolBindingAttribute : BindingAttribute
    {
        internal readonly string Text;

        internal BoolBindingAttribute(string text)
        {
            Text = text;
        }
    }
}
