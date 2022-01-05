using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Sims2HoodDuplicator
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var neighborhoodsDirectory = Functions.GetNeighborhoodsDirectory();
            if (neighborhoodsDirectory == null)
            {
                MessageBox.Show(Strings.Not_Installed);
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm
            {
                NeighborhoodsDirectory = neighborhoodsDirectory
            });
        }
    }
}
