using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NoDev.InfinityToolLib.Security;

namespace NoDev.InfinityToolLib.Memory
{
    public class GameBinary
    {
        private readonly Lazy<FileVersionInfo> _versionInfo;
        private readonly Lazy<BinaryType> _type; 

        public FileInfo FileInfo { get; private set; }
        public string Location { get; private set; }

        public string Name
        {
            get { return FileInfo.Name; }
        }

        public BinaryType Type
        {
            get { return _type.Value; }
        }

        public FileVersionInfo VersionInfo
        {
            get { return _versionInfo.Value; }
        }

        public GameBinary(string binaryPath)
            : this(new FileInfo(binaryPath))
        {
            
        }

        public GameBinary(FileInfo binaryFile)
        {
            AssemblyProtection.EnsureProtected();

            if (!binaryFile.Exists)
            {
                throw new ArgumentException("Game binary file not found.");
            }

            FileInfo = binaryFile;
            Location = binaryFile.FullName;

            _versionInfo = new Lazy<FileVersionInfo>(() => FileVersionInfo.GetVersionInfo(Location));

            _type = new Lazy<BinaryType>(() =>
            {
                if (FileInfo.Extension.Equals(".exe", StringComparison.InvariantCultureIgnoreCase))
                {
                    return BinaryType.Exe;
                }

                if (FileInfo.Extension.Equals(".dll", StringComparison.InvariantCultureIgnoreCase))
                {
                    return BinaryType.Dll;
                }

                return BinaryType.Unknown;
            });
        }

        public Process FindRunningProcess()
        {
            var processes = Process.GetProcessesByName(
                Path.GetFileNameWithoutExtension(Location));

            return processes.FirstOrDefault(proc =>
                proc.MainModule.FileName.Equals(
                Location, StringComparison.InvariantCultureIgnoreCase));
        }

        public Process RunGame(int waitForWindowSeconds = 0)
        {
            var proc = Process.Start(Location);

            if (proc == null || waitForWindowSeconds == 0)
            {
                return proc;
            }

            waitForWindowSeconds *= 100;

            for (var x = 0; proc.MainWindowHandle == IntPtr.Zero && x < waitForWindowSeconds; x++)
            {
                Thread.Sleep(10);
            }

            return proc;
        }
    }
}
