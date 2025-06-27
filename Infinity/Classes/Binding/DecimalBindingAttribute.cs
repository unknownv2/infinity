namespace NoDev.Infinity.Binding
{
    internal class DecimalBindingAttribute : BindingAttribute
    {
        internal readonly string Text;
        internal readonly decimal MaxValue;
        internal readonly int DecimalPlaces;

        internal DecimalBindingAttribute(string text, decimal maxValue, int decimalPlaces)
        {
            Text = text;
            MaxValue = maxValue;
            DecimalPlaces = decimalPlaces;
        }
    }
}
