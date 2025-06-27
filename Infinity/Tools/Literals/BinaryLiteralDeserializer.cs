using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using NoDev.InfinityToolLib.Tools;

namespace NoDev.Infinity.Tools.Literals
{
    internal class BinaryLiteralDeserializer : ILiteralDeserializer
    {
        public virtual ILiteralCollection Deserialize(byte[] buffer)
        {
            var ms = new MemoryStream(buffer);
            var ret = Deserialize(ms);
            ms.Close();

            return ret;
        }

        protected ILiteralCollection Deserialize(Stream str)
        {
            var io = new BinaryReader(str, Encoding.Unicode);

            var key = io.ReadInt32();
            var dic = new LiteralDictionary(key);
            var count = io.ReadInt32() ^ key;

            while (count-- > 0)
            {
                var id = io.ReadInt32();
                var litType = io.ReadByte();
                object value;

                switch (litType)
                {
                    case 0:
                        value = ReadPrimitive(io);
                        break;
                    case 1:
                        value = ReadArray(io);
                        break;
                    default:
                        throw new Exception("Unknown literal type.");
                }

                dic.SetValue(id, value);
            }

            return dic;
        }

        private static object ReadPrimitive(BinaryReader io)
        {
            var type = (TypeCode)io.ReadByte();

            if (type == 0)
                return null;

            object value;

            switch (type)
            {
                case TypeCode.Boolean:
                    value = io.ReadBoolean();
                    break;
                case TypeCode.Byte:
                    value = io.ReadByte();
                    break;
                case TypeCode.Char:
                    value = io.ReadChar();
                    break;
                case TypeCode.DateTime:
                    value = DateTime.FromBinary(io.ReadInt64());
                    break;
                case TypeCode.Decimal:
                    value = io.ReadDecimal();
                    break;
                case TypeCode.Double:
                    value = io.ReadDouble();
                    break;
                case TypeCode.Int16:
                    value = io.ReadInt16();
                    break;
                case TypeCode.Int32:
                    value = io.ReadInt32();
                    break;
                case TypeCode.Int64:
                    value = io.ReadInt64();
                    break;
                case TypeCode.SByte:
                    value = io.ReadSByte();
                    break;
                case TypeCode.Single:
                    value = io.ReadSingle();
                    break;
                case TypeCode.String:
                    value = io.ReadString();
                    break;
                case TypeCode.UInt16:
                    value = io.ReadUInt16();
                    break;
                case TypeCode.UInt32:
                    value = io.ReadUInt32();
                    break;
                case TypeCode.UInt64:
                    value = io.ReadUInt64();
                    break;
                default:
                    throw new Exception(string.Format("Unknown primitive type {0}.", type));
            }

            return value;
        }

        private static Array ReadArray(BinaryReader io)
        {
            var typeCode = (TypeCode)io.ReadByte();
            var rank = io.ReadInt32();
            var lengths = new long[rank];

            var numElements = 1L;

            for (var x = 0; x < rank; x++)
            {
                lengths[x] = io.ReadInt64();
                numElements *= lengths[x];
            }

            // a 1-dimensional byte array shortcut
            if (typeCode == TypeCode.Byte && rank == 1 && lengths[0] <= Int32.MaxValue)
                return io.ReadBytes((int)lengths[0]);

            // this seems like the only way to convert a TypeCode to a Type
            var type = Type.GetType("System." + typeCode);
            Debug.Assert(type != null, "type != null");
            var arr = Array.CreateInstance(type, lengths);

            for (var x = 0L; x < numElements; x++)
                arr.SetValue(ReadPrimitive(io), arr.GetMultidimensionalIndex(x));

            return arr;
        }
    }
}
