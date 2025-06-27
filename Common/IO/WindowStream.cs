using System;
using System.IO;

namespace NoDev.Common.IO
{
    public class WindowStream : Stream
    {
        private readonly Stream _stream;
        private readonly long _backingOffset;
        private readonly long _endingOffset;
        private readonly long _length;

        public WindowStream(Stream stream, long startPosition, long length)
        {
            if (startPosition < 0)
                throw new ArgumentOutOfRangeException("startPosition");

            if (length < 0)
                throw new ArgumentOutOfRangeException("length");

            _backingOffset = startPosition;
            _endingOffset = startPosition + length;
            _length = length;

            try
            {
                if (_endingOffset > stream.Length)
                    throw new IOException("The window exceeds the length of the stream.");
            }
            catch (NotSupportedException)
            {
                // length is not supported, so whatever
            }
            

            _stream = stream;
        }

        public WindowStream(byte[] buffer, long startPosition, long length)
            : this(new MemoryStream(buffer), startPosition, length)
        {

        }

        public override long Position
        {
            get { return _stream.Position - _backingOffset; }
            set { _stream.Position = _backingOffset + value; }
        }

        public override bool CanRead
        {
            get { return _stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _stream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _stream.CanWrite; }
        }

        public override long Length
        {
            get { return _length; }
        }

        public byte[] ToArray()
        {
            Position = 0;
            var buffer = new byte[Length];
            Read(buffer, 0, buffer.Length);
            return buffer;
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override void Close()
        {
            _stream.Close();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_stream.Position < _backingOffset || _stream.Position + count > _endingOffset)
                throw new IOException("Stream position is out of bounds.");

            return _stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_stream.Position < _backingOffset || _stream.Position + count > _endingOffset)
                throw new IOException("Stream position is out of bounds.");

            _stream.Write(buffer, offset, count);
        }
    }
}
