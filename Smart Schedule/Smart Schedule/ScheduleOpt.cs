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
    public partial class ScheduleOpt : Form
    {
        CheckBox[] chkRooms;
        public List<String> names;
        public ScheduleOpt()
        {
            InitializeComponent();
            chkRooms = new CheckBox[] { checkBox1, checkBox2, checkBox3, checkBox4, checkBox5, checkBox6 };
            names = new List<string>();
            rdSltDef.Checked = true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            foreach (CheckBox cb in chkRooms)
            {
                if (cb.Checked == true)
                    names.Add(cb.Text);
            }
            this.Close();
        }

        public int getType()
        {
            if (rdSltDef.Checked == true)
                return 0;

            else
                return 1;
        }
    }
}
