using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Sims2HoodDuplicator
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length >= 2)
            {
                if (args[0].Equals("-u"))
                {
                    try
                    {
                        Update.CopyUpdateFile(args[1]);
                        return;
                    }
                    catch (IOException)
                    {
                        MessageBox.Show(Strings.Update_Failed);
                    }
                }
                else if (args[0].Equals("-d"))
                {
                    Update.DeleteUpdateFile(args[1]);
                }
            }

            Mutex mutex = new Mutex(true, "The Sims 2 Hood Duplicator", out bool uniqueInstance);
            if (!uniqueInstance)
            {
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (Update.HasUpdate() && (args.Length < 2 || !args[0].Equals("-u")))
            {
                DialogResult result = MessageBox.Show(Strings.Update_Available, "", MessageBoxButtons.YesNo);
                if(result == DialogResult.Yes)
                {
                    while (true)
                    {
                        try
                        {
                            Update.DownloadUpdate();
                            return;
                        }
                        catch (Exception)
                        {
                            result = MessageBox.Show(Strings.Update_Problem, "", MessageBoxButtons.RetryCancel);
                            if(result == DialogResult.Retry)
                            {
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            Application.Run(new MainForm(mutex));
        }
    }
}
