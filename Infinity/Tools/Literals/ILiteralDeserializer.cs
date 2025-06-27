using NoDev.InfinityToolLib.Tools;

namespace NoDev.Infinity.Tools.Literals
{
    internal interface ILiteralDeserializer
    {
        ILiteralCollection Deserialize(byte[] buffer);
    }
}