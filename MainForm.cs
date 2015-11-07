﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace NFL2K5Tool
{
    public partial class MainForm : Form
    {
        private GamesaveTool mTool = new GamesaveTool();

        public MainForm()
        {
            InitializeComponent();
            //Text = Directory.GetCurrentDirectory();
        }

        private void mLoadSaveButton_Click(object sender, EventArgs e)
        {
            LoadSaveFile();
        }

        private void LoadSaveFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.InitialDirectory = Directory.GetCurrentDirectory();
            dlg.RestoreDirectory = true;
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                mTool.LoadSaveFile(dlg.FileName);
                string shortName = dlg.FileName.Substring(dlg.FileName.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                Text = "NFL2K5Tool - " + shortName;
                statusBar1.Text = dlg.FileName + " loaded";
                mListPlayersButton2.Enabled = mListPlayersButton.Enabled = true;
            }
        }


        private void mListPlayersButton_Click(object sender, EventArgs e)
        {
            mTextBox.Clear();
            StringBuilder builder = new StringBuilder(5000);

            builder.Append(mTool.GetKey(listAttributesToolStripMenuItem.Checked, listApperanceToolStripMenuItem.Checked));
            builder.Append("\n");
            
            if( listTeamsToolStripMenuItem.Checked)
                builder.Append(mTool.GetLeaguePlayers(listAttributesToolStripMenuItem.Checked, listApperanceToolStripMenuItem.Checked, true, true));
            
            if (listFreeAgentsToolStripMenuItem.Checked)
                builder.Append(mTool.GetTeamPlayers("FreeAgents", listAttributesToolStripMenuItem.Checked, listApperanceToolStripMenuItem.Checked));
            
            if( listDraftClassToolStripMenuItem.Checked)
                builder.Append(mTool.GetTeamPlayers("DraftClass", listAttributesToolStripMenuItem.Checked, listApperanceToolStripMenuItem.Checked));
            
            mTextBox.AppendText(builder.ToString());
        }

        private void mClearButton_Click(object sender, EventArgs e)
        {
            mTextBox.Clear();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mTextBox.SetSearchString();
        }

        private void stringToHexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DebugDialog form = new DebugDialog();
            form.Tool = mTool;
            form.Show();
        }

        private void loadSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadSaveFile();
        }

        private void listApperanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listApperanceToolStripMenuItem.Checked = !listApperanceToolStripMenuItem.Checked;
        }

        private void listAttributesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listAttributesToolStripMenuItem.Checked = !listAttributesToolStripMenuItem.Checked;
        }

        private void listFreeAgentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listFreeAgentsToolStripMenuItem.Checked = !listFreeAgentsToolStripMenuItem.Checked;
        }

        private void listDraftClassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listDraftClassToolStripMenuItem.Checked = !listDraftClassToolStripMenuItem.Checked;
        }

        private void listTeamsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listTeamsToolStripMenuItem.Checked = !listTeamsToolStripMenuItem.Checked;
        }

        private void mListPlayersButton2_Click(object sender, EventArgs e)
        {
            mTextBox.Clear();
            StringBuilder builder = new StringBuilder(5000);
            builder.Append(mTool.GetKey(listAttributesToolStripMenuItem.Checked, listApperanceToolStripMenuItem.Checked));
            builder.Append("\n");
            int max = (int)numericUpDown1.Value;
            for (int i = 0; i < max; i++)
            {
                builder.Append(mTool.GetPlayerData(i, listAttributesToolStripMenuItem.Checked, listApperanceToolStripMenuItem.Checked));
                builder.Append("\n");
            }
            //builder.Append(mTool.GetLeaguePlayers(listAttributesToolStripMenuItem.Checked, listApperanceToolStripMenuItem.Checked, true, true));
            mTextBox.AppendText(builder.ToString());
        }

        private void mApplyButton_Click(object sender, EventArgs e)
        {
            InputParser parser = new InputParser();
            parser.Tool = this.mTool;
            parser.ProcessText(mTextBox.Text);
        }

        private void mSaveButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.RestoreDirectory = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(dlg.FileName))
                    File.Delete(dlg.FileName);
                File.WriteAllBytes(dlg.FileName, mTool.GameSaveData);
            }
        }


    }
}
