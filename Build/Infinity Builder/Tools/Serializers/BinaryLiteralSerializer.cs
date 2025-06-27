using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NoDev.Infinity.Build.InfinityBuilder.Tools.Serializers
{
    internal class BinaryLiteralSerializer : ILiteralSerializer
    {
        private static readonly Random Rand = new Random();

        public virtual byte[] Serialize(IDictionary<int, object> literals)
        {
            var ms = new MemoryStream();
            Serialize(ms, literals);
            ms.Close();

            return ms.ToArray();
        }

        protected void Serialize(Stream str, IDictionary<int, object> literals)
        {
            var io = new BinaryWriter(str, Encoding.Unicode, true);

            int key;

            // outsiders only know the index values and the number of them,
            // so we mangle their values in memory with this key
            do { key = Rand.Next(Int32.MinValue, Int32.MaxValue); } while (key == 0);

            io.Write(key);
            io.Write(literals.Count ^ key);

            foreach (var literal in literals)
            {
                io.Write(literal.Key ^ key);

                var literalArray = literal.Value as Array;

                if (literalArray != null)
                {
                    io.Write((byte)1);
                    WriteArray(io, literalArray);
                }
                else
                {
                    io.Write((byte)0);
                    WritePrimitive(io, literal.Value);
                }
            }
        }

        private static void WritePrimitive(BinaryWriter io, object value)
        {
            if (value == null)
            {
                io.Write((byte)0);
                return;
            }

            var typeCode = Type.GetTypeCode(value.GetType());
            io.Write((byte)typeCode);

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    io.Write((bool)value);
                    break;
                case TypeCode.Byte:
                    io.Write((byte)value);
                    break;
                case TypeCode.Char:
                    io.Write((char)value);
                    break;
                case TypeCode.DateTime:
                    io.Write(((DateTime)value).ToBinary());
                    break;
                case TypeCode.Decimal:
                    io.Write((decimal)value);
                    break;
                case TypeCode.Double:
                    io.Write((double)value);
                    break;
                case TypeCode.Int16:
                    io.Write((short)value);
                    break;
                case TypeCode.Int32:
                    io.Write((int)value);
                    break;
                case TypeCode.Int64:
                    io.Write((long)value);
                    break;
                case TypeCode.SByte:
                    io.Write((sbyte)value);
                    break;
                case TypeCode.Single:
                    io.Write((float)value);
                    break;
                case TypeCode.String:
                    io.Write((string)value);
                    break;
                case TypeCode.UInt16:
                    io.Write((ushort)value);
                    break;
                case TypeCode.UInt32:
                    io.Write((uint)value);
                    break;
                case TypeCode.UInt64:
                    io.Write((ulong)value);
                    break;
                default:
                    throw new Exception(string.Format("Unsupported primitive type {0}.", typeCode));
            }
        }

        private static void WriteArray(BinaryWriter io, Array arr)
        {
            // type of elements in the array
            io.Write((byte)Type.GetTypeCode(arr.GetType().GetElementType()));

            // number of dimensions
            io.Write(arr.Rank);

            // number of elements in each dimension
            for (var x = 0; x < arr.Rank; x++)
                io.Write(arr.GetLongLength(x));

            var byteArray = arr as byte[];

            if (byteArray != null)
                io.Write(byteArray);
            else
            {
                // flatten the array
                foreach (var element in arr)
                    WritePrimitive(io, element);
            }
        }
    }
}
