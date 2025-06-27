using System.Security.Cryptography;
using System.Text;

namespace NoDev.Infinity.Build.InfinityBuilder
{
    internal class ByteArrayTransformer
    {
        internal static string ToProperty(byte[] arr, string propertyName, string modifiers = "")
        {
            var sb = new StringBuilder();

            sb.AppendFormat("{0} byte[] {1}", modifiers, propertyName);
            sb.AppendLine(" { get {\r\n");
            sb.AppendLine(ToLocalVariable(arr, "arr"));
            sb.AppendLine("return arr; }}");

            return sb.ToString();
        }

        internal static string ToLocalVariable(byte[] arr, string varName)
        {
            var indexes = new int[arr.Length];

            for (var x = 0; x < arr.Length; x++)
                indexes[x] = x;

            // shuffle the indexes
            var provider = new RNGCryptoServiceProvider();
            var n = indexes.Length;

            while (n > 1)
            {
                var box = new byte[1];

                do
                {
                    provider.GetBytes(box);
                }
                // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                while (!(box[0] < n * (0xff / n)));

                var k = box[0] % n--;
                var value = indexes[k];
                indexes[k] = indexes[n];
                indexes[n] = value;
            }

            var sb = new StringBuilder();
            sb.AppendFormat("var {0} = new byte[{1}];\r\n", varName, arr.Length);

            for (var x = 0; x < arr.Length; x++)
            {
                sb.AppendFormat("{0}[{1}] = {2:x2};\r\n", varName, indexes[x], arr[indexes[x]]);
            }

            return sb.ToString();
        }
    }
}
