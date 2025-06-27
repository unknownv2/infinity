using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using NoDev.Common.IO;
using NoDev.InfinityToolLib.Security;

namespace NoDev.InfinityToolLib.Memory
{
    public class ProcessMemory : IDisposable
    {
        public int ProcessID { get; private set; }

        public Process Process
        {
            get { return _process.Value; }
        }

        private readonly IntPtr _queryHandle;
        private readonly List<ProcessMemoryStream> _openStreams;
        private readonly Lazy<Process> _process;

        public ProcessMemory(int processId)
        {
            AssemblyProtection.EnsureProtected();

            ProcessID = processId;

            _queryHandle = NativeMethods.OpenProcess(
                NativeMethods.ProcessAccessFlags.QueryInformation | NativeMethods.ProcessAccessFlags.VirtualMemoryRead, 
                false, 
                processId
            );

            if (_queryHandle == IntPtr.Zero)
                throw new UnauthorizedAccessException("Failed to open process with query access.");

            _openStreams = new List<ProcessMemoryStream>();

            _process = new Lazy<Process>(() => Process.GetProcessById(ProcessID));
        }

        public void Close()
        {
            Dispose();
        }

        public Stream OpenModuleImageStream(string moduleName, MemoryAccess access = MemoryAccess.ReadWrite)
        {
            var handle = GetModuleHandleByBaseName(moduleName);

            if (handle == IntPtr.Zero)
                throw new ModuleNotFoundException(moduleName);

            return OpenModuleImageStream(handle, access);
        }

        public Stream OpenMainModuleImageStream(MemoryAccess access = MemoryAccess.ReadWrite)
        {
            return OpenModuleImageStream(GetAllModuleHandles()[0], access);
        }

        private Stream OpenModuleImageStream(IntPtr moduleHandle, MemoryAccess access)
        {
            if (moduleHandle == IntPtr.Zero)
                throw new ModuleNotFoundException();

            var info = new NativeMethods.MODULEINFO();

            if (!NativeMethods.GetModuleInformation(_queryHandle, moduleHandle, out info, (uint)Marshal.SizeOf(info)))
                throw new Win32Exception();

            var procStream = new ProcessMemoryStream(ProcessID, GetRealAccessFlags(access));

            _openStreams.Add(procStream);

            return new WindowStream(procStream, (long)info.lpBaseOfDll, info.SizeOfImage);
        }

        private IntPtr GetModuleHandleByBaseName(string moduleName)
        {
            var moduleHandles = GetAllModuleHandles();

            foreach (var moduleHandle in moduleHandles)
            {
                var sb = new StringBuilder(256);

                if (NativeMethods.GetModuleBaseName(_queryHandle, moduleHandle, sb, 512) == 0)
                    continue;

                if (moduleName.Equals(sb.ToString(), StringComparison.InvariantCulture))
                    return moduleHandle;
            }

            return IntPtr.Zero;
        }

        private IntPtr[] GetAllModuleHandles()
        {
            var moduleHandles = new IntPtr[64];

            uint size = 0;

            for (; ; )
            {
                var gcHandle = GCHandle.Alloc(moduleHandles, GCHandleType.Pinned);

                int x;

                for (x = 0; x < 50; x++)
                {
                    if (NativeMethods.EnumProcessModulesEx(_queryHandle, gcHandle.AddrOfPinnedObject(), (uint)(IntPtr.Size * moduleHandles.Length), out size, NativeMethods.ModuleFilterFlags.All))
                        break;
                }

                gcHandle.Free();

                if (x == 50)
                    throw new Win32Exception();

                size /= (uint)IntPtr.Size;

                if (size <= moduleHandles.Length)
                    break;

                moduleHandles = new IntPtr[moduleHandles.Length * 2];
            }

            if (moduleHandles.Length != size)
                Array.Resize(ref moduleHandles, (int)size);

            return moduleHandles;
        }

        private static NativeMethods.ProcessAccessFlags GetRealAccessFlags(MemoryAccess access)
        {
            NativeMethods.ProcessAccessFlags flags = 0;

            if (access.HasFlag(MemoryAccess.Read))
                flags |= NativeMethods.ProcessAccessFlags.VirtualMemoryRead;

            if (access.HasFlag(MemoryAccess.Write))
                flags |= NativeMethods.ProcessAccessFlags.VirtualMemoryWrite;

            return flags;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (!dispose)
                return;

            foreach (var s in _openStreams.Where(s => !s.WasClosed))
                s.Close();

            NativeMethods.CloseHandle(_queryHandle);
        }

        ~ProcessMemory()
        {
            Dispose(false);
        }
    }

    public class ProcessModule
    {
        public string Name;
        public ulong Address;
        public ulong Size;
    }

    [Flags]
    public enum MemoryAccess
    {
        Read = 0x01,
        Write = 0x02,
        ReadWrite = 0x03
    }
}
