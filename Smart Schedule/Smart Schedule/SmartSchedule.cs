using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Smart_Schedule
{
    public partial class SmartSchedule : Form
    {
        public SmartSchedule()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            Main_Menu obj = new Main_Menu();
            this.Hide();
            //while (close == false)
            {
                obj.ShowDialog();
                //close = obj.close;
                //continue;
            }
            this.Show();
            this.txtPassword.Text = "";
            this.txtUsername.Text = "";
        }
    }
}
