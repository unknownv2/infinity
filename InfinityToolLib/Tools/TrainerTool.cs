using System;
using System.IO;
using System.Windows.Forms;
using NoDev.InfinityToolLib.Controls;
using NoDev.InfinityToolLib.Memory;

namespace NoDev.InfinityToolLib.Tools
{
    public class TrainerTool : Tool
    {
        private readonly SymbolBoxButton _openButton;
        private readonly SymbolBoxButton _refreshButton;
        private readonly SymbolBoxButton _closeButton;

        private ProcessMemory _procMem;

        public TrainerTool()
        {
            _openButton = AddSideMenuButton("", "Open game process");
            _openButton.Click += OnOpenButtonClick;

            _refreshButton = AddSideMenuButton("", "Refresh values");
            _refreshButton.Click += (s, e) => OnRefresh();
            _refreshButton.Visible = false;

            _closeButton = AddSideMenuButton("", "Close process");
            _closeButton.Click += OnCloseButtonClick;

            SetEnabledState(false);
        }

        private void OnOpenButtonClick(object sender, EventArgs e)
        {
            var installDir = (string)Settings["InstallDir"] ?? FindGameInstallDirectory();

            var customDir = false;

            selectDir:
            if (installDir == null || !Directory.Exists(installDir))
            {
                var fbd = new FolderBrowserDialog
                {
                    Description = "Select the location of your game installation.",
                    ShowNewFolderButton = false,
                    SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                };

                if (fbd.ShowDialog() != DialogResult.OK)
                    return;

                installDir = fbd.SelectedPath;

                customDir = true;
            }

            var game = new Game(installDir);

            var binary = FindGameBinary(game);

            if (binary == null)
            {
                if (!customDir)
                {
                    installDir = null;
                    goto selectDir;
                }
                
                DialogBox.Show("We could not find your game files. Your installation may be corrupted or your game version may not be supported by this tool.");

                return;
            }

            var proc = binary.FindRunningProcess();

            if (proc == null)
            {
                var res = DialogBox.ShowYesNoCancel(
                    "The game must be running to use these mods.\r\n\r\nWould you like to start it now?", 
                    "Game Not Running");

                if (res != DialogResult.Yes)
                    return;

                proc = binary.FindRunningProcess() ?? binary.RunGame(2);
                    
                if (proc == null)
                {
                    DialogBox.Show("The game failed to start or is taking too long.\r\n\r\nWait until it pops up before trying again.");
                    return;
                }
            }

            if (customDir)
            {
                Settings["InstallDir"] = installDir;
                Settings.Save();
            }

            Game = game;

            try
            {
                _procMem = new ProcessMemory(proc.Id);
            }
            catch (UnauthorizedAccessException)
            {
                DialogBox.Show("Infinity must be running with administrative privileges to continue.");
                return;
            }

            try
            {
                OnProcessOpened(_procMem);
            }
            catch (Exception ex)
            {
                DialogBox.ShowException(ex);
                return;
            }

            SetEnabledState(true);
        }

        protected virtual void OnProcessOpened(ProcessMemory procMem)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnProcessClosed()
        {
            
        }

        protected virtual void OnRefresh()
        {
            throw new NotImplementedException();
        }

        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            _procMem.Close();

            OnProcessClosed();

            SetEnabledState(false);
        }

        protected override sealed void SetEnabledState(bool enabled)
        {
            base.SetEnabledState(enabled);

            _refreshButton.Enabled = enabled;
            _closeButton.Enabled = enabled;
            _openButton.Enabled = !enabled;
        }

        protected Game Game { get; private set; }

        protected virtual string FindGameInstallDirectory()
        {
            throw new NotImplementedException();
        }

        protected virtual GameBinary FindGameBinary(Game game)
        {
            throw new NotImplementedException();
        }
    }
}
