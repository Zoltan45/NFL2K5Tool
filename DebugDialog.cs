﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NFL2K5Tool
{
    public partial class DebugDialog : Form
    {
        public DebugDialog()
        {
            InitializeComponent();
            mResultsTextBox.StatusControl = mStatusLabel;
        }

        public GamesaveTool Tool { get; set; }


        private byte[] SaveFile { get { return Tool.GameSaveData; } }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            int ch;
            StringBuilder builder = new StringBuilder(20);

            builder.Append("0x");
            foreach (char c in textBox1.Text)
            {
                ch = (int)c;
                builder.Append(ch.ToString("X2"));
                if (checkBox1.Checked)
                {
                    builder.Append("00");
                }
            }
            textBox2.Text = builder.ToString();
        }

        private void mFindButton_Click(object sender, EventArgs e)
        {
            FindLocations();
        }

        private void FindLocations()
        {
            mResultsTextBox.Clear();
            List<long> locs = StaticUtils.FindStringInFile(textBox1.Text, SaveFile, 0, SaveFile.Length);
            foreach (int loc in locs)
            {
                mResultsTextBox.AppendText(String.Format("{0:x}\n", loc));
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                FindLocations();
                e.Handled = true;
            }
        }

        // need to find pointers for non-players
        private void mFindPointers_Click(object sender, EventArgs e)
        {
            mResultsTextBox.Clear();
            List<long> locs = StaticUtils.FindStringInFile(textBox1.Text, SaveFile, 0, SaveFile.Length);
            List<int> pointers;

            for (int i = 0; i < locs.Count; i++)
            {
                pointers = FindPointersForLocation(locs[i]);
                foreach (int dude in pointers)
                {
                    mResultsTextBox.AppendText(dude.ToString("X"));
                    mResultsTextBox.AppendText("\r\n");
                }
            }
        }

        private List<int> FindPointersForLocation(long location)
        {
            List<int> pointerLocations = new List<int>();
            int pointer = 0;
            long dataLocation = 0;
            int limit = this.SaveFile.Length - 4;
            for (long i = 0; i < limit; i++)
            {
                pointer =  SaveFile[i + 3] << 24;
                pointer += SaveFile[i + 2] << 16;
                pointer += SaveFile[i + 1] << 8;
                pointer += SaveFile[i];
                dataLocation = i + pointer - 1;

                if (dataLocation == location)
                {
                    pointerLocations.Add((int)i);
                }
            }
            return pointerLocations;
        }

        private void mPointsToLocButton_Click(object sender, EventArgs e)
        {
            mResultsTextBox.Clear();
            string val = textBox1.Text;
            ShowPointersForLoc(val);
        }

        private void ShowPointersForLoc(string val)
        {
            if (val.StartsWith("0x"))
                val = val.Substring(2);
            try
            {
                mResultsTextBox.Clear();
                int loc = Int32.Parse(val, System.Globalization.NumberStyles.AllowHexSpecifier);
                List<int> pointers = FindPointersForLocation(loc);
                foreach (int dude in pointers)
                {
                    mResultsTextBox.AppendText(dude.ToString("X"));
                    mResultsTextBox.AppendText("\r\n");
                }
                if (pointers.Count == 0)
                {
                    mResultsTextBox.Text = "Pointr to: "+  val + " Not found";
                }
            }
            catch (Exception e)
            {
                mResultsTextBox.Text = e.Message; 
            }
        }

        private void UpdatePlayerNameTextBox()
        {
            int player = (int) mPlayerUpDown.Value;
            mPlayerNameTextBox.Text = Tool.GetPlayerName(player, ' ');
            mLocationLabel.Text = "0x" + Tool.GetPlayerDataStart(player).ToString("X");
        }

        private void mPlayerUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdatePlayerNameTextBox();
        }

        private void mSetFirstNameButton_Click(object sender, EventArgs e)
        {
            if( mNameTextBox.Text.Length > 0)
                Tool.SetPlayerFirstName((int)mPlayerUpDown.Value, mNameTextBox.Text, mUsePointerButton.Checked);
        }

        private void mSetLastNameButton_Click(object sender, EventArgs e)
        {
            if (mNameTextBox.Text.Length > 0)
                Tool.SetPlayerLastName((int)mPlayerUpDown.Value, mNameTextBox.Text, mUsePointerButton.Checked);
        }

        private void listNumberOfPlayersButton_Click(object sender, EventArgs e)
        {
           mResultsTextBox.Text = Tool.GetNumberOfPlayersOnAllTeams();
        }

        private void mGetTeamButton_Click(object sender, EventArgs e)
        {
            mResultsTextBox.Text = Tool.GetTeamPlayers(textBox1.Text, false, false);
        }

        private void mLocationLabel_Click(object sender, EventArgs e)
        {
            ShowPointersForLoc(mLocationLabel.Text);
        }

        private void mTeamButton_Click(object sender, EventArgs e)
        {
            mResultsTextBox.Text = Tool.GetPlayerTeam((int)mPlayerUpDown.Value);
        }

        private void mListPlayersButton2_Click(object sender, EventArgs e)
        {
            mResultsTextBox.Clear();
            StringBuilder builder = new StringBuilder(5000);
            builder.Append(Tool.GetKey(false, false));
            builder.Append("\n");
            string photo, pbp;
            int max = (int)numericUpDown1.Value;
            for (int i = 0; i < max; i++)
            {
                builder.Append(Tool.GetPlayerData(i, false, false));
                if (includeDepthToolStripMenuItem.Checked)
                {
                    builder.Append(" Depth:");
                    builder.Append(Tool.GetPlayerPositionDepth(i).ToString("X2"));
                }
                if (includePhotePBPToolStripMenuItem.Checked)
                {
                    photo = Tool.GetAttribute(i, PlayerOffsets.Photo);
                    pbp = Tool.GetAttribute(i, PlayerOffsets.PBP);
                    builder.Append(photo);
                    builder.Append(",");
                    builder.Append(pbp);
                    builder.Append(",");
                    builder.Append(DataMap.GetPlayerNameForPhoto(photo));
                    builder.Append(",");
                    builder.Append(DataMap.GetPlayerNameForPBP(photo));
                    builder.Append(",");
                    
                    if (Tool.GetAttribute(i, PlayerOffsets.Photo) != Tool.GetAttribute(i, PlayerOffsets.PBP))
                    {
                        builder.Append("****,");
                    }
                }
                if (mNumBytes.Value > 0)
                {
                    int dataStart = Tool.GetPlayerDataStart(i) + (int)mOffsetUpDown.Value;
                    builder.Append(" ");
                    for (int j = 0; j < mNumBytes.Value; j++)
                    {
                        builder.Append(Tool.GameSaveData[dataStart + j].ToString("X2"));
                    }
                }
                builder.Append("\n");
            }
            mResultsTextBox.AppendText(builder.ToString());
        }

        private void numericUpDown1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                mListPlayersButton2_Click(sender, e);
        }

        private void includeDepthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            includeDepthToolStripMenuItem.Checked = !includeDepthToolStripMenuItem.Checked;
        }

        private void autoUpdateDepthChartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tool.AutoUpdateDepthChartForTeam("Packers");
        }

        private void includePhotePBPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            includePhotePBPToolStripMenuItem.Checked = !includePhotePBPToolStripMenuItem.Checked;
        }

        int FirstPlayerFNamePtr = 0xe290;
        int photoDistance = 0x32;
        int playerSize = 0xD0;

        // getting data from NFL2k2 gamesave
        private string GetNFL2K2PhotoData()
        {
            FirstPlayerFNamePtr = 0xe290;
            photoDistance = 0x32;
            playerSize = 0xD0;

            return GetNameData();
        }

        // getting data from NFL2k4 gamesave
        private string GetNFL2K4PhotoData()
        {
            FirstPlayerFNamePtr = 0x1132c;
            photoDistance = -6;
            playerSize = 0x50;

            return GetNameData();
        }

        private string GetNameData()
        {
            StringBuilder builder = new StringBuilder(5000);
            int ptrLoc = 0;
            int photoLoc = 0;
            try
            {
                for (int i = 0; i < numericUpDown1.Value; i++)
                {
                    ptrLoc = FirstPlayerFNamePtr + (i * playerSize);
                    photoLoc = ptrLoc + photoDistance;
                    builder.Append(Tool.GetString(Tool.GetPointerDestination(ptrLoc + 4))); // lname
                    builder.Append(", ");
                    builder.Append(Tool.GetString(Tool.GetPointerDestination(ptrLoc))); // fname
                    builder.Append("=");
                    builder.Append(Get2BytePointer(photoLoc));
                    //if (Get2BytePointer(photoLoc) != Get2BytePointer(photoLoc + 2))
                    //    builder.Append("*** " + Get2BytePointer(photoLoc+2));
                    //builder.Append("," + ptrLoc.ToString("X"));
                    builder.Append("\n");
                }
            }
            catch { }
            return builder.ToString();
        }

        private string Get2BytePointer(int photoLoc)
        {
            string retVal = "";
            int val = Tool.GameSaveData[photoLoc + 1] << 8;
            val += Tool.GameSaveData[photoLoc];
            if (val < 10)
                retVal = "000" + val;
            else if (val < 100)
                retVal = "00" + val;
            else if (val < 1000)
                retVal = "0" + val;
            else
                retVal = val.ToString();
            return retVal;
        }

        private void extractPHOHO2K2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mResultsTextBox.Text = GetNFL2K2PhotoData();
        }

        private void extractPhoto2K4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mResultsTextBox.Text = GetNFL2K4PhotoData();
        }

        private void mSetByteLocUpDown_ValueChanged(object sender, EventArgs e)
        {
            mSetByteValTextBox.Text = Tool.GameSaveData[(int)mSetByteLocUpDown.Value].ToString("X2");
        }

        private void SetBytes()
        {
            byte b1 = 0;

            try
            {
                for (int i = 0; i < mSetByteValTextBox.Text.Length; i += 2)
                {
                    b1 = (byte)UInt16.Parse(mSetByteValTextBox.Text.Substring(i, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                    Tool.GameSaveData[(int)mSetByteLocUpDown.Value + i] = b1;
                }
            }
            catch
            {
                mStatusLabel.Text = "Set Byte error.";
            }

        }

        private void mSetByteButton_Click(object sender, EventArgs e)
        {
            SetBytes();
        }

        private void mSetByteValTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SetBytes();
            }
        }

    }
}
