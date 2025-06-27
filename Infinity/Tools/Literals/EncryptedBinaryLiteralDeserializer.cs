using System.IO;
using System.Security.Cryptography;
using NoDev.InfinityToolLib.Tools;

namespace NoDev.Infinity.Tools.Literals
{
    internal class EncryptedBinaryLiteralDeserializer : BinaryLiteralDeserializer
    {
        private readonly AesManaged _aes;

        internal EncryptedBinaryLiteralDeserializer(byte[] key)
        {
            _aes = new AesManaged
            {
                Key = key, 
                Mode = CipherMode.CBC
            };
        }

        public override ILiteralCollection Deserialize(byte[] buffer)
        {
            _aes.GenerateIV();

            var ms = new MemoryStream(buffer);
            ms.Read(_aes.IV, 0, _aes.IV.Length);
            var str = new CryptoStream(ms, _aes.CreateDecryptor(), CryptoStreamMode.Read);
            var lits = Deserialize(str);
            str.Close();

            return lits;
        }
    }
}
