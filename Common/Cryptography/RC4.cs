using System;

namespace NoDev.Common.Cryptography
{
    public class RC4
    {
        private readonly byte[] _key;

        public RC4(byte[] key)
        {
            if (key.Length == 0 || key.Length > 256)
                throw new ArgumentOutOfRangeException("key");

            _key = key;
        }

        public void Transform(byte[] data)
        {
            var dataLength = data.Length;
            var keyLength = _key.Length;

            byte n1;
            int x;

            var b1 = new byte[0x100];
            var b2 = new byte[0x100];

            for (x = 0; x < 0x100; x++)
            {
                b1[x] = (byte)x;
                b2[x] = _key[x % keyLength];
            }

            var i = 0;

            for (x = 0; x < 0x100; x++)
            {
                i = (i + b1[x] + b2[x]) & 0xff;
                n1 = b1[x];
                b1[x] = b1[i];
                b1[i] = n1;
            }

            x = 0;
            i = 0;

            for (var z = 0; z < dataLength; z++)
            {
                x = (x + 1) & 0xff;
                i = (i + b1[x]) & 0xff;
                n1 = b1[x];
                b1[x] = b1[i];
                b1[i] = n1;

                data[z] ^= b1[(b1[x] + n1) & 0xff];
            }
        }
    }
}
