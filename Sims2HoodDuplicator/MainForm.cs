﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Sims2HoodDuplicator
{
  public partial class MainForm : Form
  {
    public MainForm(Mutex mutex)
    {
      Icon = Properties.Resources.Icon;
      var isClassicEditionInstalled = Duplication.GetClassicEditionSubkey() != null;
      var isLegacyEditionInstalled = Duplication.GetLegacyEditionSubkey() != null;

      if (isClassicEditionInstalled && isLegacyEditionInstalled)
      {
        var variantDropdown = new VariantDropdown();
        var result = variantDropdown.ShowDialog();
        if (result == DialogResult.Cancel)
        {
          ExitImmediately(mutex);
          return;
        }
        variant = variantDropdown.Variant;
      }
      else
      {
        variant =
          isClassicEditionInstalled ? Sims2Variant.Classic
          : isLegacyEditionInstalled ? Sims2Variant.Legacy
          : Sims2Variant.NotInstalled;
      }

      if (variant == Sims2Variant.NotInstalled)
      {
        MessageBox.Show(Strings.Not_Installed);
        ExitImmediately(mutex);
        return;
      }
      this.mutex = mutex;
      UserNeighborhoodsDirectory = Duplication.GetUserNeighborhoodsDirectory(variant);
      Application.ApplicationExit += OnExit;
      InitializeComponent();
      InitializeFolderBrowser();
      LocalizeUI();
      PopulateNewNeighborhoodDropdown();
      GetExistingNeighborhoods();
    }

    private void InitializeFolderBrowser()
    {
      FolderBrowser = new CommonOpenFileDialog
      {
        InitialDirectory = Duplication.GetUserNeighborhoodsDirectory(variant),
        IsFolderPicker = true,
      };
    }

    private void LocalizeUI()
    {
      Text = Strings.Program_Name;
      NewRadioButton.Text = Strings.New;
      ExistingRadioButton.Text = Strings.Existing;
      ExistingRadioButton.Location = new System.Drawing.Point(NewRadioButton.Right, ExistingRadioButton.Top);
      DuplicateButton.Text = Strings.Duplicate;
      SelectFolderButton.Text = Strings.Select_Folder;
    }

    private void PopulateNewNeighborhoodDropdown()
    {
      if (NewNeighborhoods == null)
      {
        NewNeighborhoods = new List<Neighborhood>();
        NeighborhoodDropdown.DisplayMember = "Name";
        NeighborhoodDropdown.ValueMember = "Directory";
        var packs = variant == Sims2Variant.Classic ? ClassicPacks : LegacyPacks;
        foreach (string pack in packs)
        {
          string neighborhoodTemplatesDirectory =
            variant == Sims2Variant.Classic
              ? Duplication.GetClassicNeighborhoodTemplatesDirectory(pack)
              : Duplication.GetLegacyNeighborhoodTemplatesDirectory(pack);
          if (neighborhoodTemplatesDirectory != null)
          {
            string[] dirs = Directory.GetDirectories(neighborhoodTemplatesDirectory);
            foreach (string dir in dirs)
            {
              string folderName = Path.GetFileName(dir);
              if (FolderNeighborhoodNameMappings.ContainsKey(folderName))
              {
                if (NeighborhoodStorytellingIDMappings.ContainsKey(folderName))
                {
                  List<string> screenshotFiles = new List<string>();
                  List<string> storytellingIDs = NeighborhoodStorytellingIDMappings[folderName];
                  string[] files = Directory.GetFiles(
                    variant == Sims2Variant.Classic
                      ? Duplication.GetClassicNeighborhoodTemplatesDirectory(pack, getStorytellingTemplates: true)
                      : Duplication.GetLegacyNeighborhoodTemplatesDirectory(pack, getStorytellingTemplates: true)
                  );
                  foreach (string file in files)
                  {
                    string fileName = new FileInfo(file).Name;
                    foreach (string id in storytellingIDs)
                    {
                      Regex regex = new Regex(@"^(snapshot_" + id + @"|thumbnail_" + id + "|webentry_" + id + ")");
                      if (regex.IsMatch(fileName))
                      {
                        screenshotFiles.Add(file);
                      }
                    }
                  }
                  NewNeighborhoods.Add(
                    new Neighborhood(FolderNeighborhoodNameMappings[folderName], dir, screenshotFiles)
                  );
                }
                else
                {
                  NewNeighborhoods.Add(new Neighborhood(FolderNeighborhoodNameMappings[folderName], dir));
                }
              }
            }
          }
        }
      }

      NeighborhoodDropdown.Items.Clear();
      if (NewNeighborhoods.Any())
      {
        NeighborhoodDropdown.Items.AddRange(NewNeighborhoods.ToArray());
        NeighborhoodDropdown.SelectedIndex = 0;
        Neighborhood selected = (Neighborhood)NeighborhoodDropdown.SelectedItem;
        DisplayNeighborhoodImage(selected.Directory);
      }
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

    private readonly string[] ClassicPacks = new string[]
    {
      "The Sims 2",
      "The Sims 2 Seasons",
      "The Sims 2 FreeTime",
      "The Sims 2 Apartment Life",
    };
    private readonly string[] LegacyPacks = new string[] { "Base", "EP5", "EP7", "EP8" };
    private readonly Dictionary<string, string> FolderNeighborhoodNameMappings = new Dictionary<string, string>
    {
      { "N001", Strings.Pleasantview },
      { "N002", Strings.Strangetown },
      { "N003", Strings.Veronaville },
      { "G001", Strings.Riverblossom_Hills },
      { "F001", Strings.Desiderata_Valley },
      { "E001", Strings.Belladonna_Cove },
    };
    private readonly Dictionary<string, List<string>> NeighborhoodStorytellingIDMappings = new Dictionary<
      string,
      List<string>
    >
    {
      {
        "N001",
        new List<string>() { "00000001", "2dae7a30", "2dae895f", "6dae6a73", "adae8020", "cdae71fd", "edae8d77" }
      },
      {
        "N002",
        new List<string>() { "00000002", "0d917b96", "2d7b3372", "6d7b3369", "cd338e66", "ed7b3373" }
      },
      {
        "N003",
        new List<string>() { "00000003", "2da007fb", "cda007ff", "eda007fa" }
      },
    };
    private List<Neighborhood> NewNeighborhoods,
      ExistingNeighborhoods;
    private Thread DuplicationThread;
    private CommonOpenFileDialog FolderBrowser;
    private readonly string UserNeighborhoodsDirectory;
    private string CurrentCreatedFolder;
    private readonly Mutex mutex;
    private readonly Sims2Variant variant;

    public class Neighborhood
    {
      private readonly string myName;
      private readonly string myDirectory;
      private readonly List<string> myScreenshotList;

      public Neighborhood(string strName, string strDirectory, List<string> screenshotList = null)
      {
        this.myName = strName;
        this.myDirectory = strDirectory;
        this.myScreenshotList = screenshotList;
      }

      public string Name
      {
        get { return myName; }
      }

      public string Directory
      {
        get { return myDirectory; }
      }

      public List<string> ScreenshotList
      {
        get { return myScreenshotList; }
      }
    }

    private void DuplicateButton_Click(object sender, EventArgs e)
    {
      Duplicate((Neighborhood)NeighborhoodDropdown.SelectedItem);
    }

    private void SelectFolderButton_Click(object sender, EventArgs e)
    {
      if (FolderBrowser.ShowDialog() == CommonFileDialogResult.Ok)
      {
        string duplicatedFolder = FolderBrowser.FileName;
        string[] files = Directory.GetFiles(duplicatedFolder);
        bool hasNoNeighborhoodFile = true;
        foreach (string file in files)
        {
          if (file.EndsWith("Neighborhood.package"))
          {
            hasNoNeighborhoodFile = false;
            break;
          }
        }

        DialogResult result = DialogResult.Yes;
        if (hasNoNeighborhoodFile)
        {
          result = MessageBox.Show(Strings.No_Neighborhood_Package, "", MessageBoxButtons.YesNo);
        }

        if (result == DialogResult.Yes)
        {
          Duplicate(new Neighborhood(Path.GetFileName(duplicatedFolder), duplicatedFolder));
        }
      }
    }

    private void Duplicate(Neighborhood neighborhood)
    {
      ToggleUIEnabled(false);
      if (DuplicationThread == null)
      {
        CurrentCreatedFolder = Duplication.GetNextUnusedNeighborhoodFolder(variant);
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
          string newNeighborhoodName = Duplication.DuplicateNeighborhoodTemplate(
            variant,
            neighborhood.Directory,
            CopyProgressBar,
            neighborhood.ScreenshotList
          );
          if (newNeighborhoodName == null)
          {
            MainPanel.Invoke(
              new Action(() =>
              {
                ToggleUIEnabled(false);
                MessageBox.Show(
                  string.Format(Strings.Error_Copying, neighborhood.Name),
                  Strings.Error,
                  MessageBoxButtons.OK,
                  MessageBoxIcon.Error
                );
              })
            );
          }
          else
          {
            MainPanel.Invoke(
              new Action(() =>
              {
                ToggleUIEnabled(false);
                Neighborhood newNeighborhood = new Neighborhood(
                  newNeighborhoodName,
                  Path.Combine(UserNeighborhoodsDirectory, newNeighborhoodName)
                );
                ExistingNeighborhoods.Add(newNeighborhood);
                if (ExistingRadioButton.Checked)
                {
                  NeighborhoodDropdown.Items.Add(newNeighborhood);
                }
                MessageBox.Show(string.Format(Strings.Success, newNeighborhoodName));
              })
            );
          }
        }
        catch (ThreadAbortException) { }
        catch (Exception)
        {
          MainPanel.Invoke(
            new Action(() =>
            {
              ToggleUIEnabled(false);
              MessageBox.Show(
                string.Format(Strings.Error_Copying, neighborhood.Name),
                Strings.Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
              );
            })
          );
        }
        finally
        {
          DuplicationThread = null;
          MainPanel.Invoke(
            new Action(() =>
            {
              CopyProgressBar.Value = 0;
              DuplicateButton.Text = Strings.Duplicate;
              ToggleUIEnabled(true);
            })
          );
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
        MainPanel.Invoke(
          new Action(() =>
          {
            CopyProgressBar.Value = 0;
            DuplicateButton.Text = Strings.Duplicate;
            ToggleUIEnabled(true);
          })
        );
      }).Start();
    }

    private void ToggleUIEnabled(bool enabled)
    {
      NeighborhoodDropdown.Enabled = enabled;
      DuplicateButton.Enabled = enabled;
      SelectFolderButton.Enabled = enabled;
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

    private void ExitImmediately(Mutex mutex)
    {
      Load += (s, e) =>
      {
        mutex.Close();
        Close();
      };
    }

    internal void OnExit(object sender, EventArgs e)
    {
      mutex.Close();
    }
  }
}
