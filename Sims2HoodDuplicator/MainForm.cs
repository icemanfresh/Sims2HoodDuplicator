using System;
using System.Windows.Forms;
using System.IO;

namespace Sims2HoodDuplicator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            if (Functions.GetNeighborhoodsDirectory() == null)
            {
                MessageBox.Show(Strings.Not_Installed);
                return;
            }
            InitializeComponent();
            LocalizeUI();
            PopulateNeighborhoodDropdown();
        }

        private void LocalizeUI()
        {
            Text = Strings.Program_Name;
            DuplicateButton.Text = Strings.Duplicate;
        }

        private void PopulateNeighborhoodDropdown()
        {
            NeighborhoodDropdown.DisplayMember = "Name";
            NeighborhoodDropdown.ValueMember = "Directory";
            foreach (string pack in _packs)
            {
                string neighborhoodTemplatesDirectory = Functions.GetNeighborhoodTemplatesDirectory(pack);
                if (neighborhoodTemplatesDirectory != null)
                {
                    string[] dirs = Directory.GetDirectories(neighborhoodTemplatesDirectory);
                    foreach (string dir in dirs)
                    {
                        string folderName = Path.GetFileName(dir);
                        if (_folderNeighborhoodNameMappings.ContainsKey(folderName))
                        {
                            NeighborhoodDropdown.Items.Add(new Neighborhood(_folderNeighborhoodNameMappings[folderName], dir));
                        }
                    }
                }
            }
            NeighborhoodDropdown.SelectedIndex = 0;
            Neighborhood selected = (Neighborhood) NeighborhoodDropdown.SelectedItem;
            DisplayNeighborhoodImage(selected.Directory);
        }

        private void DisplayNeighborhoodImage(string sourceDir)
        {
            string[] files = Directory.GetFiles(sourceDir);
            foreach (string file in files)
            {
                if (file.EndsWith("Neighborhood.png"))
                {
                    NeighborhoodImageBox.ImageLocation = file;
                    return;
                }
            }
            NeighborhoodImageBox.ImageLocation = null;
        }

        private System.Windows.Forms.ComboBox NeighborhoodDropdown;
        private System.Windows.Forms.Button DuplicateButton;
        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.ProgressBar CopyProgressBar;
        private System.Windows.Forms.PictureBox NeighborhoodImageBox;

        public class Neighborhood
        {
            private string myName;
            private string myDirectory;

            public Neighborhood(string strName, string strDirectory)
            {
                this.myName = strName;
                this.myDirectory = strDirectory;
            }

            public string Name
            {
                get
                {
                    return myName;
                }
            }

            public string Directory
            {
                get
                {
                    return myDirectory;
                }
            }
        }

        private void DuplicateButton_Click(object sender, EventArgs e)
        {
            DuplicateButton.Enabled = false;
            var neighborhood = ((Neighborhood)NeighborhoodDropdown.SelectedItem);
            try
            {
                string newNeighborhoodName = Functions.DuplicateNeighborhoodTemplate(neighborhood.Directory);
                if (newNeighborhoodName == null)
                {
                    MessageBox.Show(String.Format(Strings.Error_Copying, neighborhood.Name), Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(String.Format(Strings.Success, newNeighborhoodName));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format(Strings.Error_Copying, neighborhood.Name), Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                DuplicateButton.Enabled = true;
            }
        }

        private void NeighborhoodDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            Neighborhood selected = (Neighborhood)NeighborhoodDropdown.SelectedItem;
            DisplayNeighborhoodImage(selected.Directory);
        }
    }
}
