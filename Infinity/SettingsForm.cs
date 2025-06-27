using System;
using NoDev.Common.Storage;
using DevComponents.DotNetBar.Metro;

namespace NoDev.Infinity
{
    internal partial class SettingsForm : MetroForm
    {
        private readonly bool _doneLoading;
        private readonly ISettingsStorage _settings;

        internal SettingsForm(ISettingsStorage settings)
        {
            _settings = settings;

            InitializeComponent();

            /*** COLOR SCHEME ***/
            cbColorScheme.Items.AddRange(new object[]
            {
                "Default",
                "Red",
                "Green"
            });
            cbColorScheme.SelectedIndex = 0;

            /*** WINDOW TITLE ***/
            cbWindowTitleMode.SelectedIndexChanged += OnWindowTitleModeSelectedIndexChanged;
            cbWindowTitleMode.Items.AddRange(new object[]
            {
                "Default",
                "Randomize",
                "Custom"
            });
            cbWindowTitleMode.SelectedIndex = 0;

            txtWindowTitle.Enabled = false;

            foreach (var setting in settings)
                LoadSetting(setting.Key, setting.Value);

            _doneLoading = true;
        }

        private void LoadSetting(string setting, object value)
        {
            switch (setting)
            {
                case InfinitySetting.WindowTitleMode:
                    var i = Convert.ToInt32(value);
                    cbWindowTitleMode.SelectedIndex = i < 0 || i > 2 ? 0 : i;
                    txtWindowTitle.Enabled = cbWindowTitleMode.SelectedIndex == 2;
                    break;
                case InfinitySetting.CustomWindowTitleText:
                    txtWindowTitle.Text = (string)value;
                    break;
            }
        }

        private void OnWindowTitleModeSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_doneLoading) return;
            txtWindowTitle.Enabled = cbWindowTitleMode.SelectedIndex == 2;

            _settings[InfinitySetting.WindowTitleMode] = cbWindowTitleMode.SelectedIndex;
            _settings.Save();
        }

        private void txtWindowTitle_TextChanged(object sender, EventArgs e)
        {
            if (!_doneLoading) return;
            _settings[InfinitySetting.CustomWindowTitleText] = txtWindowTitle.Text;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}