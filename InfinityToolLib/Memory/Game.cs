using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NoDev.InfinityToolLib.Security;

namespace NoDev.InfinityToolLib.Memory
{
    public class Game
    {
        private static readonly string[] BinaryExtensions = { ".exe", ".dll" };

        public DirectoryInfo DirectoryInfo { get; private set; }

        public string Location
        {
            get { return DirectoryInfo.FullName; }
        }

        public Game(string gameDir)
        {
            AssemblyProtection.EnsureProtected();

            DirectoryInfo = new DirectoryInfo(gameDir);

            if (!DirectoryInfo.Exists)
            {
                throw new FileNotFoundException("Game directory does not exist: " + DirectoryInfo.FullName);
            }
        }

        public IEnumerable<GameBinary> Binaries
        {
            get
            {
                var allowedExtensions = new HashSet<string>(BinaryExtensions, StringComparer.OrdinalIgnoreCase);

                return DirectoryInfo.EnumerateFiles()
                    .Where(f => allowedExtensions.Contains(f.Extension))
                    .Select(f => new GameBinary(f));
            }
        }

        public IEnumerable<GameBinary> Executables
        {
            get
            {
                return DirectoryInfo.EnumerateFiles("*.exe", SearchOption.AllDirectories)
                    .Select(f => new GameBinary(f));
            }
        }

        public IEnumerable<GameBinary> Dlls
        {
            get
            {
                return DirectoryInfo.EnumerateFiles("*.dll", SearchOption.AllDirectories)
                    .Select(f => new GameBinary(f));
            }
        }

        public GameBinary FindBinary(string filename)
        {
            var fileInfo = new FileInfo(Path.Combine(Location, filename));

            return fileInfo.Exists ? new GameBinary(fileInfo) : null;
        }
    }
}
