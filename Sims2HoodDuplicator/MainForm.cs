using System;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Collections.Generic;

namespace Sims2HoodDuplicator
{
    public partial class MainForm : Form
    {
        public MainForm(Mutex mutex)
        {
            if (Duplication.GetUserNeighborhoodsDirectory() == null)
            {
                MessageBox.Show(Strings.Not_Installed);
                mutex.Close();
                return;
            }
            this.mutex = mutex;
            Application.ApplicationExit += OnExit;
            InitializeComponent();
            LocalizeUI();
            PopulateNewNeighborhoodDropdown();
            GetExistingNeighborhoods();
        }

        private void LocalizeUI()
        {
            Text = Strings.Program_Name;
            NewRadioButton.Text = Strings.New;
            ExistingRadioButton.Text = Strings.Existing;
            ExistingRadioButton.Location = new System.Drawing.Point(NewRadioButton.Right, ExistingRadioButton.Top);
            DuplicateButton.Text = Strings.Duplicate;
        }

        private void PopulateNewNeighborhoodDropdown()
        {
            if (NewNeighborhoods == null)
            {
                NewNeighborhoods = new List<Neighborhood>();
                NeighborhoodDropdown.DisplayMember = "Name";
                NeighborhoodDropdown.ValueMember = "Directory";
                foreach (string pack in Packs)
                {
                    string neighborhoodTemplatesDirectory = Duplication.GetNeighborhoodTemplatesDirectory(pack);
                    if (neighborhoodTemplatesDirectory != null)
                    {
                        string[] dirs = Directory.GetDirectories(neighborhoodTemplatesDirectory);
                        foreach (string dir in dirs)
                        {
                            string folderName = Path.GetFileName(dir);
                            if (FolderNeighborhoodNameMappings.ContainsKey(folderName))
                            {
                                NewNeighborhoods.Add(new Neighborhood(FolderNeighborhoodNameMappings[folderName], dir));
                            }
                        }
                    }
                }
            }
            NeighborhoodDropdown.Items.Clear();
            NeighborhoodDropdown.Items.AddRange(NewNeighborhoods.ToArray());
            NeighborhoodDropdown.SelectedIndex = 0;
            Neighborhood selected = (Neighborhood) NeighborhoodDropdown.SelectedItem;
            DisplayNeighborhoodImage(selected.Directory);
        }

        private void GetExistingNeighborhoods()
        {
            ExistingNeighborhoods = new List<Neighborhood>();
            if (!Directory.Exists(UserNeighborhoodsDirectory))
            {
                Directory.CreateDirectory(UserNeighborhoodsDirectory);
            }
            string[] hoods = Directory.GetDirectories(UserNeighborhoodsDirectory);
            foreach (string hood in hoods)
            {
                string folderName = Path.GetFileName(hood);
                if (!folderName.Equals("Tutorial"))
                {
                    ExistingNeighborhoods.Add(new Neighborhood(folderName, hood));
                }
            }
            if (ExistingNeighborhoods.Count == 0)
            {
                ExistingRadioButton.Enabled = false;
            }
        }

