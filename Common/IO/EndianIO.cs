using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using NoDev.Common.Security;

namespace NoDev.Common.IO
{
    public sealed class EndianIO
    {
        private EndianType _endianness;
        private bool _requiresReverse;
        private Encoding _unicodeEncoder;

        private readonly Stream _stream;
        private readonly byte[] _buffer = new byte[8];

        public EndianIO(Stream stream, EndianType endianType = EndianType.Native)
        {
            AssemblyProtection.EnsureProtected();

            _stream = stream;
            Endianness = endianType;
        }

        public EndianIO(EndianType endianType = EndianType.Native)
            : this(new MemoryStream(), endianType)
        {

        }

        public EndianIO(int maxSize, EndianType endianType = EndianType.Native)
            : this(new MemoryStream(maxSize), endianType)
        {

        }

        public EndianIO(byte[] buffer, EndianType endianType = EndianType.Native)
            : this(new MemoryStream(buffer), endianType)
        {

        }

        public EndianIO(string fileName, EndianType endianType = EndianType.Native, FileMode fileMode = FileMode.Open,
            FileAccess fileAccess = FileAccess.ReadWrite, FileShare fileShare = FileShare.Read, int bufferSize = 4096, bool isAsync = false)
            : this(new FileStream(fileName, fileMode, fileAccess, fileShare, bufferSize, isAsync), endianType)
        {

        }

        public EndianType Endianness
        {
            get { return _endianness; }
            set
            {
                _endianness = value;

                switch (value)
                {
                    case EndianType.Native:
                        _requiresReverse = false;
                        _unicodeEncoder = BitConverter.IsLittleEndian ? Encoding.Unicode : Encoding.BigEndianUnicode;
                        break;
                    case EndianType.Little:
                        _requiresReverse = !BitConverter.IsLittleEndian;
                        _unicodeEncoder = Encoding.Unicode;
                        break;
                    case EndianType.Big:
                        _requiresReverse = BitConverter.IsLittleEndian;
                        _unicodeEncoder = Encoding.BigEndianUnicode;
                        break;
                }
            }
        }

        public bool EOF
        {
            get { return Position == Length; }
        }

        public long Length
        {
            get
            {
                return _stream.Length;
            }
        }

        public long Position
        {
            get { return _stream.Position; }
            set { _stream.Position = value; }
        }

        public void Seek(long position, SeekOrigin origin)
        {
            _stream.Seek(position, origin);
        }

        public void Flush()
        {
            _stream.Flush();
        }

        public void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public Stream GetStream()
        {
            return _stream;
        }

        public void Close()
        {
            _stream.Close();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReadByteArray(int count)
        {
            var buffer = new byte[count];
            Read(buffer, 0, count);
            return buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReadByteArray(long count)
        {
            var buffer = new byte[count];
            Read(buffer, 0, buffer.Length);
            return buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ReadToEnd()
        {
            return ReadByteArray((int)(Length - Position));
        }

        public sbyte ReadSByte()
        {
            Read(_buffer, 0, 1);
            return (sbyte)_buffer[0];
        }

        public byte ReadByte()
        {
            Read(_buffer, 0, 1);
            return _buffer[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBoolean()
        {
            return ReadByte() != 0x00;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            return unchecked((ushort)ReadInt16());
        }

        public unsafe short ReadInt16()
        {
            Read(_buffer, 0, 2);

            if (_requiresReverse)
                return (short)(_buffer[1] | (_buffer[0] << 8));

            fixed (byte* pbyte = &_buffer[0])
                return *((short*)pbyte);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32()
        {
            return unchecked((uint)ReadInt32());
        }

        public unsafe int ReadInt32()
        {
            Read(_buffer, 0, 4);

            if (_requiresReverse)
                return _buffer[3] | (_buffer[2] << 8) | (_buffer[1] << 16) | (_buffer[0] << 24);

            fixed (byte* pbyte = &_buffer[0])
                return *((int*)pbyte);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            return unchecked((ulong)ReadInt64());
        }

        public unsafe long ReadInt64()
        {
            Read(_buffer, 0, 8);

            if (_requiresReverse)
            {
                var n1 = (_buffer[3] | (_buffer[2] << 8) | (_buffer[1] << 16) | (_buffer[0] << 24)) & 0xffffffff;
                var n2 = (_buffer[7] | (_buffer[6] << 8) | (_buffer[5] << 16) | (_buffer[4] << 24)) & 0xffffffff;

                return n2 | (n1 << 32);
            }

            fixed (byte* pbyte = &_buffer[0])
                return *((long*)pbyte);
        }

        public uint ReadUInt24()
        {
            Read(_buffer, 0, 3);

            if (!_requiresReverse)
                return (uint)(_buffer[0] | (_buffer[1] << 8) | (_buffer[2] << 16));

            return (uint)(_buffer[2] | (_buffer[1] << 8) | (_buffer[0] << 16));
        }

        public unsafe float ReadSingle()
        {
            var x = ReadInt32();

            return *(float*)&x;
        }

        public unsafe double ReadDouble()
        {
            var x = ReadInt64();

            return *((double*)&x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadString(Encoding encoding, int lengthInBytes)
        {
            return encoding.GetString(ReadByteArray(lengthInBytes));
        }

        public string ReadNullTerminatedString(Encoding encoding)
        {
            var sb = new StringBuilder();

            var io = new StreamReader(_stream, encoding, false, 16, true);

            int currentChar;
            while ((currentChar = io.Read()) != -1 && currentChar != 0)
                sb.Append((char)currentChar);

            io.Close();

            return sb.ToString();
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
        {
            const byte one = 1, zero = 0;
            Write(value ? one : zero);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(sbyte value)
        {
            Write(unchecked((byte)value));
        }

        public void Write(byte value)
        {
            _buffer[0] = value;
            Write(_buffer, 0, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short value)
        {
            Write(unchecked((ushort)value));
        }

        public void Write(ushort value)
        {
            if (_requiresReverse)
                value = (ushort)((value << 8) | (value >> 8));

            Write(BitConverter.GetBytes(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value)
        {
            Write(unchecked((uint)value));
        }

        public void Write(uint value)
        {
            if (_requiresReverse)
                value = (value << 24) | (value >> 24) | ((value & 0xff00) << 8) | ((value >> 8) & 0xff00);

            Write(BitConverter.GetBytes(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(long value)
        {
            Write(unchecked((ulong)value));
        }

        public void Write(ulong value)
        {
            if (_requiresReverse)
            {
                value = (value << 56) | (value >> 56) |
                    ((value & 0xff00) << 40) | ((value >> 40) & 0xff00) |
                    ((value & 0xff0000) << 24) | ((value >> 24) & 0xff0000) |
                    ((value & 0xff000000) << 8) | ((value >> 8) & 0xff000000);
            }

            Write(BitConverter.GetBytes(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(float value)
        {
            Write(*((uint*)&value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(double value)
        {
            Write(*((ulong*)&value));
        }

        public void WriteUInt24(uint value)
        {
            var buffer = BitConverter.GetBytes(value);

            Array.Resize(ref buffer, 3);

            if (_requiresReverse)
            {
                var t = buffer[0];
                buffer[0] = buffer[2];
                buffer[2] = t;
            }

            Write(buffer);
        }

        public int Write(string value, Encoding encoding, int maxLengthInBytes = -1)
        {
            if (maxLengthInBytes == 0)
                return 0;

            var arr = encoding.GetBytes(value);

            var count = (maxLengthInBytes > 0 && arr.Length > maxLengthInBytes) ? maxLengthInBytes : arr.Length;

            Write(arr, 0, count);

            return count;
        }
    }
}