using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Deployment.Application;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using NoDev.Common.Storage;
using NoDev.Infinity.Controls;
using NoDev.Infinity.Tools;
using NoDev.InfinityToolLib;
using NoDev.Infinity.User;
using NoDev.Infinity.Network.Api;
using NoDev.Infinity.Network;
using NoDev.Infinity.Tools.Info;
using NoDev.Infinity.Tools.Literals;
using DevComponents.DotNetBar.Metro;

namespace NoDev.Infinity
{
    internal partial class Main : MetroAppForm
    {
        private SettingsForm _settingsForm;
        private readonly InfinitySettings _settings;

        private readonly InfinityTab _homeTab;
        private readonly HomePanel _homePanel;
        private readonly Lazy<NotLoggedInPanel> _notLoggedInPanel;
        private readonly Lazy<UpgradeToDiamondPanel> _upgradeToDiamondPanel; 
        private readonly Lazy<FailedToLoadToolPanel> _failedToLoadPanel; 

        private readonly ToolFactory _toolFactory;
        private readonly IToolImageRetriever _toolImageRetriever;
        private readonly IToolInfoRetriever _toolInfoRetriever;
        private readonly IGameInfoRetriever _gameInfoRetriever;

        private Dictionary<string, DownloadGameToolsPanel> _downloadGameToolsPanels;

        private Control ActivePanel
        {
            // ReSharper disable once UnusedMember.Local
            get { return panelContainer.Controls.Count == 1 ? panelContainer.Controls[0] : null; }
            set
            {
                if (panelContainer.Controls.Count == 1)
                {
                    if (panelContainer.Controls[0] == value)
                        return;

                    panelContainer.Controls.RemoveAt(0);
                }
                    
                value.Location = new Point(0, 0);
                value.Size = new Size(600, panelContainer.Height);
                value.Anchor = AnchorStyles.Bottom | AnchorStyles.Top;

                panelContainer.Controls.Add(value);
            }
        }
        
        internal Main()
        {
            InfinityStyleManager.LoadDefaultStyle();

            InitializeComponent();

            gameList.BackColor = InfinityStyleManager.Colors.BaseColor;

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            _settings = new InfinitySettings(Storage.GetInstance(Environment.SpecialFolder.LocalApplicationData).GetFilePath("Settings.json"));
            _settings.SettingChanged += (s, e) => ApplySetting((ISettingsStorage)s, e.SettingName, e.SettingValue);
            
            foreach (var setting in _settings)
                ApplySetting(_settings, setting.Key, setting.Value);

            mainShell.SettingsButtonClick += SettingsButtonClick;
            mainShell.HelpButtonClick += HelpButtonClick;

            gameList.SelectedGameChanged += OnSelectedGameChanged;
            tabStrip.SelectedTabChanged += OnSelectedTabChanged;

            _homeTab = new InfinityTab("Home", "Home");
            _homePanel = new HomePanel();

            _notLoggedInPanel = new Lazy<NotLoggedInPanel>(() => new NotLoggedInPanel());
            _failedToLoadPanel = new Lazy<FailedToLoadToolPanel>(() => new FailedToLoadToolPanel());
            _upgradeToDiamondPanel = new Lazy<UpgradeToDiamondPanel>(() => new UpgradeToDiamondPanel());

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

#if DEBUG
            _gameInfoRetriever = new ManifestFileGameInfoRetriever(baseDir + "Games.json");
            var debugFinder = new DebugToolRetriever(baseDir + @"..\..\..\Tools", "Manifest.json");
            _toolInfoRetriever = debugFinder;
            _toolImageRetriever = debugFinder;
#else
            var manifestRetriever = new ManifestFileToolAndGameInfoRetriever(baseDir + "Manifest.json");
            _toolInfoRetriever = manifestRetriever;
            _gameInfoRetriever = manifestRetriever;

            var toolImageRetriever = new ToolImageRetriever();
            toolImageRetriever.Retrievers.Add(new DirectoryToolImageRetriever(baseDir + "tools"));
            _toolImageRetriever = toolImageRetriever;
#endif

            _toolFactory = new ToolFactory(_toolImageRetriever);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            SetColors(InfinityStyleManager.Colors);

            InfinityStyleManager.ColorsChanged += (s, a) => SetColors(a.Colors);

            foreach (var game in _gameInfoRetriever.RetrieveGameInfo().Where(g => _toolInfoRetriever.RetrieveToolInfoByGameId(g.ID).Any()))
                gameList.AddOrUpdateGame(game.ID, game.Name, game.Thumbnail);

            tabStrip.Tabs.Add(_homeTab);
            tabStrip.SelectedTab = _homeTab;

            Me.OnLogin += OnLogin;
            Me.OnLogout += OnLogout;
        }

