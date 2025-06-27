using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using NoDev.Common.IO;

namespace NoDev.Common.Cryptography
{
    public class CRC32
    {
        public const uint DefaultNormalPolynomial = 0x04c11db7;
        public const uint DefaultReversedPolynomial = 0xedb88320;
        public const uint DefaultReversedReciprocalPolynomial = 0x82608edb;

        private static readonly IDictionary<uint, uint[]> CachedCrc32Tables = new ConcurrentDictionary<uint, uint[]>();

        private static uint[] BuildCRC32Table(uint polynomial)
        {
            if (CachedCrc32Tables.ContainsKey(polynomial))
                return CachedCrc32Tables[polynomial];

            var table = new uint[256];

            for (uint x = 0; x < 256; x++)
            {
                var dwCrc = x;

                for (var i = 0; i < 8; i++)
                {
                    if ((dwCrc & 1) != 0)
                        dwCrc = (dwCrc >> 1) ^ polynomial;
                    else
                        dwCrc >>= 1;
                }

                table[x] = dwCrc;
            }

            lock (CachedCrc32Tables)
            {
                if (!CachedCrc32Tables.ContainsKey(polynomial))
                {
                    CachedCrc32Tables.Add(polynomial, table);
                }
            }

            return table;
        }

        private readonly uint[] _crc32Table;
        private readonly uint _xorInit;

        private readonly bool _reverse;

        private uint _crc;

        public CRC32(EndianType endianType = EndianType.Native, uint polynomial = DefaultReversedPolynomial, uint initialValue = 0xffffffff)
        {
            _crc32Table = BuildCRC32Table(polynomial);
            _xorInit = initialValue;

            if (endianType == EndianType.Native)
                _reverse = false;
            else if (BitConverter.IsLittleEndian)
                _reverse = endianType == EndianType.Big;
            else
                _reverse = endianType == EndianType.Little;

            Clear();
        }

        public void Clear()
        {
            _crc = _xorInit;
        }

        private void HashCore(byte[] buffer, int offset, int count)
        {
            for (var x = offset; x < count; x++)
                _crc = (_crc >> 8) ^ _crc32Table[(_crc & 0xff) ^ buffer[x]];
        }

        private uint HashFinal()
        {
            var crc = _crc ^ _xorInit;

            if (_reverse)
                crc = (crc << 24) | ((crc & 0xff00) << 8) | ((crc & 0xff0000) >> 8) | (crc >> 24);

            return crc;
        }

        public uint Calculate(byte[] buffer)
        {
            return Calculate(buffer, 0, buffer.Length);
        }

        public uint Calculate(byte[] buffer, int offset, int count)
        {
            HashCore(buffer, offset, count);

            return HashFinal();
        }

        public uint Calculate(Stream inputStream)
        {
            int bytesRead;

            var buffer = new byte[4096];

            while ((bytesRead = inputStream.Read(buffer, 0, 4096)) > 0)
                HashCore(buffer, 0, bytesRead);

            return HashFinal();
        }
    }
}
