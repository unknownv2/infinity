using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Linq;

namespace NoDev.Common
{
    public static class Formatting
    {
        private static readonly string[] SizeByteOrders = { "TB", "GB", "MB", "KB", "B" };
        private static readonly ulong SizeByteMax = (ulong)Math.Pow(1024, SizeByteOrdersLengthMinus1);
        private const int SizeByteOrdersLength = 5, SizeByteOrdersLengthMinus1 = SizeByteOrdersLength - 1;

        public static string GetSizeStringFromBytes(decimal bytes)
        {
            var max = SizeByteMax;

            for (var x = 0; x < SizeByteOrdersLength; x++)
            {
                if (bytes >= max)
                    return string.Format(((x == SizeByteOrdersLengthMinus1) ? "{0:0} {1}" : "{0:0.00} {1}"), bytes / max, SizeByteOrders[x]);

                max /= 1024;
            }

            return "0 B";
        }

        public static string ByteArrayToHexString(byte[] arr, bool upperCase = false)
        {
            var arrLen = arr.Length;
            var format = upperCase ? "X2" : "x2";
            var sb = new StringBuilder(arrLen << 1);

            for (var x = 0; x < arrLen; x++)
                sb.Append(arr[x].ToString(format));

            return sb.ToString();
        }

        public static byte[] HexStringToByteArray(string hexString)
        {
            var strLen = hexString.Length;
            var output = new byte[strLen >> 1];

            for (int x = 0, i = 0; x < strLen; x += 2, i++)
                output[i] = Convert.ToByte(hexString.Substring(x, 2), 16);

            return output;
        }

        public static byte[] SerializeToByteArray(object obj)
        {
            var ms = new MemoryStream();
            var bin = new BinaryFormatter
            {
                AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            };
            bin.Serialize(ms, obj);
            ms.Close();
            return ms.ToArray();
        }

        public static object DeserializeToObject(byte[] arr)
        {
            var bin = new BinaryFormatter
            {
                AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
            };
            var ms = new MemoryStream();
            ms.Write(arr, 0, arr.Length);
            var obj = bin.Deserialize(ms);
            ms.Dispose();
            return obj;
        }

        private static void InitializeStringEncodings()
        {
            _encodings = new List<Encoding>(Encoding.GetEncodings().Select(e => e.GetEncoding()).Where(e => e.GetPreamble().Length != 0));
        }

        private static List<Encoding> _encodings;

        public static Encoding GetStringEncoding(byte[] arr)
        {
            if (arr.Length < 2)
                return Encoding.UTF8;

            if (_encodings == null)
                InitializeStringEncodings();

            Debug.Assert(_encodings != null, "_encodings != null");

            foreach (var encoding in _encodings)
            {
                var preamble = encoding.GetPreamble();

                var len = preamble.Length;

                if (arr.Length < len)
                    continue;

                var good = true;
                for (var x = 0; x < len; x++)
                {
                    if (arr[x] == preamble[x])
                        continue;

                    good = false;

                    break;
                }

                if (good)
                    return encoding;
            }

            return Encoding.UTF8;
        }
    }
}