        private void OnLogout(object sender, EventArgs e)
        {
            _homeTab.Text = "Home";
        }

        private void OnLogin(object sender, EventArgs e)
        {
            _homeTab.Text = Me.Username;
        }

        private void SetColors(InfinityStyleColors colors)
        {
            gameList.BackColor = colors.BaseColor;
            gameList.GameColor = colors.Darker;
            gameList.GameColorHover = colors.Darkest;
            gameList.GameColorClick = Color.FromArgb(0x3868b1);
            gameList.TextColor = colors.Lightest;
            gameList.GameColorSelected = Color.FromArgb(0xfafafa);
        }

        private void OnSelectedGameChanged(object sender, SelectedGameChangedEventArgs e)
        {
            var gameId = e.GameID;

            tabStrip.Tabs.Clear();

            if (gameId != null)
            {
                var tools = _toolInfoRetriever.RetrieveToolInfoByGameId(gameId).ToArray();

                foreach (var tool in tools)
                    tabStrip.Tabs.Add(new InfinityTab(tool.ID, tool.Name));

                tabStrip.SelectTabByID(tools[0].ID);
            }
            else
            {
                tabStrip.Tabs.Add(_homeTab);
                tabStrip.SelectedTab = _homeTab;
            }
        }

        private void OnSelectedTabChanged(object sender, SelectedTabChangedEventArgs e)
        {
            var tab = e.Tab;

            if (tab == _homeTab)
            {
                homeIcon.Visible = false;
                ActivePanel = _homePanel;
            }
            else
            {
                homeIcon.Visible = true;
                ShowTool(tab.ID);
            }
        }

        private async void ShowTool(string toolId)
        {
            var toolInfo = _toolInfoRetriever.RetrieveToolInfo(toolId);

            // locally check if the user is allowed to access the tool
            if (!Me.HasAccess(toolInfo.AccessLevel))
            {
                if (Me.IsLoggedIn)
                {
                    // if the user is logged in, they must upgrade to Diamond to use the tool
                    ActivePanel = _upgradeToDiamondPanel.Value;
                }
                else
                {
                    // otherwise, he or she has to login
                    ActivePanel = _notLoggedInPanel.Value;
                }

                return;
            }

            // check if we have a copy of the tool DLL
            if (!_toolImageRetriever.HasToolImage(toolId))
            {
                if (_downloadGameToolsPanels == null)
                    _downloadGameToolsPanels = new Dictionary<string, DownloadGameToolsPanel>(1);

                // if we don't, ask the user if he or she wants to download the tools for the selected game
                var panel = _downloadGameToolsPanels.GetOrDefault(toolInfo.GameID);

                if (panel == null)
                {
                    panel = new DownloadGameToolsPanel(
                        toolInfo.GameID,
                        _gameInfoRetriever.RetrieveGameInfo(toolInfo.GameID).Name,
                        _toolInfoRetriever.RetrieveToolInfoByGameId(toolInfo.GameID).Select(t => t.Name).ToArray());

                    panel.DownloadRequested += OnGameToolsDownloadRequested;
                    panel.CancelRequested += OnGamesToolDownloadCancelRequested;

                    _downloadGameToolsPanels.Add(toolInfo.GameID, panel);
                }

                ActivePanel = panel;
                return;
            }

            // literals are only stored for diamond+ tools. this is to limit the attack vector to people who pay $5.
            if (toolInfo.AccessLevel < AccessLevel.Diamond)
            {
                ActivePanel = _toolFactory.GetToolInstance(toolId).Panel;
                return;
            }

            // retrieve the literals from the server
            var req = ApiRequestFactory.Create(Method.GET, string.Format("/tools/{0}/literals", toolId));

            var response = await req.SendAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ActivePanel = _upgradeToDiamondPanel.Value;
                throw new Exception(response.Headers["X-Message"]);
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                ActivePanel = _upgradeToDiamondPanel.Value;
                throw new Exception("Unsupported server response code.");
            }

            var literals = response.ContentLength == 0
                ? null
                : new EncryptedBinaryLiteralDeserializer(Server.Key).Deserialize(await response.ToByteArrayAsync());

            var tool = _toolFactory.GetToolInstance(toolId, literals);

            // show the tool or the failed to load panel
            ActivePanel = tool == null ? _failedToLoadPanel.Value : tool.Panel;
        }

