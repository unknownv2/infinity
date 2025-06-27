using NoDev.InfinityToolLib.Security;
using System;
using System.ComponentModel;
using System.IO;

namespace NoDev.InfinityToolLib.Memory
{
    public class ProcessMemoryStream : Stream
    {
        private readonly IntPtr _handle;
        private readonly NativeMethods.ProcessAccessFlags _accessFlags;

        internal ProcessMemoryStream(int processId, NativeMethods.ProcessAccessFlags accessFlags)
        {
            AssemblyProtection.EnsureProtected();

            _accessFlags = accessFlags;

            _handle = NativeMethods.OpenProcess(_accessFlags, false, processId);

            if (_handle == IntPtr.Zero)
                throw new UnauthorizedAccessException(string.Format("Failed to open process {0} with memory read and/or write access.", processId));
        }

        internal bool WasClosed { get; private set; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            IntPtr bytesRead;
            bool result;

            if (offset == 0)
                result = NativeMethods.ReadProcessMemory(_handle, (IntPtr)Position, buffer, (IntPtr)count, out bytesRead);
            else
            {
                unsafe
                {
                    fixed (byte* basePtr = buffer)
                    {
                        result = NativeMethods.ReadProcessMemory(_handle, (IntPtr)Position, (IntPtr)(basePtr + offset), (IntPtr)count, out bytesRead);
                    }
                }
            }

            if (!result)
                throw new Win32Exception();

            Position += (long)bytesRead;

            return (int)bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            IntPtr bytesWritten;
            bool result;

            if (offset == 0)
                result = NativeMethods.WriteProcessMemory(_handle, (IntPtr)Position, buffer, (IntPtr)count, out bytesWritten);
            else
            {
                unsafe
                {
                    fixed (byte* basePtr = buffer)
                    {
                        result = NativeMethods.WriteProcessMemory(_handle, (IntPtr)Position, (IntPtr)(basePtr + offset), (IntPtr)count, out bytesWritten);
                    }
                }
            }

            if (!result)
                throw new Win32Exception();

            Position += (long)bytesWritten;
        }

        public override void Flush()
        {
            // Nothing to flush
        }

        public override void Close()
        {
            NativeMethods.CloseHandle(_handle);

            WasClosed = true;

            base.Close();
        }

        public override long Position { get; set; }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    return Position = offset;
                case SeekOrigin.Current:
                    return Position += offset;
                default:
                    throw new NotSupportedException();
            }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return _accessFlags.HasFlag(NativeMethods.ProcessAccessFlags.VirtualMemoryRead); }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return _accessFlags.HasFlag(NativeMethods.ProcessAccessFlags.VirtualMemoryWrite); }
        }
    }
}
