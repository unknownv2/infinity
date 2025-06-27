using NoDev.Common.IO;
using NoDev.InfinityToolLib.Memory;
using NoDev.InfinityToolLib.Tools;
using Microsoft.Win32;

namespace NoDev.InfinityTool
{
    internal sealed partial class Main : TrainerTool
    {
        public Main()
        {
            InitializeComponent();
        }

        protected override string FindGameInstallDirectory()
        {
            var value = (string)Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Ubisoft\Launcher\Installs\420", 
                "InstallDir", 
                null
            );

            return value != null ? value.TrimEnd('/').Replace('/', '\\') : null;
        }

        protected override GameBinary FindGameBinary(Game game)
        {
            return game.FindBinary(@"bin\FarCry4.exe");
        }

        protected override void OnProcessOpened(ProcessMemory procMem)
        {
            var io = new EndianIO(procMem.OpenMainModuleImageStream());

            
        }
    }
}