        private static void OnGamesToolDownloadCancelRequested(object sender, EventArgs e)
        {
            if (DialogBox.ShowYesNoCancel("Are you sure you want to cancel the download?", "Confirm") != DialogResult.Yes)
                return;

            var panel = (DownloadGameToolsPanel)sender;
            panel.State = DownloadGameToolsPanel.PanelState.Idle;
        }

        private bool _clickOnceGroupsBootstrapped;
        private void OnGameToolsDownloadRequested(object sender, EventArgs e)
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
            {
                DialogBox.Show("The requested tools cannot be downloaded at this time.", "Application Error",
                    MessageBoxButtons.OK, MessageBoxDefaultButton.Button1, MessageBoxIcon.Error);
                return;
            }

            var app = ApplicationDeployment.CurrentDeployment;

            var panel = (DownloadGameToolsPanel)sender;
            panel.State = DownloadGameToolsPanel.PanelState.Downloading;

            if (!_clickOnceGroupsBootstrapped)
            {
                app.DownloadFileGroupProgressChanged += OnClickOnceDownloadFileGroupProgressChanged;
                app.DownloadFileGroupCompleted += OnClickOnceDownloadFileGroupCompleted;
                _clickOnceGroupsBootstrapped = true;
            }

            app.DownloadFileGroupAsync(panel.GameId, panel);
        }

        private void OnClickOnceDownloadFileGroupCompleted(object sender, DownloadFileGroupCompletedEventArgs e)
        {
            var panel = (DownloadGameToolsPanel)e.UserState;
            panel.State = DownloadGameToolsPanel.PanelState.Idle;

            var gameInfo = _gameInfoRetriever.RetrieveGameInfo(panel.GameId);

            if (ActivePanel != panel && DialogBox.ShowYesNoCancel(
                string.Format("The {0} tools have been downloaded!{1}{1}Go to them now?", gameInfo.Name,Environment.NewLine),
                "Tools Downloaded", MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                gameList.SelectedGame = gameInfo.ID;
            }
            else
            {
                ShowTool(tabStrip.SelectedTab.ID);
            }
        }

        private static void OnClickOnceDownloadFileGroupProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            var panel = (DownloadGameToolsPanel)e.UserState;
            panel.BytesToDownload = 100;
            panel.BytesDownloaded = e.ProgressPercentage;
        }

        private void ApplySetting(ISettingsStorage settings, string setting, object value)
        {
            switch (setting)
            {
                case InfinitySetting.WindowTitleMode:
                    switch (Convert.ToInt32(value))
                    {
                        case (int)WindowTitleMode.Randomize:
                            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                            var bytes = Rand.NextBytes(Rand.Next(10, 15));
                            var titleChars = new char[bytes.Length];
                            for (var x = 0; x < titleChars.Length; x++)
                                titleChars[x] = chars[bytes[x] % 62];
                            Text = new string(titleChars);
                            break;
                        case (int)WindowTitleMode.Custom:
                            if (!settings.ContainsKey(InfinitySetting.CustomWindowTitleText))
                                Text = @" ";
                            else
                            {
                                var windowText = (string)settings[InfinitySetting.CustomWindowTitleText];
                                Text = windowText.Length == 0 ? @" " : windowText;
                            }
                            break;
                        default:
                            Text = @"Infinity - PC Game Modder";
                            break;
                    }
                    break;
                case InfinitySetting.CustomWindowTitleText:
                    if (settings.ContainsKey(InfinitySetting.WindowTitleMode))
                    {
                        var windowTitleMode = (WindowTitleMode)Convert.ToInt32(settings[InfinitySetting.WindowTitleMode]);
                        if (windowTitleMode == WindowTitleMode.Custom)
                        {
                            var text = (string)value;
                            Text = text.Length == 0 ? " " : text;
                        }
                    }
                    break;
            }
        }

        private void SettingsButtonClick(object sender, EventArgs e)
        {
            if (_settingsForm != null)
                _settingsForm.BringToFront();
            else
            {
                _settingsForm = new SettingsForm(_settings);
                _settingsForm.FormClosed += (s, v) =>
                {
                    _settings.Save();
                    _settingsForm = null;
                };
                _settingsForm.Show();
            }
        }

        private static void HelpButtonClick(object sender, EventArgs e)
        {
            MessageBox.Show(@"Not here yet!");
        }

        private void HomeIcon_MouseEnter(object sender, EventArgs e)
        {
            homeIcon.SymbolColor = Color.WhiteSmoke;
        }

        private void HomeIcon_MouseLeave(object sender, EventArgs e)
        {
            homeIcon.SymbolColor = Color.White;
        }

        private void HomeIcon_Click(object sender, EventArgs e)
        {
            gameList.SelectedGame = null;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        } 
    }
}
