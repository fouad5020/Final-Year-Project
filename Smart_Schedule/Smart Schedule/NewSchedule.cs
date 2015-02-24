using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Smart_Schedule
{
    public partial class NewSchedule : Form
    {
        #region
        /// <summary>
        /// declaring variables for storing project details
        /// </summary>
        bool cancelProj = false;            // boolean value for handling the form closing event
        public string projName;             // name of the Project given by the user
        public string dataFile;             // path of the Excel file containing all the data
        #endregion

        public NewSchedule()
        {
            InitializeComponent();
            if (!Directory.Exists(Application.StartupPath + "\\Schedules"))
            {//if the directory does not exist, it will be created
                Directory.CreateDirectory(Application.StartupPath + "\\Schedules");
            }
            string[] projects = Directory.GetDirectories(Application.StartupPath + "\\Schedules\\");      // all the existing project paths will be store to display to user
            foreach (string sfile in projects)
                cmbImpAls.Items.Add(sfile);         //names of projects will be added in the combo box for selection
        }

        /// <summary>
        /// event to handle File Dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFileDiag_Click(object sender, EventArgs e)
        {//to open the File Dialog
            fdOpen.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            fdOpen.ShowDialog();
            txtDataFile.Text = fdOpen.FileName;     // the path of the data file is saved and displayed
        }

        #region
        /// <summary>
        /// to handle user-side events for retreiving information entered about the project
        /// </summary>
        /// <returns></returns>
        public string getSchedName()
        { return projName; }
        public string getDataFile()
        { return dataFile; }
        #endregion

        #region
        /// <summary>
        /// events to handle buttons for closing form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            projName = txtSchName.Text;
            dataFile = txtDataFile.Text;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cancelProj = true;
        }

        #endregion

        private void NewSchedule_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cancelProj == true)
                return;
            if (Directory.Exists(Application.StartupPath + "\\Schedules\\" + txtSchName.Text))
            {
                DialogResult dR = MessageBox.Show("A Schedule with the name " + txtSchName.Text + " already exists. Overwrite?", "Schedule Exists", MessageBoxButtons.YesNo);
                if (dR == System.Windows.Forms.DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            e.Cancel = false;
            cancelProj = false;
        }
    }
}
