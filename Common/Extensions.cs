using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using NoDev.Common;
using NoDev.Common.IO;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class Extensions
    {
        public static long[] GetMultidimensionalIndex(this Array arr, long index)
        {
            var indexes = new long[arr.Rank];
            var elementsInDimension = 1L;

            for (var x = arr.Rank - 1; x >= 0; x--)
            {
                var len = arr.GetLongLength(x);
                indexes[x] = (index / elementsInDimension) % len;
                elementsInDimension *= len;
            }

            return indexes;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            return dict.ContainsKey(key) ? dict[key] : default(TValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEqual(this byte[] b1, byte[] b2)
        {
            if (b1.Equals(b2))
                return true;

            return b1.Length == b2.Length && NativeMethods.memcmp(b1, b2, b1.Length) == 0;
        }

        public static T GetAttribute<T>(this Enum enumVal) where T : Attribute
        {
            var attributes = enumVal.GetType().GetMember(enumVal.ToString())[0].GetCustomAttributes(typeof(T), false);
            return attributes.Length != 0 ? (T)attributes[0] : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Memset(this byte[] buffer, byte value)
        {
            Memset(buffer, value, 0, buffer.Length);
        }

        public static unsafe void Memset(this byte[] buffer, byte value, int index, int count)
        {
            if (count < 0 || count > buffer.Length)
                throw new ArgumentOutOfRangeException("count");

            if (index < 0 || index + count > buffer.Length)
                throw new ArgumentOutOfRangeException("index");

            if (count == 0)
                return;

            var longValue = (long)value;

            longValue = (longValue << 8) | (longValue << 16) | (longValue << 24)
                | (longValue << 32) | (longValue << 48) | (longValue << 56) | (longValue << 64);

            fixed (byte* ptr = &buffer[index])
            {
                var lPtr = (long*)ptr;

                while (count >= 8)
                {
                    *lPtr = longValue;
                    lPtr++;
                    count -= 8;
                }

                var bPtr = (byte*)lPtr;

                while (count > 0)
                {
                    *bPtr = value;
                    bPtr++;
                    count--;
                }
            }
        }

        private static readonly byte[] NullBuffer = new byte[102400];

        public static bool IsNull(this byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (buffer.Length > 102400)
                throw new Exception("Buffer too large to check if null. Don't use this method.");

            return NativeMethods.memcmp(buffer, NullBuffer, buffer.Length) == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] ComputeSHA1(this byte[] data)
        {
            return SHA1.Create().ComputeHash(data);
        }

        public static byte[] ToArray(this string str, Encoding encoding, bool nullTerminated = false)
        {
            var arrSize = encoding.GetByteCount(str);

            if (nullTerminated)
                arrSize += encoding.GetByteCount("\0");

            var buffer = new byte[arrSize];

            encoding.GetBytes(str, 0, arrSize, buffer, 0);

            return buffer;
        }

        public static T ToStruct<T>(this byte[] arr) where T : struct
        {
            var gcHandle = GCHandle.Alloc(arr, GCHandleType.Pinned);
            var s = (T)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(T));
            gcHandle.Free();
            return s;
        }


        /*****************************************************************************/

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IoRequiresReverse(EndianType endianness)
        {
            if (BitConverter.IsLittleEndian)
                return endianness == EndianType.Big;

            return endianness == EndianType.Little;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Read(this Stream io, int length)
        {
            var buffer = new byte[length];
            io.Read(buffer, 0, length);
            return buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Read(this byte[] input, int index, int length)
        {
            var output = new byte[length];
            Array.Copy(input, index, output, 0, length);
            return output;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Read(this byte[] input, long index, long length)
        {
            var output = new byte[length];
            Array.Copy(input, index, output, 0, length);
            return output;
        }

        // Int16

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16(this byte[] data, int offset = 0, EndianType endianness = EndianType.Native)
        {
            return unchecked((short)data.ReadUInt16(offset, endianness));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16(this byte[] data, int offset = 0, EndianType endianness = EndianType.Native)
        {
            var x = BitConverter.ToUInt16(data, offset);

            if (!IoRequiresReverse(endianness))
                return x;

            return (ushort)((x << 8) | (x >> 8));
        }

        // Int32

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32(this byte[] data, int offset = 0, EndianType endianness = EndianType.Native)
        {
            return unchecked((int)data.ReadUInt32(offset, endianness));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32(this byte[] data, int offset = 0, EndianType endianness = EndianType.Native)
        {
            var x = BitConverter.ToUInt32(data, offset);

            if (!IoRequiresReverse(endianness))
                return x;

            return (x << 24) | (x >> 24) | ((x & 0xff00) << 8) | ((x >> 8) & 0xff00);
        }

        // Int64

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ReadInt64(this byte[] data, int offset = 0, EndianType endianness = EndianType.Native)
        {
            return unchecked((long)data.ReadUInt64(offset, endianness));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ReadUInt64(this byte[] data, int offset = 0, EndianType endianness = EndianType.Native)
        {
            var x = BitConverter.ToUInt64(data, offset);

            if (!IoRequiresReverse(endianness))
                return x;

            return
                (x << 56) | (x >> 56) |
                ((x & 0xff00) << 40) | ((x >> 40) & 0xff00) |
                ((x & 0xff0000) << 24) | ((x >> 24) & 0xff0000) |
                ((x & 0xff000000) << 8) | ((x >> 8) & 0xff000000);
        }

        // Single

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float ReadSingle(this byte[] data, int offset = 0, EndianType endianness = EndianType.Native)
        {
            if (offset < 0 || offset + 4 >= data.Length)
                throw new ArgumentOutOfRangeException("offset");

            if (!IoRequiresReverse(endianness))
                return BitConverter.ToSingle(data, offset);

            var asInt32 = data.ReadInt32(offset, endianness);

            return *(float*)&asInt32;
        }

        // Double

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe double ReadDouble(this byte[] data, int offset = 0, EndianType endianness = EndianType.Native)
        {
            if (offset < 0 || offset + 8 >= data.Length)
                throw new ArgumentOutOfRangeException("offset");

            if (!IoRequiresReverse(endianness))
                return BitConverter.ToDouble(data, offset);

            var asInt64 = data.ReadInt64(offset, endianness);

            return *(double*)&asInt64;
        }


        /*****************************************************************************/


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream io, byte[] buffer)
        {
            io.Write(buffer, 0, buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this byte[] input, byte[] buffer)
        {
            Array.Copy(buffer, 0, input, 0, buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this byte[] input, byte[] buffer, int index, int length)
        {
            Array.Copy(buffer, 0, input, index, length);
        }

        // Ints

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this byte[] data, short value, int offset = 0, EndianType endianness = EndianType.Native)
        {
            Write(data, unchecked((ushort)value), offset, endianness);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this byte[] data, ushort value, int offset = 0, EndianType endianness = EndianType.Native)
        {
            var bytes = BitConverter.GetBytes(value);

            if (!IoRequiresReverse(endianness))
                Array.Copy(bytes, 0, data, offset, 2);
            else
            {
                data[offset] = bytes[1];
                data[offset + 1] = bytes[0];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this byte[] data, int value, int offset = 0, EndianType endianness = EndianType.Native)
        {
            Write(data, unchecked((uint)value), offset, endianness);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this byte[] data, uint value, int offset = 0, EndianType endianness = EndianType.Native)
        {
            var bytes = BitConverter.GetBytes(value);

            if (!IoRequiresReverse(endianness))
                Array.Copy(bytes, 0, data, offset, 4);
            else
            {
                data[offset] = bytes[3];
                data[offset + 1] = bytes[2];
                data[offset + 2] = bytes[1];
                data[offset + 3] = bytes[0];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this byte[] data, long value, int offset = 0, EndianType endianness = EndianType.Native)
        {
            Write(data, unchecked((ulong)value), offset, endianness);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this byte[] data, ulong value, int offset = 0, EndianType endianness = EndianType.Native)
        {
            var bytes = BitConverter.GetBytes(value);

            if (!IoRequiresReverse(endianness))
                Array.Copy(bytes, 0, data, offset, 8);
            else
            {
                data[offset] = bytes[7];
                data[offset + 7] = bytes[0];
                data[offset + 1] = bytes[6];
                data[offset + 6] = bytes[1];
                data[offset + 2] = bytes[5];
                data[offset + 5] = bytes[2];
                data[offset + 3] = bytes[4];
                data[offset + 4] = bytes[3];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this byte[] data, float value, int offset = 0, EndianType endianness = EndianType.Native)
        {
            var bytes = BitConverter.GetBytes(value);

            if (!IoRequiresReverse(endianness))
                Array.Copy(bytes, 0, data, offset, 4);
            else
            {
                data[offset] = bytes[3];
                data[offset + 1] = bytes[2];
                data[offset + 2] = bytes[1];
                data[offset + 3] = bytes[0];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this byte[] data, double value, int offset = 0, EndianType endianness = EndianType.Native)
        {
            var bytes = BitConverter.GetBytes(value);

            if (!IoRequiresReverse(endianness))
                Array.Copy(bytes, 0, data, offset, 8);
            else
            {
                data[offset] = bytes[7];
                data[offset + 1] = bytes[6];
                data[offset + 2] = bytes[5];
                data[offset + 3] = bytes[4];
                data[offset + 4] = bytes[3];
                data[offset + 5] = bytes[2];
                data[offset + 6] = bytes[1];
                data[offset + 7] = bytes[0];
            }
        }


        /*****************************************************************************/


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte RotateLeft(this sbyte value, int bits)
        {
            return unchecked((sbyte)RotateLeft((byte)value, bits));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte RotateLeft(this byte value, int bits)
        {
            if (bits <= 0 || bits >= 8)
                throw new ArgumentOutOfRangeException("bits");

            var x = value;

            x <<= bits;
            value >>= 16 - bits;
            x |= value;

            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short RotateLeft(this short value, int bits)
        {
            return unchecked((short)RotateLeft((ushort)value, bits));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort RotateLeft(this ushort value, int bits)
        {
            if (bits <= 0 || bits >= 16)
                throw new ArgumentOutOfRangeException("bits");

            var x = value;

            x <<= bits;
            value >>= 16 - bits;
            x |= value;

            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RotateLeft(this int value, int bits)
        {
            return unchecked((int)RotateLeft((uint)value, bits));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RotateLeft(this uint value, int bits)
        {
            if (bits <= 0 || bits >= 32)
                throw new ArgumentOutOfRangeException("bits");

            var x = value;

            x <<= bits;
            value >>= 32 - bits;
            x |= value;

            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long RotateLeft(this long value, int bits)
        {
            return unchecked((long)RotateLeft((ulong)value, bits));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong RotateLeft(this ulong value, int bits)
        {
            if (bits <= 0 || bits >= 64)
                throw new ArgumentOutOfRangeException("bits");

            var x = value;

            x <<= bits;
            value >>= 64 - bits;
            x |= value;

            return x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitSet(this sbyte value, int bit) { return ((1 << bit) & value) != 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitSet(this byte value, int bit) { return ((1 << bit) & value) != 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitSet(this short value, int bit) { return ((1 << bit) & value) != 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitSet(this ushort value, int bit) { return ((1 << bit) & value) != 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitSet(this int value, int bit) { return ((1 << bit) & value) != 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitSet(this uint value, int bit) { return ((1 << bit) & value) != 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitSet(this long value, int bit) { return ((1 << bit) & value) != 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitSet(this ulong value, int bit) { return ((1U << bit) & value) != 0; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte SetBit(this sbyte value, int bit, bool on) { return on ? (sbyte)((byte)value | (1 << bit)) : (sbyte)(value & ~(1 << bit)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte SetBit(this byte value, int bit, bool on) { return on ? (byte)(value | (1 << bit)) : (byte)(value & ~(1 << bit)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short SetBit(this short value, int bit, bool on) { return on ? (short)((ushort)value | (1 << bit)) : (short)(value & ~(1 << bit)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort SetBit(this ushort value, int bit, bool on) { return on ? (ushort)(value | (1 << bit)) : (ushort)(value & ~(1 << bit)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SetBit(this int value, int bit, bool on) { return on ? (value | (1 << bit)) : (value & ~(1 << bit)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SetBit(this uint value, int bit, bool on) { return on ? (value | (1U << bit)) : (value & ~(1U << bit)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long SetBit(this long value, int bit, bool on) { return on ? (value | (1L << bit)) : (value & ~(1L << bit)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong SetBit(this ulong value, int bit, bool on) { return on ? (value | (1UL << bit)) : (value & ~(1UL << bit)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFlagOn(this sbyte value, sbyte flag) { return (value & flag) != 0x00; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFlagOn(this byte value, byte flag) { return (value & flag) != 0x00; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFlagOn(this short value, short flag) { return (value & flag) != 0x00; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFlagOn(this ushort value, ushort flag) { return (value & flag) != 0x00; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFlagOn(this int value, int flag) { return (value & flag) != 0x00; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFlagOn(this uint value, uint flag) { return (value & flag) != 0x00; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFlagOn(this long value, long flag) { return (value & flag) != 0x00; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFlagOn(this ulong value, ulong flag) { return (value & flag) != 0x00; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountLeadingZeros(this sbyte value) { return CountLeadingZeros(unchecked((byte)value)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountLeadingZeros(this byte value) { return 8 - CountDigits(value); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountLeadingZeros(this short value) { return CountLeadingZeros(unchecked((ushort)value)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountLeadingZeros(this ushort value) { return 16 - CountDigits(value); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountLeadingZeros(this int value) { return CountLeadingZeros(unchecked((uint)value)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountLeadingZeros(this uint value) { return 32 - CountDigits(value); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountLeadingZeros(this long value) { return CountLeadingZeros(unchecked((ulong)value)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CountLeadingZeros(this ulong value) { return 64 - CountDigits(value); }

        private static int CountDigits(this ulong value)
        {
            var digits = 0;

            while (value != 0)
            {
                value >>= 1;
                digits++;
            }

            return digits;
        }
    }
}