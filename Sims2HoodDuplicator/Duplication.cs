using Microsoft.Win32;
using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Sims2HoodDuplicator
{
    internal class Duplication
    {
        internal static string GetUserNeighborhoodsDirectory()
        {
            string keyName = string.Format(@"Software{0}\EA Games\The Sims 2", Environment.Is64BitProcess ? @"\WOW6432Node" : "");
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

        internal static string GetNeighborhoodTemplatesDirectory(string pack, bool getStorytellingTemplates = false)
        {
            string keyName = string.Format(@"Software{0}\EA Games\{1}", Environment.Is64BitProcess ? @"\WOW6432Node" : "", pack);
            string valueName = "Install Dir";
            var packSubkey = Registry.LocalMachine.OpenSubKey(keyName, false);
            if (packSubkey == null)
            {
                packSubkey.Close();
                return null;
            }

            string installationDir = packSubkey.GetValue(valueName).ToString();
            packSubkey.Close();
            return Path.Combine(installationDir, "TSData", "Res", "UserData", getStorytellingTemplates ? "Storytelling" : "Neighborhoods");
        }

        internal static string DuplicateNeighborhoodTemplate(string sourceDir, ProgressBar progressBar = null, List<string> sourceStorytellingFiles = null)
        {
            string newFolderName = GetNextUnusedNeighborhoodFolder();
            if (newFolderName == null)
            {
                return null;
            }

            string neighborhoodsDir = GetUserNeighborhoodsDirectory();
            if (!Directory.Exists(neighborhoodsDir))
            {
                Directory.CreateDirectory(neighborhoodsDir);
            }

            string newDirectory = Path.Combine(GetUserNeighborhoodsDirectory(), newFolderName);
            string sourceFolderName = Path.GetFileName(sourceDir);
            string destFolderName = Path.GetFileName(newDirectory);
            CopiedBytes = 0;
            TotalBytes = DirSize(new DirectoryInfo(sourceDir));
            if (sourceStorytellingFiles != null)
            {
                foreach (string image in sourceStorytellingFiles)
                {
                    if (File.Exists(image))
                    {
                        TotalBytes += new FileInfo(image).Length;
                    }
                }
            }
            if (TotalBytes == 0)
            {
                TotalBytes = 1;
                if (progressBar != null)
                {
                    progressBar.Value = 100;
                }
            }
            DirectoryCopy(sourceDir, newDirectory, sourceFolderName, destFolderName, progressBar);
            if (sourceStorytellingFiles != null)
            {
                StorytellingCopy(sourceStorytellingFiles, Path.Combine(newDirectory, "Storytelling"), progressBar);
            }

            return newFolderName;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, string sourceFolderName, string destFolderName, ProgressBar progressBar = null)
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
                CopiedBytes += file.Length;
                progressBar?.Invoke(new Action(() => progressBar.Value = (int) (((double) CopiedBytes / TotalBytes) * 100)));
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, sourceFolderName, destFolderName, progressBar);
            }
        }

        private static void StorytellingCopy(List<string> sourceStorytellingFiles, string destDirectory, ProgressBar progressBar = null)
        {
            if (!Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
            }

            foreach (string image in sourceStorytellingFiles)
            {
                if (File.Exists(image))
                {
                    FileInfo file = new FileInfo(image);
                    File.Copy(image, Path.Combine(destDirectory, file.Name), true);
                    if (progressBar != null)
                    {
                        CopiedBytes += file.Length;
                        progressBar?.Invoke(new Action(() => progressBar.Value = (int)(((double)CopiedBytes / TotalBytes) * 100)));
                    }
                }
            }
        }

        internal static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }

        internal static string GetNextUnusedNeighborhoodFolder()
        {
            string userNeighborhoodsDirectory = GetUserNeighborhoodsDirectory();
            if (!Directory.Exists(userNeighborhoodsDirectory))
            {
                Directory.CreateDirectory(userNeighborhoodsDirectory);
            }
            string[] neighborhoods = Directory.GetDirectories(userNeighborhoodsDirectory).Select(dir => Path.GetFileName(dir)).ToArray();
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

        private static long CopiedBytes, TotalBytes;
    }
}
