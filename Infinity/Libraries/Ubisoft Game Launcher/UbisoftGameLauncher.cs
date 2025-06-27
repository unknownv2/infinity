using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoDev.Infinity.Libraries.Ubisoft_Game_Launcher
{
    class UbisoftGameLauncher
    {
        internal static void GetLocalUsers()
        {
            var usersFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            usersFile += @"\Ubisoft Game Launcher\users.dat";
        }
    }
}
