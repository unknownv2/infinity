using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace NoDev.Infinity.Libraries
{
    internal class OriginLibrary
    {
        internal string Location { get; private set; }
        internal IEnumerable<OriginGame> Games { get; private set; }

        internal OriginLibrary(string libraryDir)
        {
            Location = libraryDir;

            ReloadGames();
        }

        internal void ReloadGames()
        {
            var games = new List<OriginGame>();

            try
            {
                var gameDirs = Directory.GetDirectories(Location);

                foreach (var gameDir in gameDirs)
                {
                    var installFile = gameDir + @"\__Installer\installerdata.xml";

                    if (!File.Exists(installFile))
                        continue;

                    var contentId = ExtractContentIdFromInstallerData(installFile);

                    if (contentId != null && games.All(g => g.ID != contentId))
                        games.Add(new OriginGame(contentId, gameDir));
                }
            }
            catch
            {
                // library directory probably didn't exist
            }

            Games = games.AsReadOnly();
        }

        private static string ExtractContentIdFromInstallerData(string installerFile)
        {
            try
            {
                using (var io = File.OpenText(installerFile))
                {
                    string line;

                    /* we could parse the XML, but this is probably more efficient */
                    while ((line = io.ReadLine()) != null)
                    {
                        if (!line.Contains("<contentID>"))
                            continue;

                        var s = line.Split('>');

                        if (s.Length < 2)
                            continue;

                        s = s[1].Split('<');

                        if (s.Length != 1)
                            return s[0];
                    }
                }
            }
            catch
            {
                
            }

            return null;
        }

        internal static OriginLibrary GetCurrentUserLibrary()
        {
            var settingsFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Origin\local.xml";

            if (!File.Exists(settingsFile))
                return null;

            var xml = new XmlDocument();

            try
            {
                xml.Load(settingsFile);

                var settingNodes = xml.GetElementsByTagName("Setting");

                foreach (XmlNode node in settingNodes)
                {
                    if (node.Attributes == null)
                        continue;
                    
                    if (node.Attributes["key"].InnerText == "DownloadInPlaceDir")
                        return new OriginLibrary(node.Attributes["value"].InnerText);
                }

                return new OriginLibrary(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)+ @"\Origin Games"
                );
            }
            catch
            {
                
            }

            return null;
        }
    }

    internal class OriginGame
    {
        internal string ID { get; private set; }
        internal string InstallLocation { get; private set; }

        internal OriginGame(string id, string installLocation)
        {
            ID = id;
            InstallLocation = installLocation;
        }
    }
}
