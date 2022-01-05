using System.Collections.Generic;
using System.IO;

namespace Sims2HoodDuplicator
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private string[] _packs = new string[] { "The Sims 2", "The Sims 2 Seasons", "The Sims 2 FreeTime", "The Sims 2 Apartment Life" };
        private Dictionary<string, string> _folderNeighborhoodNameMappings = new Dictionary<string, string>
        {
            { "N001", Strings.Pleasantview },
            { "N002", Strings.Strangetown },
            { "N003", Strings.Veronaville },
            { "G001", Strings.Riverblossom_Hills },
            { "F001", Strings.Desiderata_Valley },
            { "E001", Strings.Belladonna_Cove }
        };

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.NeighborhoodDropdown = new System.Windows.Forms.ComboBox();
            this.DuplicateButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.CopyProgressBar = new System.Windows.Forms.ProgressBar();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // NeighborhoodDropdown
            // 
            this.NeighborhoodDropdown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NeighborhoodDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.NeighborhoodDropdown.FormattingEnabled = true;
            this.NeighborhoodDropdown.Location = new System.Drawing.Point(12, 12);
            this.NeighborhoodDropdown.Name = "NeighborhoodDropdown";
            this.NeighborhoodDropdown.Size = new System.Drawing.Size(402, 21);
            this.NeighborhoodDropdown.TabIndex = 0;
            // 
            // DuplicateButton
            // 
            this.DuplicateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DuplicateButton.AutoSize = true;
            this.DuplicateButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DuplicateButton.Location = new System.Drawing.Point(352, 57);
            this.DuplicateButton.Name = "DuplicateButton";
            this.DuplicateButton.Size = new System.Drawing.Size(62, 23);
            this.DuplicateButton.TabIndex = 1;
            this.DuplicateButton.Text = "Duplicate";
            this.DuplicateButton.UseVisualStyleBackColor = true;
            this.DuplicateButton.Click += new System.EventHandler(this.DuplicateButton_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.CopyProgressBar);
            this.panel1.Controls.Add(this.NeighborhoodDropdown);
            this.panel1.Controls.Add(this.DuplicateButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(424, 92);
            this.panel1.TabIndex = 2;
            // 
            // CopyProgressBar
            // 
            this.CopyProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CopyProgressBar.Location = new System.Drawing.Point(12, 39);
            this.CopyProgressBar.Name = "CopyProgressBar";
            this.CopyProgressBar.Size = new System.Drawing.Size(402, 12);
            this.CopyProgressBar.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 92);
            this.Controls.Add(this.panel1);
            this.MinimumSize = new System.Drawing.Size(440, 131);
            this.Name = "MainForm";
            this.Text = "The Sims 2 Hood Duplicator";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

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
        }

        private System.Windows.Forms.ComboBox NeighborhoodDropdown;
        private System.Windows.Forms.Button DuplicateButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ProgressBar CopyProgressBar;

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
    }
}

