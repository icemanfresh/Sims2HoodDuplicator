using System;
using System.Windows.Forms;

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
    }
}
