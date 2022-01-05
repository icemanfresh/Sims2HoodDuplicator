using Microsoft.Win32;
using System;
using System.Linq;
using System.IO;

namespace Sims2HoodDuplicator
{
    internal class Functions
    {
        internal static string GetNeighborhoodsDirectory()
        {
            string keyName = String.Format(@"Software{0}\EA Games\The Sims 2", Environment.Is64BitProcess ? @"\WOW6432Node" : "");
            string valueName = "DisplayName";
            var sims2Subkey = Registry.LocalMachine.OpenSubKey(keyName, false);
            if (sims2Subkey == null)
            {
                sims2Subkey.Close();
                return null;
            }

            string displayName = sims2Subkey.GetValue(valueName).ToString();
            sims2Subkey.Close();
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EA Games", displayName, "Neighborhoods");
        }

        internal static string GetNeighborhoodTemplatesDirectory(string pack)
        {
            string keyName = String.Format(@"Software{0}\EA Games\{1}", Environment.Is64BitProcess ? @"\WOW6432Node" : "", pack);
            string valueName = "Install Dir";
            var packSubkey = Registry.LocalMachine.OpenSubKey(keyName, false);
            if (packSubkey == null)
            {
                packSubkey.Close();
                return null;
            }

            string installationDir = packSubkey.GetValue(valueName).ToString();
            packSubkey.Close();
            return Path.Combine(installationDir, "TSData", "Res", "UserData", "Neighborhoods");
        }

        internal static string DuplicateNeighborhoodTemplate(string sourceDir)
        {
            string newFolderName = GetNextUnusedNeighborhoodFolder();
            if (newFolderName == null)
            {
                return null;
            }

            string neighborhoodsDir = GetNeighborhoodsDirectory();
            if (!Directory.Exists(neighborhoodsDir))
            {
                Directory.CreateDirectory(neighborhoodsDir);
            }
            string newDirectory = Path.Combine(GetNeighborhoodsDirectory(), newFolderName);
            string sourceFolderName = Path.GetFileName(sourceDir);
            string destFolderName = Path.GetFileName(newDirectory);
            DirectoryCopy(sourceDir, newDirectory, sourceFolderName, destFolderName);

            return newFolderName;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, string sourceFolderName, string destFolderName)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();  
            Directory.CreateDirectory(destDirName);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string fileName = file.Name;
                if (fileName.StartsWith(sourceFolderName))
                {
                    fileName = fileName.Replace(sourceFolderName, destFolderName);
                }
                string tempPath = Path.Combine(destDirName, fileName);
                file.CopyTo(tempPath, false);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, sourceFolderName, destFolderName);
            }
        }

        internal static string GetNextUnusedNeighborhoodFolder()
        {
            string[] neighborhoods = Directory.GetDirectories(GetNeighborhoodsDirectory()).Select(dir => Path.GetFileName(dir)).ToArray();
            char initialLetter = 'N';
            short neighborhoodNumber = 1;
            while (true)
            {
                string newFolderName = initialLetter + neighborhoodNumber.ToString("D3");
                if (!neighborhoods.Contains(newFolderName))
                {
                    return newFolderName;
                }

                if (initialLetter == 'M' && neighborhoodNumber == 999)
                {
                    return null;
                }

                if (neighborhoodNumber == 999)
                {
                    initialLetter = (char) ((int) initialLetter + 1);
                    if (initialLetter == (char) ((int)'Z' + 1))
                    {
                        initialLetter = 'A';
                    }
                    neighborhoodNumber = 1;
                }
                else
                {
                    neighborhoodNumber++;
                }
            }
        }
    }
}
