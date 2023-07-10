using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;

namespace ServerModManager
{
    [HarmonyPatch(typeof(ConnectionManager), nameof(ConnectionManager.Connect))]
    internal class ConnectionManagerPatch
    {
        public static void Prefix(GameServerInfo _gameServerInfo)
        {
            try
            {

                string serverName = _gameServerInfo.GetValue(GameInfoString.GameHost);
                Log.Out($"SERVER MOD MANAGER - Connecting to {serverName}");

                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string sevenDaysModsPath = Path.Combine(appDataPath, "7DaysToDie", "Mods");
                string modsPath = Path.Combine(appDataPath, "ServerModManager", serverName);
                List<string> dirs = new List<string>(Directory.EnumerateDirectories(modsPath));

                foreach (string dir in dirs)
                {
                    if (!Directory.Exists(dir))
                    {
                        Log.Out($"Directory doesn't exist: {dir}");
                        continue;
                    }
                    if (!File.Exists(Path.Combine(dir, "ModInfo.xml")))
                    {
                        Log.Out($"Folder doesn't contain a mod: {dir}");
                        continue;
                    }

                    DirectoryInfo dirInfo = new DirectoryInfo(dir);

                    string destination = Path.Combine(sevenDaysModsPath, dirInfo.Name);
                    ModUtilities.CopyDirectory(dir, destination);
                    Log.Out($"Copied {dirInfo.Name}");
                }

                ModManager.LoadMods();
                Log.Out("Loaded the rest of the mods");
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
    }

    public class ModUtilities
    {

        public static void CopyDirectory(string source, string destination, bool recursive = true)
        {
            var dir = new DirectoryInfo(source);
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            Log.Out("Copying " + source + " to " + destination);

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destination);
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destination, file.Name);
                file.CopyTo(targetFilePath);
            }
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destination, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
