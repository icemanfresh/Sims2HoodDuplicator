namespace Sims2HoodDuplicator
{
  partial class VariantDropdown
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

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
      this.VariantSelection = new System.Windows.Forms.ComboBox();
      this.SelectButton = new System.Windows.Forms.Button();
      this.VariantPrompt = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // VariantSelection
      // 
      this.VariantSelection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.VariantSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.VariantSelection.FormattingEnabled = true;
      this.VariantSelection.Location = new System.Drawing.Point(10, 74);
      this.VariantSelection.Name = "VariantSelection";
      this.VariantSelection.Size = new System.Drawing.Size(402, 21);
      this.VariantSelection.TabIndex = 0;
      this.VariantSelection.SelectedIndexChanged += new System.EventHandler(this.VariantSelection_SelectedIndexChanged);
      // 
      // SelectButton
      // 
      this.SelectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.SelectButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.SelectButton.Location = new System.Drawing.Point(337, 101);
      this.SelectButton.Name = "SelectButton";
      this.SelectButton.Size = new System.Drawing.Size(75, 23);
      this.SelectButton.TabIndex = 1;
      this.SelectButton.Text = "Select";
      this.SelectButton.UseVisualStyleBackColor = true;
      // 
      // VariantPrompt
      // 
      this.VariantPrompt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.VariantPrompt.AutoEllipsis = true;
      this.VariantPrompt.Location = new System.Drawing.Point(12, 9);
      this.VariantPrompt.Name = "VariantPrompt";
      this.VariantPrompt.Size = new System.Drawing.Size(402, 57);
      this.VariantPrompt.TabIndex = 2;
      this.VariantPrompt.Text = "ultiple installations of The Sims 2 were found. Which edition do you want to work" +
    " with?";
      this.VariantPrompt.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
      // 
      // VariantDropdown
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(424, 133);
      this.Controls.Add(this.VariantPrompt);
      this.Controls.Add(this.SelectButton);
      this.Controls.Add(this.VariantSelection);
      this.MinimumSize = new System.Drawing.Size(440, 172);
      this.Name = "VariantDropdown";
      this.Text = "Select Version";
      this.Load += new System.EventHandler(this.VariantDropdown_Load);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ComboBox VariantSelection;
    private System.Windows.Forms.Button SelectButton;
    private System.Windows.Forms.Label VariantPrompt;
  }
}