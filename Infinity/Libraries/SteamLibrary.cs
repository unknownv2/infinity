using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Win32;

namespace NoDev.Infinity.Libraries
{
    internal class SteamLibrary
    {
        internal string Location { get; private set; }
        internal IEnumerable<SteamApp> Apps { get; private set; } 

        private readonly string _appsDirectory;

        internal SteamLibrary(string libraryDirectory)
        {
            Location = libraryDirectory;
            _appsDirectory = libraryDirectory + @"\steamapps";

            ReloadApps();
        }

        internal void ReloadApps()
        {
            var apps = new List<SteamApp>();

            try
            {
                var appManifests = Directory.GetFiles(_appsDirectory, "appmanifest_*.acf", SearchOption.TopDirectoryOnly);

                foreach (var app in appManifests.Select(ParseConfigFile).Where(app => app != null && app.ContainsKey("appid") && app.ContainsKey("installdir")))
                {
                    int appId;

                    if (!int.TryParse(app["appid"], out appId))
                        continue;

                    if (apps.Any(a => a.ID == appId))
                        continue;

                    var installDir = app["installdir"];

                    if (installDir[1] != ':')
                        installDir = Path.Combine(_appsDirectory, installDir);

                    apps.Add(new SteamApp(appId, installDir));
                }
            }
            catch
            {
                // library directory probably didn't exist
            }

            Apps = apps.AsReadOnly();
        }

        internal bool ContainsApp(int appId)
        {
            return Apps.Any(a => a.ID == appId);
        }

        internal static SteamLibrary GetDefaultLibrary()
        {
            try
            {
                var dir = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null);

                return dir != null ? new SteamLibrary(dir) : null;
            }
            catch
            {
                return null;
            }
        }

        internal IEnumerable<SteamLibrary> GetExternalLibraries()
        {
            var libs = new List<SteamLibrary>();

            var libFile = _appsDirectory + @"\libraryfolders.vdf";

            if (!File.Exists(libFile))
                return libs.AsReadOnly();

            var libConfig = ParseConfigFile(libFile);

            if (libConfig == null)
                return libs.AsReadOnly();

            foreach (var lib in libConfig)
            {
                int libId;

                if (!int.TryParse(lib.Key, out libId))
                    continue;

                libs.Add(new SteamLibrary(lib.Value));
            }

            return libs.AsReadOnly();
        }

        private static IReadOnlyDictionary<string, string> ParseConfigFile(string file)
        {
            try
            {
                var vars = new Dictionary<string, string>();

                using (var io = File.OpenText(file))
                {
                    string line;

                    while ((line = io.ReadLine()) != null)
                    {
                        if (line.Length <= 2 || line[0] != '\t' || line[1] != '"')
                            continue;

                        var p = line.Split('"');

                        if (p.Length >= 4)
                            vars[p[1].ToLower()] = p[3].Replace(@"\\", @"\");
                    }
                }
                
                return new ReadOnlyDictionary<string, string>(vars);
            }
            catch
            {
                return null;
            }
        }
    }
}
