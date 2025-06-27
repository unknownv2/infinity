using NoDev.InfinityToolLib.Tools;
using NoDev.Common.IO;

namespace NoDev.Infinity.Tools.Save_Editor
{
    internal partial class MyTool : SavegameTool
    {
        public MyTool()
        {
            InitializeComponent();
        }
		
		protected override void OnSavegameSelected(string filename)
        {
            var io = new EndianIO(filename);
			
			
        }
    }
}
