using System.Globalization;
using System.Runtime.CompilerServices;

namespace NoDev.Common
{
    public static class Numbers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseUInt64Hex(string s, out ulong output)
        {
            return ulong.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out output);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseInt32Hex(string s, out int output)
        {
            return int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out output);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseUInt32Hex(string s, out uint output)
        {
            return uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out output);
        }
    }
}
