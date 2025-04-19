using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Sims2HoodDuplicator
{
  internal enum Sims2Variant
  {
    Classic,
    Legacy,
    NotInstalled,
  }

  internal class Duplication
  {
    internal static RegistryKey GetClassicEditionSubkey()
    {
      string keyName = string.Format(
        @"Software{0}\EA Games\The Sims 2",
        Environment.Is64BitProcess ? @"\WOW6432Node" : ""
      );
      return Registry.LocalMachine.OpenSubKey(keyName, false);
    }

    internal static RegistryKey GetLegacyEditionSubkey()
    {
      return Registry.CurrentUser.OpenSubKey(@"Software\Electronic Arts\The Sims 2 Ultimate Collection 25", false);
    }

    internal static string GetUserNeighborhoodsDirectory(Sims2Variant variant)
    {
      RegistryKey sims2Subkey = null;
      switch (variant)
      {
        case Sims2Variant.Classic:
          sims2Subkey = GetClassicEditionSubkey();
          break;
        case Sims2Variant.Legacy:
          sims2Subkey = GetLegacyEditionSubkey();
          break;
      }

      if (sims2Subkey == null)
      {
        return null;
      }

      string valueName = "DisplayName";
      var displayName = sims2Subkey.GetValue(valueName)?.ToString();
      sims2Subkey.Close();
      if (displayName == null)
      {
        return null;
      }

      return Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "EA Games",
        displayName,
        "Neighborhoods"
      );
    }

    internal static string GetClassicNeighborhoodTemplatesDirectory(string pack, bool getStorytellingTemplates = false)
    {
      if (!pack.Equals("The Sims 2") && UltimateCollectionIsInstalled())
      {
        return GetUCNeighborhoodTemplatesDirectory(pack, getStorytellingTemplates);
      }

      string keyName = string.Format(
        @"Software{0}\EA Games\{1}",
        Environment.Is64BitProcess ? @"\WOW6432Node" : "",
        pack
      );
      var packSubkey = Registry.LocalMachine.OpenSubKey(keyName, false);
      if (packSubkey == null)
      {
        return null;
      }

      string valueName = "Install Dir";
      string installationDir = packSubkey.GetValue(valueName)?.ToString();
      packSubkey.Close();
      if (installationDir == null)
      {
        return null;
      }

      var neighborhoodDir = Path.Combine(
        installationDir,
        "TSData",
        "Res",
        "UserData",
        getStorytellingTemplates ? "Storytelling" : "Neighborhoods"
      );
      if (Directory.Exists(neighborhoodDir))
      {
        return neighborhoodDir;
      }

      return null;
    }

    internal static string GetLegacyNeighborhoodTemplatesDirectory(string pack, bool getStorytellingTemplates = false)
    {
      // You can install both the Steam and EA versions at the same time...but since they have the same templates,
      // we just need to retrieve one of them

      // Steam
      var steamSubkey = Registry.LocalMachine.OpenSubKey(
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 3314070",
        false
      );

      if (steamSubkey != null)
      {
        var installationDir = steamSubkey.GetValue("InstallLocation")?.ToString();
        steamSubkey.Close();
        if (installationDir == null)
        {
          return null;
        }

        var neighborhoodDir = Path.Combine(
          installationDir,
          pack,
          "TSData",
          "Res",
          "UserData",
          getStorytellingTemplates ? "Storytelling" : "Neighborhoods"
        );
        if (Directory.Exists(neighborhoodDir))
        {
          return neighborhoodDir;
        }
      }

      // EA
      string keyName = string.Format(
        @"SOFTWARE{0}\Maxis\The Sims 2 Legacy",
        Environment.Is64BitProcess ? @"\WOW6432Node" : "",
        pack
      );
      var eaSubkey = Registry.LocalMachine.OpenSubKey(keyName, false);
      if (eaSubkey != null)
      {
        var installationDir = eaSubkey.GetValue("Install Dir")?.ToString();
        eaSubkey.Close();
        if (installationDir == null)
        {
          return null;
        }

        var neighborhoodDir = Path.Combine(
          installationDir,
          pack,
          "TSData",
          "Res",
          "UserData",
          getStorytellingTemplates ? "Storytelling" : "Neighborhoods"
        );
        if (Directory.Exists(neighborhoodDir))
        {
          return neighborhoodDir;
        }
      }

      return null;
    }

    private static bool UltimateCollectionIsInstalled()
    {
      string keyName = string.Format(
        @"Software{0}\EA Games\The Sims 2",
        Environment.Is64BitProcess ? @"\WOW6432Node" : ""
      );
      var packSubkey = Registry.LocalMachine.OpenSubKey(keyName, false);
      if (packSubkey == null)
      {
        return false;
      }

      string valueName = "Install Dir";
      string installationDir = packSubkey.GetValue(valueName)?.ToString() ?? string.Empty;
      packSubkey.Close();
      return installationDir.Contains("Ultimate Collection");
    }

    private static string GetUCNeighborhoodTemplatesDirectory(string pack, bool getStorytellingTemplates)
    {
      string keyName = string.Format(
        @"Software{0}\EA Games\The Sims 2",
        Environment.Is64BitProcess ? @"\WOW6432Node" : ""
      );
      var packSubkey = Registry.LocalMachine.OpenSubKey(keyName, false);
      if (packSubkey == null)
      {
        return null;
      }

      string valueName = "Install Dir";
      var installationDir = packSubkey.GetValue(valueName)?.ToString();
      packSubkey.Close();
      if (installationDir == null)
      {
        return null;
      }

      var neighborhoodDir = Path.Combine(
        installationDir,
        "..",
        "..",
        PackToUCFolderMappings[pack],
        "TSData",
        "Res",
        "UserData",
        getStorytellingTemplates ? "Storytelling" : "Neighborhoods"
      );
      if (Directory.Exists(neighborhoodDir))
      {
        return neighborhoodDir;
      }

      return null;
    }

    internal static string DuplicateNeighborhoodTemplate(
      Sims2Variant variant,
      string sourceDir,
      ProgressBar progressBar = null,
      List<string> sourceStorytellingFiles = null
    )
    {
      string newFolderName = GetNextUnusedNeighborhoodFolder(variant);
      if (newFolderName == null)
      {
        return null;
      }

      string neighborhoodsDir = GetUserNeighborhoodsDirectory(variant);
      if (!Directory.Exists(neighborhoodsDir))
      {
        Directory.CreateDirectory(neighborhoodsDir);
      }

      List<uint> usedNIDs = GetUsedNIDs(variant);
      string newDirectory = Path.Combine(GetUserNeighborhoodsDirectory(variant), newFolderName);
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

    private static void DirectoryCopy(
      string sourceDirName,
      string destDirName,
      string sourceFolderName,
      string destFolderName,
      ProgressBar progressBar = null
    )
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
        string destFile = Path.Combine(destDirName, fileName);
        if (!destFile.EndsWith(".bkp") && !destFile.EndsWith(".bak"))
        {
          file.CopyTo(destFile, false);
        }
        CopiedBytes += file.Length;
        progressBar?.Invoke(new Action(() => progressBar.Value = (int)((double)CopiedBytes / TotalBytes * 100)));
      }

      foreach (DirectoryInfo subdir in dirs)
      {
        string tempPath = Path.Combine(destDirName, subdir.Name);
        DirectoryCopy(subdir.FullName, tempPath, sourceFolderName, destFolderName, progressBar);
      }
    }

    private static void StorytellingCopy(
      List<string> sourceStorytellingFiles,
      string destDirectory,
      ProgressBar progressBar = null
    )
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
            progressBar?.Invoke(new Action(() => progressBar.Value = (int)((double)CopiedBytes / TotalBytes * 100)));
          }
        }
      }
    }

    private static void EditCopiedNIDs(string newFolder, string destFolderName, List<uint> usedNIDs)
    {
      FileInfo[] files = new DirectoryInfo(newFolder).GetFiles();
      Array.Sort(files, new FileInfoComparer());
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
            byte[] newNeighborhoodBytes = new byte[neighborhoodBytes];
            int nameBytesIndex = 0;
            for (int i = 0; i < neighborhoodBytes; )
            {
              if (nameBytesIndex == nameBytes.Length)
              {
                newNeighborhoodBytes[i++] = (byte)char.MinValue;
              }
              else if (nameBytes[nameBytesIndex] != 0)
              {
                newNeighborhoodBytes[i++] = nameBytes[nameBytesIndex++];
              }
              else
              {
                nameBytesIndex++;
              }
            }

            stream.Write(newNeighborhoodBytes, 0, (int)neighborhoodBytes);
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
          string newName = screenshot.FullName.Replace(hyphenatedOldID, hyphenatedNewID);
          if (!File.Exists(newName))
          {
            File.Move(screenshot.FullName, newName);
          }
        }
        else if (screenshot.Name.Contains(webentryOldID))
        {
          string newName = screenshot.FullName.Replace(webentryOldID, webentryNewID);
          if (!File.Exists(newName))
          {
            string xml = File.ReadAllText(screenshot.FullName);
            xml = xml.Replace(hyphenatedOldID, hyphenatedNewID);
            xml = xml.Replace(oldIDNode, newIDNode);
            File.WriteAllText(screenshot.FullName, xml);
            File.Move(screenshot.FullName, newName);
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

    internal static string GetNextUnusedNeighborhoodFolder(Sims2Variant variant)
    {
      string userNeighborhoodsDirectory = GetUserNeighborhoodsDirectory(variant);
      if (!Directory.Exists(userNeighborhoodsDirectory))
      {
        Directory.CreateDirectory(userNeighborhoodsDirectory);
      }
      string[] neighborhoods = Directory
        .GetDirectories(userNeighborhoodsDirectory)
        .Select(dir => Path.GetFileName(dir))
        .ToArray();
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
          initialLetter = (char)((int)initialLetter + 1);
          if (initialLetter == (char)((int)'Z' + 1))
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

    internal static List<uint> GetUsedNIDs(Sims2Variant variant)
    {
      List<uint> usedNIDs = new List<uint>();
      DirectoryInfo directoryInfo = Directory.CreateDirectory(GetUserNeighborhoodsDirectory(variant));
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
            } while (value == 0xffffffff || value == 0x00000000 || value == 0x00000001);

            location = value;
            hasNoLocation = false;
            break;
          }

          lastRead = stream.ReadByte();
          data[0] = data[1];
          data[1] = data[2];
          data[2] = data[3];
          data[3] = (byte)lastRead;
        } while (lastRead != -1);

        if (hasNoLocation)
        {
          return null;
        }

        stream.Seek(location + 4, SeekOrigin.Begin);
        location = (uint)stream.Position;
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

      public uint NID
      {
        get { return myNID; }
      }
      public uint Location
      {
        get { return myLocation; }
      }
    }

    private class FileInfoComparer : IComparer<FileInfo>
    {
      public int Compare(FileInfo x, FileInfo y)
      {
        string xName = x.Name;
        string yName = y.Name;
        string[] categories = new string[] { "Neighborhood", "University", "Downtown", "Suburb", "Vacation" };
        int xIndex = 5;
        int yIndex = 5;
        for (int i = 0; i < categories.Length; i++)
        {
          string category = categories[i];
          if (xName.Contains(category) && yName.Contains(category))
          {
            return xName.CompareTo(yName);
          }

          if (xName.Contains(category))
          {
            xIndex = i;
          }

          if (yName.Contains(category))
          {
            yIndex = i;
          }
        }

        return xIndex - yIndex;
      }
    }

    private static readonly Regex NeighborhoodRegex = new Regex(
      @"(Neighborhood|(Suburb|Downtown|University|Vacation)[0-9]{3})\.package"
    );
    private static readonly Dictionary<string, string> PackToUCFolderMappings = new Dictionary<string, string>
    {
      { "The Sims 2 Seasons", "Seasons" },
      { "The Sims 2 FreeTime", "Free Time" },
      { "The Sims 2 Apartment Life", "Apartment Life" },
    };
    private static long CopiedBytes,
      TotalBytes;
  }
}
