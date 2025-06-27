 // ReSharper disable once CheckNamespace
namespace System
{
    public static class Rand
    {
        private static readonly Random RandomObj = new Random();

        public static int Next()
        {
            return RandomObj.Next();
        }

        public static int Next(int maxValue)
        {
            return RandomObj.Next(maxValue);
        }

        public static int Next(int minValue, int maxValue)
        {
            return RandomObj.Next(minValue, maxValue);
        }

        public static void NextBytes(byte[] buffer)
        {
            RandomObj.NextBytes(buffer);
        }

        public static byte[] NextBytes(int length)
        {
            var buffer = new byte[length];

            RandomObj.NextBytes(buffer);

            return buffer;
        }

        public static double NextDouble()
        {
            return RandomObj.NextDouble();
        }
    }
}
