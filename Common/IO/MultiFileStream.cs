using System;
using System.Collections.Generic;
using System.IO;

namespace NoDev.Common.IO
{
    public sealed class MultiFileStream : Stream
    {
        private readonly Stream[] _io;
        private readonly int _ioLength;
        private readonly long[] _sizeUpTill;
        private readonly long _length;

        private int _currentFileIndex;
        private Stream _currentStream;

        public MultiFileStream(IReadOnlyList<string> filePaths, FileMode fileMode = FileMode.Open, FileAccess fileAccess = FileAccess.ReadWrite)
        {
            _ioLength = filePaths.Count;

            _io = new Stream[_ioLength];

            _sizeUpTill = new long[_ioLength + 1];

            for (var x = 0; x < _ioLength; x++)
            {
                _io[x] = new FileStream(
                    NativeMethods.CreateFile(filePaths[x], fileAccess, FileShare.None,
                    IntPtr.Zero, fileMode, 0xc0000040, IntPtr.Zero), fileAccess, 4096, true);

                _sizeUpTill[x + 1] = _sizeUpTill[x] + _io[x].Length;
            }

            _length = _sizeUpTill[_ioLength];

            OpenFile(0);
        }

        private void OpenFile(int fileIndex)
        {
            if (fileIndex >= _ioLength)
                throw new IOException("Cannot seek passed the end of the stream.");

            _currentFileIndex = fileIndex;
            _currentStream = _io[_currentFileIndex];
            _currentStream.Position = 0;
        }

        public override void Flush()
        {
            foreach (var s in _io)
                s.Flush();
        }

        public override void Close()
        {
            foreach (var s in _io)
                s.Close();
        }

        public override long Position
        {
            get { return _sizeUpTill[_currentFileIndex] + _currentStream.Position; }
            set
            {
                var newIndex = GetFileIndexFromPosition(value);

                if (newIndex != _currentFileIndex)
                    OpenFile(newIndex);

                _currentStream.Position = value - _sizeUpTill[_currentFileIndex];
            }
        }

        private int GetFileIndexFromPosition(long position)
        {
            for (var x = 1; x <= _ioLength; x++)
                if (_sizeUpTill[x] > position)
                    return x - 1;

            throw new IOException("Cannot seek passed the end of the stream.");
        }

        public override bool CanRead
        {
            get { return _currentStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _currentStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _currentStream.CanWrite; }
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Seek(long seekLength, SeekOrigin orgin)
        {
            switch (orgin)
            {
                case SeekOrigin.Begin:
                    Position = seekLength;
                    break;
                case SeekOrigin.Current:
                    Position += seekLength;
                    break;
                case SeekOrigin.End:
                    Position = Length - seekLength;
                    break;
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        private int BytesLeftInCurrentFile
        {
            get { return (int) (_currentStream.Length - _currentStream.Position); }
        }

        public override int Read(byte[] buffer, int index, int count)
        {
            var bytesToRead = count;

            while (bytesToRead != 0)
            {
                var readBytes = BytesLeftInCurrentFile;

                while (readBytes == 0)
                {
                    OpenFile(_currentFileIndex + 1);
                    readBytes = (int)_currentStream.Length;
                }

                if (bytesToRead <= readBytes)
                    readBytes = bytesToRead;

                _currentStream.Read(buffer, index, readBytes);

                bytesToRead -= readBytes;
                index += readBytes;
            }

            return count;
        }

        public override void Write(byte[] buffer, int index, int count)
        {
            while (count != 0)
            {
                var writeBytes = BytesLeftInCurrentFile;

                while (writeBytes == 0)
                {
                    OpenFile(_currentFileIndex + 1);
                    writeBytes = (int)_currentStream.Length;
                }

                if (count <= writeBytes)
                    writeBytes = count;

                _currentStream.Write(buffer, index, writeBytes);

                count -= writeBytes;
                index += writeBytes;
            }
        }
    }
}
