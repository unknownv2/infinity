
namespace NoDev.Infinity.Libraries
{
    internal class SteamApp
    {
        internal int ID { get; private set; }
        internal string InstallLocation { get; private set; }

        internal SteamApp(int appId, string installLocation)
        {
            ID = appId;
            InstallLocation = installLocation;
        }
    }
}
