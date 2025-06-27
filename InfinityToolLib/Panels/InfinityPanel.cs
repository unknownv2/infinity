using NoDev.InfinityToolLib.Security;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NoDev.InfinityToolLib.Panels
{
    [ToolboxItem(false)]
    public class InfinityPanel : UserControl
    {
        protected InfinityPanel()
        {
#if !DEBUG
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                throw new System.Exception();
#else
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                InfinityStyleManager.LoadDefaultStyle();
            else
#endif
            AssemblyProtection.EnsureProtected();

            SuspendLayout();
            Name = "InfinityTool";
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96F, 96F);
            Size = new Size(600, 400);
            MinimumSize = Size;
            MaximumSize = new Size(600, 10000);
            BackColor = Color.FromArgb(250, 250, 250);
            ResumeLayout(false);

            InfinityStyleManager.ColorsChanged += (s, e) => OnStyleColorsChanged(e.Colors);
        }

        protected virtual void OnStyleColorsChanged(InfinityStyleColors colors)
        {
            
        }
    }
}