        private void PopulateExistingNeighborhoodDropdown()
        {
            NeighborhoodDropdown.Items.Clear();
            NeighborhoodDropdown.Items.AddRange(ExistingNeighborhoods.ToArray());
            NeighborhoodDropdown.SelectedIndex = 0;
            Neighborhood selected = (Neighborhood)NeighborhoodDropdown.SelectedItem;
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

        private readonly string[] Packs = new string[] { "The Sims 2", "The Sims 2 Seasons", "The Sims 2 FreeTime", "The Sims 2 Apartment Life" };
        private readonly Dictionary<string, string> FolderNeighborhoodNameMappings = new Dictionary<string, string>
        {
            { "N001", Strings.Pleasantview },
            { "N002", Strings.Strangetown },
            { "N003", Strings.Veronaville },
            { "G001", Strings.Riverblossom_Hills },
            { "F001", Strings.Desiderata_Valley },
            { "E001", Strings.Belladonna_Cove }
        };
        private List<Neighborhood> NewNeighborhoods, ExistingNeighborhoods;
        private Thread DuplicationThread;
        private readonly string UserNeighborhoodsDirectory = Duplication.GetUserNeighborhoodsDirectory();
        private string CurrentCreatedFolder;
        private readonly Mutex mutex;

        public class Neighborhood
        {
            private readonly string myName;
            private readonly string myDirectory;

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
            Duplicate(((Neighborhood)NeighborhoodDropdown.SelectedItem));
        }

        private void Duplicate(Neighborhood neighborhood)
        {
            ToggleUIEnabled(false);
            if (DuplicationThread == null)
            {
                CurrentCreatedFolder = Duplication.GetNextUnusedNeighborhoodFolder();
                if (!Directory.Exists(neighborhood.Directory))
                {
                    RefreshDropDown();
                    return;
                }
                LaunchDuplicationThread(neighborhood);
            }
            else
            {
                CancelDuplication();
            }
        }

        private void LaunchDuplicationThread(Neighborhood neighborhood)
        {
            DuplicationThread = new Thread(() =>
            {
                try
                {
                    string newNeighborhoodName = Duplication.DuplicateNeighborhoodTemplate(neighborhood.Directory, CopyProgressBar);
                    if (newNeighborhoodName == null)
                    {
                        MainPanel.Invoke(new Action(() =>
                        {
                            ToggleUIEnabled(false);
                            MessageBox.Show(string.Format(Strings.Error_Copying, neighborhood.Name), Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                    else
                    {
                        MainPanel.Invoke(new Action(() =>
                        {
                            ToggleUIEnabled(false);
                            Neighborhood newNeighborhood = new Neighborhood(newNeighborhoodName, Path.Combine(UserNeighborhoodsDirectory, newNeighborhoodName));
                            ExistingNeighborhoods.Add(newNeighborhood);
                            if (ExistingRadioButton.Checked)
                            {
                                NeighborhoodDropdown.Items.Add(newNeighborhood);
                            }
                            MessageBox.Show(string.Format(Strings.Success, newNeighborhoodName));
                        }));
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception)
                {
                    MainPanel.Invoke(new Action(() =>
                    {
                        ToggleUIEnabled(false);
                        MessageBox.Show(string.Format(Strings.Error_Copying, neighborhood.Name), Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
                finally
                {
                    DuplicationThread = null;
                    MainPanel.Invoke(new Action(() =>
                    {
                        CopyProgressBar.Value = 0;
                        DuplicateButton.Text = Strings.Duplicate;
                        ToggleUIEnabled(true);
                    }));
                }
            });
            DuplicationThread.Start();
            DuplicateButton.Text = Strings.Cancel;
            DuplicateButton.Enabled = true;
        }

        private void CancelDuplication()
        {
            DuplicationThread.Abort();
            new Thread(() =>
            {
                string createdDirectory = Path.Combine(UserNeighborhoodsDirectory, CurrentCreatedFolder);
                if (Directory.Exists(createdDirectory))
                {
                    Directory.Delete(createdDirectory, true);
                }
                DuplicationThread = null;
                MainPanel.Invoke(new Action(() =>
                {
                    CopyProgressBar.Value = 0;
                    DuplicateButton.Text = Strings.Duplicate;
                    ToggleUIEnabled(true);
                }));
            }).Start();
        }

        private void ToggleUIEnabled(bool enabled)
        {
            NeighborhoodDropdown.Enabled = enabled;
            DuplicateButton.Enabled = enabled;
            NewRadioButton.Enabled = enabled;
            ExistingRadioButton.Enabled = enabled && ExistingNeighborhoods.Count > 0;
        }

        private void NewExistingRadioGroup_CheckedChanged(object sender, EventArgs e)
        {
            if (NewRadioButton.Checked)
            {
                PopulateNewNeighborhoodDropdown();
            }
            else
            {
                PopulateExistingNeighborhoodDropdown();
            }
        }

        private void RefreshDropDown()
        {
            ToggleUIEnabled(false);
            MessageBox.Show(Strings.No_Longer_Exists);
            if (NewRadioButton.Checked)
            {
                NewNeighborhoods = null;
                PopulateNewNeighborhoodDropdown();
            }
            else if (ExistingRadioButton.Checked)
            {
                ExistingNeighborhoods.Clear();
                GetExistingNeighborhoods();
                if (ExistingNeighborhoods.Count == 0)
                {
                    NewRadioButton.Checked = true;
                    ExistingRadioButton.Checked = false;
                }
                else
                {
                    PopulateExistingNeighborhoodDropdown();
                }
            }
            Neighborhood selected = (Neighborhood)NeighborhoodDropdown.SelectedItem;
            DisplayNeighborhoodImage(selected.Directory);
            ToggleUIEnabled(true);
        }

        private void NeighborhoodDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            Neighborhood selected = (Neighborhood)NeighborhoodDropdown.SelectedItem;
            if (!Directory.Exists(selected.Directory))
            {
                RefreshDropDown();
            }
            else
            {
                DisplayNeighborhoodImage(selected.Directory);
            }
        }

        internal void OnExit(object sender, EventArgs e)
        {
            mutex.Close();
        }
    }
}
