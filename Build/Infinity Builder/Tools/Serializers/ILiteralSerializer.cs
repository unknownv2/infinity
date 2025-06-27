using System.Collections.Generic;

namespace NoDev.Infinity.Build.InfinityBuilder.Tools.Serializers
{
    internal interface ILiteralSerializer
    {
        byte[] Serialize(IDictionary<int, object> literals);
    }
}
