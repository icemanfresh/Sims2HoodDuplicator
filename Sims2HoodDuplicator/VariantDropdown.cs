using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Sims2HoodDuplicator.MainForm;

namespace Sims2HoodDuplicator
{
  public partial class VariantDropdown : Form
  {
    internal Sims2Variant Variant { get; private set; }

    public VariantDropdown()
    {
      Icon = Properties.Resources.Icon;
      InitializeComponent();
      LocalizeUI();
    }

    private void VariantDropdown_Load(object sender, EventArgs e) { }

    private void LocalizeUI()
    {
      Text = Strings.Select_Version;
      VariantPrompt.Text = Strings.Multiple_Variants;
      SelectButton.Text = Strings.Select;
      VariantSelection.Items.AddRange(new string[] { Strings.The_Sims_2_Classic, Strings.The_Sims_2_Legacy });
      VariantSelection.SelectedIndex = 0;
    }

    private void VariantSelection_SelectedIndexChanged(object sender, EventArgs e)
    {
      Variant = (Sims2Variant)VariantSelection.SelectedIndex;
    }
  }
}
