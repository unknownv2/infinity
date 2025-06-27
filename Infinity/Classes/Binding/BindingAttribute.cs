using System;

namespace NoDev.Infinity.Binding
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    internal class BindingAttribute : Attribute
    {

    }
}
