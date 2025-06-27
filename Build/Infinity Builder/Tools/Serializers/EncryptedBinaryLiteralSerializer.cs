using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace NoDev.Infinity.Build.InfinityBuilder.Tools.Serializers
{
    internal class EncryptedBinaryLiteralSerializer : BinaryLiteralSerializer
    {
        private readonly AesManaged _aes;

        internal EncryptedBinaryLiteralSerializer(byte[] key)
        {
            _aes = new AesManaged
            {
                Key = key, 
                Mode = CipherMode.CBC
            };
        }

        public override byte[] Serialize(IDictionary<int, object> literals)
        {
            _aes.GenerateIV();

            var ms = new MemoryStream();
            var str = new CryptoStream(ms, _aes.CreateEncryptor(), CryptoStreamMode.Write);
            str.Write(_aes.IV, 0, _aes.IV.Length);
            Serialize(str, literals);
            str.Close();

            return ms.ToArray();
        }
    }
}
