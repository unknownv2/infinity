using System.IO;
using System.Security.Cryptography;

namespace NoDev.Common.Cryptography
{
    public class SimpleAES
    {
        public static byte[] Encrypt(byte[] data, byte[] key)
        {
            var outStream = new MemoryStream();

            Encrypt(new MemoryStream(data), outStream, key);

            return outStream.ToArray();
        }

        public static void Encrypt(string infile, string outfile, byte[] key)
        {
            Encrypt(File.OpenRead(infile), File.OpenWrite(outfile), key);
        }

        public static void Encrypt(Stream inStream, Stream outStream, byte[] key, bool leaveOpen = false)
        {
            var aes = new AesManaged
            {
                Key = key,
                Mode = CipherMode.CBC
            };

            aes.GenerateIV();

            outStream.Write(aes.IV, 0, aes.IV.Length);
            var encStream = new CryptoStream(outStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            inStream.CopyTo(encStream);

            if (!leaveOpen)
            {
                inStream.Close();
                outStream.Close();
            }
        }

        public static byte[] Decrypt(byte[] data, byte[] key)
        {
            var ms = new MemoryStream();

            Decrypt(new MemoryStream(data), ms, key);

            return ms.ToArray();
        }

        public static void Decrypt(string infile, string outfile, byte[] key)
        {
            Decrypt(File.OpenRead(infile), File.OpenWrite(outfile), key);
        }

        public static void Decrypt(Stream inStream, Stream outStream, byte[] key, bool leaveOpen = false)
        {
            var aes = new AesManaged
            {
                Key = key,
                Mode = CipherMode.CBC
            };

            aes.GenerateIV();

            inStream.Read(aes.IV, 0, aes.IV.Length);
            var encStream = new CryptoStream(inStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            encStream.CopyTo(outStream);

            if (!leaveOpen)
            {
                inStream.Close();
                encStream.Close();
            }
        }
    }
}
