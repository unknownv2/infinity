 namespace NoDev.Infinity.Binding
{
    internal class BitBindingAttribute : BindingAttribute
    {
        internal readonly string Text;
        internal readonly int Bit;

        internal BitBindingAttribute(string text, int bit)
        {
            Text = text;
            Bit = bit;
        }
    }
}
