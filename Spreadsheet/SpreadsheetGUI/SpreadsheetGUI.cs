﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSGui;
using SpreadsheetGUI;
using System.Windows.Forms;

namespace SpreadsheetGUI
{
    public partial class SpreadsheetGUI : Form , ISSInterface
    {
        public string Message
        {
            set
            {
                MessageBox.Show(value);
            }
        }

        public SpreadsheetGUI()
        {
            InitializeComponent();
        }

        public event Action NewWindowEvent;
        public event Action CloseWindowEvent;
        public event Action<string> FileChosenEvent;
        public event Action<string> SaveFileEvent;

        private void newToolStripMenuItem_Click(object sender, EventArgs e)//FILE>NEW
        {
            if (NewWindowEvent != null)
            {
                NewWindowEvent();
            }
        }
        public void CreateNewWindowHandler()
        {
            SpreadsheetApplicationContext.GetContext().RunNew();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)//FILE>CLOSE
        {
            if(CloseWindowEvent != null)
            {
                CloseWindowEvent();
            }
        }
        public void CloseCurrentWindowHandler()
        {
            //Added in a check to make sure user has saved
            Close();
        }
        private void FileDialogueBox_FileOk(object sender, EventArgs e)
        {

        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)//File > Open
        {
            DialogResult result = FileDialogueBox.ShowDialog();
            if (result == DialogResult.Yes || result == DialogResult.OK)
            {
                if (FileChosenEvent != null)
                {
                    FileChosenEvent(FileDialogueBox.FileName);
                }
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            DialogResult result = SaveDialogueBox.ShowDialog();
            if (result == DialogResult.Yes || result == DialogResult.OK)
            {
                if (SaveFileEvent != null)
                {
                    SaveFileEvent(SaveDialogueBox.FileName);
                }
            }
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void SaveDialogueBox_FileOk(object sender, CancelEventArgs e)
        {

        }
    }
}
