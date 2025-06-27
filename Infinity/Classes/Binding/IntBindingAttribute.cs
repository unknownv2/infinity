namespace NoDev.Infinity.Binding
{
    internal class IntBindingAttribute : BindingAttribute
    {
        internal readonly string Text;
        internal readonly int MaxValue;
        internal readonly bool ShowMaxButton;

        internal IntBindingAttribute(string text, int maxValue = -1, bool showMaxButton = true)
        {
            Text = text;
            MaxValue = maxValue;
            ShowMaxButton = showMaxButton;
        }
    }
}
