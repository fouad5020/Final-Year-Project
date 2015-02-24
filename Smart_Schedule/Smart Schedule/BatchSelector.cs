using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace Smart_Schedule
{
    public partial class BatchSelector : Form
    {
        private List<Batch> bList;
        public BatchSelector()
        {
            InitializeComponent();
            bList = new List<Batch>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                int bt;
                Int32.TryParse(txtBatch.Text, out bt);
                Color c = Color.FromName(comboBox1.SelectedItem.ToString());
                Batch b = new Batch(bt,c);
                b.setRepeat(chkRepeat.Checked);
                bList.Add(b);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            txtBatch.Text = "";
            comboBox1.SelectedIndex = -1;
            updateTable();
                
        }

        private void cmbColPik_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = e.Bounds;
            if (e.Index >= 0)
            {
                string n = ((ComboBox)sender).Items[e.Index].ToString();
                Font f = comboBox1.Font;
                Color c = Color.FromName(n);
                Brush b = new SolidBrush(c);
                g.DrawString(n, f, Brushes.Black, rect.X + 35, rect.Top);
                g.FillRectangle(b, rect.X + 2, rect.Top + 2, 30, rect.Height - 4);
                Pen p = new Pen(Color.Black);
                g.DrawRectangle(p, rect.X + 1, rect.Top + 1, 30, rect.Height - 3);
            }
            /*
            if (e.Index != -1)
            {
                e.DrawBackground();
                e.Graphics.FillRectangle(GetCurrentBrush(comboBox1.Items[e.Index].ToString()), e.Bounds);
                Font f = comboBox1.Font;
                e.Graphics.DrawString(comboBox1.Items[e.Index].ToString(), f, Brushes.Black, e.Bounds, StringFormat.GenericDefault);
                e.DrawFocusRectangle();
            }*/
        }

        private Brush GetCurrentBrush(string colorName)
        {
            return new SolidBrush(Color.FromName(colorName));
        }

        private void Batch_Load(object sender, EventArgs e)
        {
            Type colorType = typeof(System.Drawing.Color);
            PropertyInfo[] propInfoList = colorType.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);
            foreach (PropertyInfo c in propInfoList)
            {
                this.comboBox1.Items.Add(c.Name);
            }
        }

        public void showForEdit(ref List<Batch> batchList)
        {
            bList = batchList;
            updateTable();
            this.ShowDialog();
        }

        private void updateTable()
        {
            int i = 0;
            dgvBatch.Rows.Clear();
            foreach (Batch bt in bList)
            {
                dgvBatch.Rows.Add();
                dgvBatch.Rows[i].Cells[0].Value = bt.getBatch().ToString();
                dgvBatch.Rows[i].Cells[1].Style.BackColor = bt.getColor();
                dgvBatch.Refresh();
                i++;
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            txtBatch.Text = "";
            comboBox1.SelectedIndex = -1;
            chkRepeat.Checked = false;
        }
    }
}
