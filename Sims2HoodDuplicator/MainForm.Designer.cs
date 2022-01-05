using System.Collections.Generic;

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
            this.MainPanel = new System.Windows.Forms.Panel();
            this.NeighborhoodImageBox = new System.Windows.Forms.PictureBox();
            this.CopyProgressBar = new System.Windows.Forms.ProgressBar();
            this.MainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NeighborhoodImageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // NeighborhoodDropdown
            // 
            this.NeighborhoodDropdown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NeighborhoodDropdown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.NeighborhoodDropdown.FormattingEnabled = true;
            this.NeighborhoodDropdown.Location = new System.Drawing.Point(10, 182);
            this.NeighborhoodDropdown.Name = "NeighborhoodDropdown";
            this.NeighborhoodDropdown.Size = new System.Drawing.Size(402, 21);
            this.NeighborhoodDropdown.TabIndex = 0;
            this.NeighborhoodDropdown.SelectedIndexChanged += new System.EventHandler(this.NeighborhoodDropdown_SelectedIndexChanged);
            // 
            // DuplicateButton
            // 
            this.DuplicateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DuplicateButton.AutoSize = true;
            this.DuplicateButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DuplicateButton.Location = new System.Drawing.Point(350, 227);
            this.DuplicateButton.Name = "DuplicateButton";
            this.DuplicateButton.Size = new System.Drawing.Size(62, 23);
            this.DuplicateButton.TabIndex = 1;
            this.DuplicateButton.Text = "Duplicate";
            this.DuplicateButton.UseVisualStyleBackColor = true;
            this.DuplicateButton.Click += new System.EventHandler(this.DuplicateButton_Click);
            // 
            // MainPanel
            // 
            this.MainPanel.Controls.Add(this.NeighborhoodImageBox);
            this.MainPanel.Controls.Add(this.CopyProgressBar);
            this.MainPanel.Controls.Add(this.NeighborhoodDropdown);
            this.MainPanel.Controls.Add(this.DuplicateButton);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = new System.Drawing.Point(0, 0);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(424, 262);
            this.MainPanel.TabIndex = 2;
            // 
            // NeighborhoodImageBox
            // 
            this.NeighborhoodImageBox.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.NeighborhoodImageBox.BackColor = System.Drawing.SystemColors.ControlDark;
            this.NeighborhoodImageBox.Location = new System.Drawing.Point(112, 16);
            this.NeighborhoodImageBox.Name = "NeighborhoodImageBox";
            this.NeighborhoodImageBox.Size = new System.Drawing.Size(200, 150);
            this.NeighborhoodImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.NeighborhoodImageBox.TabIndex = 3;
            this.NeighborhoodImageBox.TabStop = false;
            // 
            // CopyProgressBar
            // 
            this.CopyProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CopyProgressBar.Location = new System.Drawing.Point(10, 209);
            this.CopyProgressBar.Name = "CopyProgressBar";
            this.CopyProgressBar.Size = new System.Drawing.Size(402, 12);
            this.CopyProgressBar.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(424, 262);
            this.Controls.Add(this.MainPanel);
            this.MinimumSize = new System.Drawing.Size(440, 301);
            this.Name = "MainForm";
            this.Text = "The Sims 2 Hood Duplicator";
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NeighborhoodImageBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
    }
}

