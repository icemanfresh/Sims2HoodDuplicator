using Microsoft.Win32;
using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

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

            List<uint> usedNIDs = GetUsedNIDs();
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

            EditCopiedNIDs(newDirectory, destFolderName, usedNIDs);

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

        private static void EditCopiedNIDs(string newFolder, string destFolderName, List<uint> usedNIDs)
        {
            FileInfo[] files = new DirectoryInfo(newFolder).GetFiles();
            string storytellingDirectory = Path.Combine(newFolder, "Storytelling");
            foreach (FileInfo file in files)
            {
                if (NeighborhoodRegex.IsMatch(file.Name))
                {
                    var oldID = GetNeighborhoodNID(file.FullName);
                    uint newID = 0;
                    for (uint i = 1; i <= uint.MaxValue; i++)
                    {
                        if (usedNIDs.BinarySearch(i) < 0)
                        {
                            newID = i;
                            break;
                        }
                    }

                    if (newID == 0) // ran out of possible NIDs for some reason
                    {
                        return;
                    }

                    usedNIDs.Add(newID);
                    usedNIDs.Sort();
                    using (FileStream stream = File.Open(file.FullName, FileMode.OpenOrCreate))
                    {
                        stream.Seek(oldID.Location, SeekOrigin.Begin);
                        byte[] data = new byte[4];
                        stream.Read(data, 0, 4);
                        uint neighborhoodBytes = BitConverter.ToUInt32(data, 0);
                        var nameBytes = Encoding.Unicode.GetBytes(destFolderName);
                        byte[] newNeighborhoodBytes = new byte[4];
                        int index = 0;
                        foreach (byte b in nameBytes)
                        {
                            if (b != 0)
                            {
                                newNeighborhoodBytes[index++] = b;
                            }
                        }

                        if (neighborhoodBytes != 4) // Really wish Maxis kept folder names fixed at 4 characters :\
                        {
                            string tempFile = Path.GetTempFileName();
                            using (FileStream temp = File.Open(tempFile, FileMode.Create, FileAccess.ReadWrite))
                            {
                                stream.CopyTo(temp);
                                stream.Seek(oldID.Location, SeekOrigin.Begin);
                                stream.SetLength(stream.Position);
                                data = new byte[4] { 0x04, 0x00, 0x00, 0x00 };
                                stream.Write(data, 0, 4);
                                stream.Write(newNeighborhoodBytes, 0, 4);
                                long currentPosition = stream.Position;
                                temp.Seek(neighborhoodBytes, SeekOrigin.Begin);
                                temp.CopyTo(stream);
                                stream.Seek(currentPosition, SeekOrigin.Begin);
                            }
                            File.Delete(tempFile);
                        }
                        else
                        {
                            stream.Write(newNeighborhoodBytes, 0, 4);
                        }
                        data[0] = (byte)newID;
                        data[1] = (byte)(newID >> 8);
                        data[2] = (byte)(newID >> 16);
                        data[3] = (byte)(newID >> 24);
                        stream.Write(data, 0, 4);
                    }

                    UpdateNeighborhoodStorytelling(storytellingDirectory, oldID.NID, newID);
                }
            }
        }

        private static void UpdateNeighborhoodStorytelling(string storytellingDir, uint oldID, uint newID)
        {
            if (!Directory.Exists(storytellingDir))
            {
                return;
            }

            string oldIDHex = oldID.ToString("x8");
            string newIDHex = newID.ToString("x8");
            string hyphenatedOldID = string.Format("_{0}_", oldIDHex);
            string hyphenatedNewID = string.Format("_{0}_", newIDHex);
            string webentryOldID = string.Format("webentry_{0}", oldIDHex);
            string webentryNewID = string.Format("webentry_{0}", newIDHex);
            string oldIDNode = string.Format("<ID>0x{0}</ID>", oldIDHex);
            string newIDNode = string.Format("<ID>0x{0}</ID>", newIDHex);
            FileInfo[] screenshots = new DirectoryInfo(storytellingDir).GetFiles();
            foreach (FileInfo screenshot in screenshots)
            {
                if (screenshot.Name.Contains(hyphenatedOldID))
                {
                    File.Move(screenshot.FullName, screenshot.FullName.Replace(hyphenatedOldID, hyphenatedNewID));
                }
                else if (screenshot.Name.Contains(webentryOldID))
                {
                    string xml = File.ReadAllText(screenshot.FullName);
                    xml = xml.Replace(hyphenatedOldID, hyphenatedNewID);
                    xml = xml.Replace(oldIDNode, newIDNode);
                    File.WriteAllText(screenshot.FullName, xml);
                    File.Move(screenshot.FullName, screenshot.FullName.Replace(webentryOldID, webentryNewID));
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

        internal static List<uint> GetUsedNIDs()
        {
            List<uint> usedNIDs = new List<uint>();
            DirectoryInfo directoryInfo = Directory.CreateDirectory(GetUserNeighborhoodsDirectory());
            DirectoryInfo[] userNeighborhoods = directoryInfo.GetDirectories();
            foreach (DirectoryInfo neighborhood in userNeighborhoods)
            {
                FileInfo[] packages = neighborhood.GetFiles();
                foreach (FileInfo package in packages)
                {
                    if (NeighborhoodRegex.IsMatch(package.Name))
                    {
                        NeighborhoodID info = GetNeighborhoodNID(package.FullName);
                        if (info != null)
                        {
                            usedNIDs.Add(info.NID);
                        }
                    }
                }
            }

            usedNIDs.Sort();
            return usedNIDs;
        }

        internal static NeighborhoodID GetNeighborhoodNID(string neighborhoodFile)
        {
            using (FileStream stream = File.OpenRead(neighborhoodFile))
            {
                if (stream.Length < 4)
                {
                    return null;
                }

                byte[] data = new byte[4];
                stream.Read(data, 0, 4);
                uint location = 0;
                bool hasNoLocation = true;
                int lastRead;
                do
                {
                    if (BitConverter.ToUInt32(data, 0) == 0xac8a7a2e)
                    {
                        uint value;
                        do
                        {
                            stream.Read(data, 0, 4);
                            value = BitConverter.ToUInt32(data, 0);
                        }
                        while (value == 0xffffffff || value == 0x00000000 || value == 0x00000001);

                        location = value;
                        hasNoLocation = false;
                        break;
                    }

                    lastRead = stream.ReadByte();
                    data[0] = data[1];
                    data[1] = data[2];
                    data[2] = data[3];
                    data[3] = (byte) lastRead;
                }
                while (lastRead != -1);

                if (hasNoLocation)
                {
                    return null;
                }

                stream.Seek(location + 4, SeekOrigin.Begin);
                location = ((uint)stream.Position);
                stream.Read(data, 0, 4);
                uint neighborhoodBytes = BitConverter.ToUInt32(data, 0);
                stream.Seek(neighborhoodBytes, SeekOrigin.Current);
                stream.Read(data, 0, 4);
                uint NID = BitConverter.ToUInt32(data, 0);
                return new NeighborhoodID(NID, location);
            }
        }

        public class NeighborhoodID
        {
            private readonly uint myNID;
            private readonly uint myLocation;

            public NeighborhoodID(uint NID, uint location)
            {
                this.myNID = NID;
                this.myLocation = location;
            }

            public uint NID { get { return myNID; } }
            public uint Location { get { return myLocation; } }
        }

        private static readonly Regex NeighborhoodRegex = new Regex(@"(Neighborhood|(Suburb|Downtown|University|Vacation)[0-9]{3})\.package");
        private static long CopiedBytes, TotalBytes;
    }
}
