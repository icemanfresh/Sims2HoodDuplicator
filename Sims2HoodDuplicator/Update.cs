using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Sims2HoodDuplicator
{
    internal class Update
    {
        internal static bool HasUpdate()
        {
            using (FileStream hoodCheckerExe = File.OpenRead(Assembly.GetExecutingAssembly().Location))
            {
                long size = hoodCheckerExe.Length;
                string sha256 = GetSHA256Checksum(hoodCheckerExe);
                try
                {
                    var url = "https://raw.githubusercontent.com/icemanfresh/Sims2HoodDuplicator/master/latestbuild/latest.txt";
                    var client = new WebClient();
                    var stream = client.OpenRead(url);
                    var reader = new StreamReader(stream);
                    var updateSize = long.Parse(reader.ReadLine());
                    var updateSha256 = reader.ReadLine();

                    if (size != updateSize || !sha256.Equals(updateSha256))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch(Exception)
                {
                    return false;
                }
            }
        }

        internal static void DownloadUpdate()
        {
            string tempFile = Path.GetTempFileName().Replace(".tmp", ".exe");
            var url = "https://raw.githubusercontent.com/icemanfresh/Sims2HoodDuplicator/master/latestbuild/TS2HD.exe";
            var client = new WebClient();
            client.DownloadFile(url, tempFile);
            Process process = new Process();
            process.StartInfo.FileName = tempFile;
            process.StartInfo.Arguments = "-u " + Assembly.GetExecutingAssembly().Location;
            process.Start();
        }

        internal static void CopyUpdateFile(string location)
        {
            string updateFileLocation = Assembly.GetExecutingAssembly().Location;
            File.Copy(updateFileLocation, location, true);
            Process process = new Process();
            process.StartInfo.FileName = location;
            process.StartInfo.Arguments = "-d " + updateFileLocation;
            process.Start();
        }

        internal static void DeleteUpdateFile(string location)
        {
            try
            {
                File.Delete(location);
            }
            catch (IOException) { }
        }

        internal static string GetSHA256Checksum(FileStream fileStream)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(fileStream);
                var stringBuilder = new StringBuilder();
                foreach(byte b in hash)
                {
                    stringBuilder.Append(b.ToString("x2"));
                }

                return stringBuilder.ToString();
            }
        }
    }
}
