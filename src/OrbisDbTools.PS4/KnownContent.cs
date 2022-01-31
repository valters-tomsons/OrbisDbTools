using OrbisDbTools.PS4.Models;

namespace OrbisDbTools.PS4
{
    public static class KnownContent
    {
        /// <summary>
        /// List of known homebrew tools
        /// </summary>
        public static List<AppTitle> KnownHomebrewApps = new()
        {
            new AppTitle("AZIF00003", "Payload Guest"),
            new AppTitle("BREW00031", "Patch Installer"),
            new AppTitle("BREW00050", "Internal PKG Installer"),
            new AppTitle("FLTZ00003", "Remote PKG Installer"),
            new AppTitle("LAPY20001", "PS4-Xplorer")
        };

        /// <summary>
        /// List of Sony apps that require PSN connection
        /// </summary>
        public static List<AppTitle> KnownPsnApps = new()
        {
            new AppTitle("NPXS20108", "What's New"),
            new AppTitle("NPXS20105", "Live from PlayStation"),
            new AppTitle("NPXS20114", "PlayStation™Now"),
            new AppTitle("CUSA01697", "PlayStation™Now"),
            new AppTitle("NPXS20979", "PlayStation Store"),
            new AppTitle("CUSA00572", "SHAREfactory™"),
            new AppTitle("NPXS20118", "Share Play"),
            new AppTitle("NPXS20120", "PlayStation™Video"),
            new AppTitle("NPXS20107", "PlayStation™Video - My Videos"),
        };
    }
}