using System;
using System.Windows.Forms;
using DevComponents.DotNetBar;

namespace NoDev.InfinityToolLib
{
    public static class DialogBox
    {
        public static DialogResult Show(string message)
        {
            return Show(message, null, MessageBoxButtons.OK, MessageBoxDefaultButton.Button1, MessageBoxIcon.Information);
        }

        public static DialogResult Show(string message, string title)
        {
            return Show(message, title, MessageBoxButtons.OK, MessageBoxDefaultButton.Button1, MessageBoxIcon.Information);
        }

        public static DialogResult Show(string message, string title, MessageBoxIcon icon)
        {
            return Show(message, title, MessageBoxButtons.OK, MessageBoxDefaultButton.Button1, icon);
        }

        public static DialogResult Show(string message, string title, MessageBoxButtons buttons)
        {
            return Show(message, title, buttons, MessageBoxDefaultButton.Button1, MessageBoxIcon.Information);
        }

        public static DialogResult Show(string message, string title, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton)
        {
            return Show(message, title, buttons, MessageBoxDefaultButton.Button1, MessageBoxIcon.Information);
        }

        public static DialogResult Show(string message, string title, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, MessageBoxIcon icon)
        {
            return MessageBoxEx.Show(Application.OpenForms[0], message, title, buttons, icon, defaultButton);
        }

        public static DialogResult ShowYesNoCancel(string message, string title, MessageBoxIcon icon = MessageBoxIcon.Question, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button3)
        {
            return Show(message, title, MessageBoxButtons.YesNoCancel, defaultButton, icon);
        }

        public static void ShowException(Exception ex)
        {
            Show(ex.Message, "An Error Has Occured", MessageBoxIcon.Error);
        }
    }
}
