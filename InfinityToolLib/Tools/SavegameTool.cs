using System;
using System.Windows.Forms;

namespace NoDev.InfinityToolLib.Tools
{
    public class SavegameTool : Tool
    {
        private readonly Control _openButton;
        private readonly Control _saveButton;
        private readonly Control _closeButton;

        protected virtual string OpenFileFilter
        {
            get { return ""; } 
        }

        public SavegameTool()
        {
            _openButton = AddSideMenuButton("", "Open a saved game file");
            _openButton.Click += OnOpenButtonClick;

            _saveButton = AddSideMenuButton("", "Save changes");
            _saveButton.Click += OnSaveButtonClick;

            _closeButton = AddSideMenuButton("", "Close file");
            _closeButton.Click += OnCloseButtonClick;

            SetEnabledState(false);
        }

        protected virtual void OnOpenButtonClick(object sender, EventArgs e)
        {
            var filter = OpenFileFilter;

            if (!string.IsNullOrEmpty(filter))
            {
                filter += "|All Files (*.*)|*.*";
            }

            var ofd = new OpenFileDialog
            {
                Title = "Select a Saved Game File",
                Filter = filter
            };

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                OnSavegameSelected(ofd.FileName);
                SetEnabledState(true);
            }
            catch (Exception ex)
            {
                DialogBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxDefaultButton.Button1, MessageBoxIcon.Error);
            }
        }

        protected override sealed void SetEnabledState(bool enabled)
        {
            base.SetEnabledState(enabled);

            _saveButton.Enabled = enabled;
            _closeButton.Enabled = enabled;
        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            if (OnSave())
            {
                DialogBox.Show("Saved successfully!", "Saved", MessageBoxButtons.OK, 
                    MessageBoxDefaultButton.Button1, MessageBoxIcon.Information);
            }
        }

        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            if (OnClose())
            {
                SetEnabledState(false);
            }
        }

        protected virtual void OnSavegameSelected(string filename)
        {

        }

        protected virtual bool OnSave()
        {
            return true;
        }

        protected virtual bool OnClose()
        {
            return true;
        }
    }
}
