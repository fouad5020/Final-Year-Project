using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Smart_Schedule
{
    public partial class Main_Menu : Form
    {
        //declaring globals for use throughout the program
        #region
        private List<String> roomArr = new List<string>();
        private List<String> labArr = new List<string>();
        private CheckBox[] chkRooms;
        private CheckBox[] chkLabs;
        private String fileName;
        private CourseHash crsHash;
        private List<Course> crsList;
        public bool close = false;
        Microsoft.Office.Interop.Excel.Application ExcelObj = new Microsoft.Office.Interop.Excel.Application();
        private int chk = 0;
        private List<Batch> batchList;
        private HashTable<trackNode> track = new HashTable<trackNode>();
        private Timetable timeTable;
        //HashNode<Course> crsH = new HashNode<Course>();
        private Domain MasterDom = new Domain();
        #endregion

        public Main_Menu()
        {
            InitializeComponent();
            //initialization of user controls
            #region
            //to treat the checkboxes as an array, we declare an array of checkboxes and refer them to the checkboxes on the form
            chkRooms = new CheckBox[]{checkBox1,checkBox2,checkBox3,checkBox4,checkBox5,checkBox6,checkBox7,checkBox8,checkBox9,
                checkBox10,checkBox11,checkBox12,checkBox13,checkBox14,checkBox15,checkBox16,checkBox17,checkBox18,checkBox19,checkBox20,checkBox21,
                checkBox22};
            //to access the checkboxes of labs
            chkLabs = new CheckBox[] { checkBox23, checkBox24, checkBox25, checkBox26 };
            foreach (CheckBox c in chkRooms)
                c.Checked = true;           //initially check all rooms
            bgClash.WorkerReportsProgress = true;
            dtpSlotSTime.Format = dtpSlotETime.Format = DateTimePickerFormat.Time;
            dtpSlotSTime.ShowUpDown = dtpSlotETime.ShowUpDown = true;
            dtpSlotSTime.Value = dtpSlotETime.Value = DateTime.Now;
            dtpSlotETime.Value = dtpSlotSTime.Value.AddHours(1);
            cmbBatch.Items.Add("Add new batch..");
            batchList = new List<Batch>();
            slotInsertion(false);
            fillMenu();
            initGridViewTrack();
            #endregion
        }

        //all the menu strip events are handled here
        #region

        // events for menu items under File
        #region
        /// <summary>
        /// event behind the New button of File strip.. when a user wants to create a new Schedule
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnFileNew_Click(object sender, EventArgs e)
        {
            NewSchedule obj = new NewSchedule();                                // a new instance of NewSchedule will be created for project
            DialogResult dR = obj.ShowDialog();                                 // to store result obtained by closing Project window
            if (dR == System.Windows.Forms.DialogResult.OK)
            {//if the project details have been inserted and user wants to make a new project
                string path = Application.StartupPath + "\\Schedules\\";
                path = path + obj.getSchedName();                               // name of project will be added to path to create a new directory
                if (Directory.Exists(path))                                     // if a project exists, all the data will be reset and overwritten
                    Directory.Delete(path, true);
                Directory.CreateDirectory(path);
                File.Copy(obj.getDataFile(), path + "\\" + Path.GetFileName(obj.getDataFile()), true);      // the data file will be copied to the created directory

                crsList = new List<Course>();                                   // course list will be re-initialized
                crsHash = new CourseHash();                                     // course hash will be re-initialized
                dgvCrs.Rows.Clear();
                fileName = path + "\\" + Path.GetFileName(obj.getDataFile());   // filename of excel file
                showData(fileName);
                dgvCrs.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvCrs.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgvCrs.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
        }

        /// <summary>
        /// to fill the Menu strip's Open button.. in case user wants to open an already created Schedule
        /// </summary>
        private void fillMenu()
        {
            string[] projs = Directory.GetDirectories(Application.StartupPath+"\\Schedules");
            foreach (string pN in projs)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(Path.GetFileName(pN));       //to extract the Schedule name from path = 
                item.Click += new EventHandler(this.menuItem1_Click);                       // add event handler to the created item
                mnFileOpen.DropDownItems.Add(item);                                         // add item to the drop-down list associated with Open
            }
        }

        /// <summary>
        /// event called when user clicks on the name of the Schedule from the dropdown list of saved Schedules
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItem1_Click(object sender, System.EventArgs e)
        {
            string p = Application.StartupPath + "\\Schedules\\" + sender.ToString();

            string[] files = Directory.GetFiles(p);             //all files in the Schedule Project will be saved
            foreach (string fN in files)
                if ((Path.GetExtension(fN) == ".xls") || (Path.GetExtension(fN) == ".xlsx"))
                    fileName = fN;                              //path of data file will be saved for further use

            crsList = new List<Course>();
            crsHash = new CourseHash();
            batchList = new List<Batch>();
            dgvCrs.Rows.Clear();
            showData(fileName);
            string chk = Path.GetDirectoryName(fileName);
            dgvCrs.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvCrs.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvCrs.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }
        #endregion

        // events for menu items under Batch
        #region
        /// <summary>
        /// event to open the Batch Manager for adding/editting batches
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnBatchMgr_Click(object sender, EventArgs e)
        {
            BatchSelector b = new BatchSelector();
            b.showForEdit(ref batchList);
            cmbBatch.Items.Clear();
            cmbBatch.Items.Add("Add new batch..");
            foreach (Batch bt in batchList)
            {
                string val = bt.getBatch().ToString();
                if (bt.getRepeat() == true)
                    val += " R";
                cmbBatch.Items.Add(val);
            }
        }
        #endregion

        #endregion

        // private events to be used by the class
        #region
        /// <summary>
        /// to store and show the data captured from Excel Sheet in Course Datagrid view
        /// </summary>
        /// <param name="fN"></param>
        /// 
        private void showData(String fN)
        {
            Microsoft.Office.Interop.Excel.Workbook theWorkbook = ExcelObj.Workbooks.Open(Path.GetFullPath(fN), 0, true, 5, "", "", true,
                    Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true);

            Microsoft.Office.Interop.Excel.Sheets sheets = theWorkbook.Worksheets;
            Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)sheets.get_Item(1);

            progInsert.Visible = true;
            progInsert.Minimum = 0;
            progInsert.Maximum = worksheet.UsedRange.Rows.Count;

            for (int i = 0; i < worksheet.UsedRange.Rows.Count; i++)
            {
                Microsoft.Office.Interop.Excel.Range oRng = worksheet.get_Range("A" + (i + 1).ToString(), "H" + (i + 1).ToString());
                System.Array row = (System.Array)oRng.Cells.Value;

                progInsert.Value = i;
                if ((row.GetValue(1, 4).ToString() == "CS492") || (row.GetValue(1, 4).ToString() == "CS491"))
                    continue;

                if (crsHash.Insert(crsHash.getHashVal(row.GetValue(1, 4).ToString()), row))
                {
                    crsList.Add(new Course(row.GetValue(1, 4).ToString(), row.GetValue(1, 5).ToString()));
                    dgvCrs.Rows.Add(row.GetValue(1, 4).ToString(), row.GetValue(1, 5).ToString(), crsHash.getCrsCredits(row.GetValue(1, 4).ToString()));
                }
            }
            dgvCrs.Refresh();
            progInsert.Value = 0;
            bool isBusy = true;
            bgClash.WorkerReportsProgress = true;
            bgClash.RunWorkerCompleted += delegate
            {
                isBusy = false;
                progInsert.Visible = false;
            };
            bgSaved.RunWorkerCompleted += delegate
            {
                isBusy = false;
                updateCourseGrid();
            };
            if (bgClash.IsBusy != true)
            {
                bgClash.RunWorkerAsync();
            }

            if (bgSaved.IsBusy != true)
            {
                bgSaved.RunWorkerAsync();
            }

        }

        private void updateCourseGrid()
        {
            for (int i = 0; i < dgvCrs.Rows.Count; i++)
            {
                if (dgvCrs.Rows[i].Cells[0].Value == null)
                    continue;
                dgvCrs.Rows[i].Cells[3].Value = crsHash.getAls(dgvCrs.Rows[i].Cells[0].Value.ToString());
            }
        }

        private void updateMDom()
        {
            MasterDom = new Domain();
            foreach (Course c in crsList)
            {
                List<Section> sec = new List<Section>();
                sec = crsHash.getSectionList(c.getID());
                foreach (Section s in sec)
                {
                    //crsHash.clashSet.Initialize(c.getID(), c.getName(), s.getID());
                    MasterDom.insertEntry(c.getID(), s.getID(), crsHash.getSecAls(c.getID(), s.getID()));
                    //mDom.Add(new DomainNode(c.getID(), s.getID(), c.getAlias()));
                    //cmbSlot.Items.Add(c.getID() + " " + s.getID());
                    //gvClsCourse.Rows.Add(c.getID(), c.getName(), s.getID());
                }
            }
        }

        private Batch decryptBatch(string val)
        {
            //string val = cmbBatch.SelectedItem.ToString();
            string[] div = val.Split(' ');
            int ind = div.Length;
            Batch b = new Batch();
            b.setBatch(Int32.Parse(div[0]));
            if (ind > 1)
                b.setRepeat(true);
            foreach (Batch bt in batchList)
            {
                if ((bt.getBatch() == b.getBatch()) && (bt.getRepeat() == b.getRepeat()))
                    b.setColor(bt.getColor());
            }
            return b;
        }

        private void writeToFile(string Path, string[] List)
        {/*
            if (!File.Exists(aliasPath))
                File.Create(aliasPath);*/

            TextWriter tw = new StreamWriter(Path);
            for (int i = 0; i < List.Length; i++)
                tw.WriteLine(List[i]);

            tw.Close();
        }

        /// <summary>
        /// to enable the controls for adding slot
        /// </summary>
        /// <param name="val">visible or hidden</param>
        private void slotInsertion(bool val)
        {
            lblStime.Visible = lblEtime.Visible = val;
            dtpSlotSTime.Visible = dtpSlotETime.Visible = val;
            btnAddSlot.Visible = btnSlotCancel.Visible = val;
        }

        /// <summary>
        /// to update the timetable gridview after any change
        /// </summary>
        /// <param name="d">name of day</param>
        private void updateTimeTableGrid(string d)
        {
            Slot temp = timeTable.getDayHead(d);
            //temp.setDom(mDom);
            dgvTimeTable.Rows.Clear();
            dgvTimeTable.Columns.Clear();
            if (temp == null)
                return;
            while (temp != null)
            {
                var dgCol = new DataGridViewTextBoxColumn();
                if (temp.breakTime == false)
                    dgCol.HeaderText = temp.getHeader();
                //dgCol.CellType = DataGridViewTextBoxCell
                dgCol.SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvTimeTable.Columns.Add(dgCol);
                temp = temp.next;
            }
            dgvTimeTable.Refresh();
            dgvTimeTable.Update();
            dgvTimeTable.Rows.Add(roomArr.Count + labArr.Count);
            for (int k = 0; k < roomArr.Count; k++)
            {
                dgvTimeTable.Rows[k].HeaderCell.Value = String.Format("{0}", roomArr[k].ToString());
                dgvTimeTable.Rows[k].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                //dgvTimeTable.Rows[k].HeaderCell.
            }
            int prev = roomArr.Count;
            for (int j = roomArr.Count; j < prev + labArr.Count; j++)
            {
                dgvTimeTable.Rows[j].HeaderCell.Value = String.Format("{0}", labArr[j - prev].ToString());
                dgvTimeTable.Rows[j].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }
            temp = timeTable.getDayHead(d);
            int l = 0;
            while (temp != null)
            {
                if (temp.rooms != null)
                {
                    for (int i = 0; i < temp.rooms.Length; i++)
                    {
                        if (temp.rooms[i] != null)
                        {
                            dgvTimeTable.Rows[i].Cells[l].Value = temp.rooms[i].getAls();
                            Batch b = crsHash.getBatch(temp.rooms[i].getCrs(), temp.rooms[i].getSec());
                            foreach (Batch bt in batchList)
                            {
                                if (bt.Compare(b))
                                    dgvTimeTable.Rows[i].Cells[l].Style.BackColor = bt.getColor();
                            }
                        }
                    }
                }

                if (temp.labs != null)
                {
                    for (int a = roomArr.Count; a < prev + temp.labs.Length; a++)
                    {
                        if (temp.labs[a - prev] != null)
                        {
                            dgvTimeTable.Rows[a].Cells[l].Value = temp.labs[a - prev].getAls();
                            Batch b = crsHash.getBatch(temp.labs[a - prev].getCrs(), temp.labs[a - prev].getSec());
                            foreach (Batch bt in batchList)
                            {
                                if (bt.Compare(b))
                                    dgvTimeTable.Rows[a].Cells[l].Style.BackColor = bt.getColor();
                            }
                        }
                    }
                }
                temp = temp.next;
                l++;
            }
            //dgvTimeTable.
        }

        private void updateSelDays()
        {
            if (timeTable != null)
            {
                cmbSelDay.Items.Clear();
                List<String> names = timeTable.getDayNames();
                foreach (String s in names)
                {
                    cmbSelDay.Items.Add(s);
                }
                cmbSelDay.SelectedIndex = 0;
            }
        }

        private void initDays()
        {
            ScheduleOpt dayObj = new ScheduleOpt();
            dayObj.ShowDialog();
            List<String> names = dayObj.names;

            if (names.Count > 0)
            {
                if (dayObj.getType() == 0)
                {
                    roomArr.Clear();
                    for (int i = 0; i < chkRooms.Length; i++)
                    {
                        if (!chkRooms[i].Checked)
                            continue;

                        roomArr.Add(chkRooms[i].Text);
                    }

                    labArr.Clear();
                    for (int i = 0; i < chkLabs.Length; i++)
                    {
                        if (!chkLabs[i].Checked)
                            continue;

                        labArr.Add(chkLabs[i].Text);
                    }

                    timeTable = new Timetable(names, track, roomArr.Count, labArr.Count);
                    updateMDom();

                    Slot teaBreak = new Slot(0, 0, DateTime.Parse("11:30"), DateTime.Parse("11:50"), new List<DomainNode>(), new List<Batch>());
                    teaBreak.breakTime = true;
                    Slot lunchBreak = new Slot(0, labArr.Count, DateTime.Parse("13:50"), DateTime.Parse("14:30"), MasterDom.getDomList(), new List<Batch>());
                    lunchBreak.breakTime = true;
                    timeTable.addDaySlotAll(new Slot(roomArr.Count, labArr.Count, DateTime.Parse("08:30"), DateTime.Parse("09:30"), MasterDom.getDomList(), batchList));
                    timeTable.addDaySlotAll(new Slot(roomArr.Count, labArr.Count, DateTime.Parse("09:30"), DateTime.Parse("10:30"), MasterDom.getDomList(), batchList));
                    timeTable.addDaySlotAll(new Slot(roomArr.Count, labArr.Count, DateTime.Parse("10:30"), DateTime.Parse("11:30"), MasterDom.getDomList(), batchList));
                    timeTable.addDaySlotAll(teaBreak);
                    timeTable.addDaySlotAll(new Slot(roomArr.Count, labArr.Count, DateTime.Parse("11:50"), DateTime.Parse("12:50"), MasterDom.getDomList(), batchList));
                    timeTable.addDaySlotAll(new Slot(roomArr.Count, labArr.Count, DateTime.Parse("12:50"), DateTime.Parse("13:50"), MasterDom.getDomList(), batchList));
                    timeTable.addDaySlotAll(lunchBreak);
                    timeTable.addDaySlotAll(new Slot(roomArr.Count, labArr.Count, DateTime.Parse("14:30"), DateTime.Parse("15:30"), timeTable.MaxDom, batchList));
                    timeTable.addDaySlotAll(new Slot(roomArr.Count, labArr.Count, DateTime.Parse("15:30"), DateTime.Parse("16:30"), timeTable.MaxDom, batchList));
                    timeTable.addDaySlotAll(new Slot(roomArr.Count, labArr.Count, DateTime.Parse("16:30"), DateTime.Parse("17:30"), timeTable.MaxDom, batchList));

                    updateSelDays();
                    updateTimeTableGrid(cmbSelDay.SelectedItem.ToString());
                    slotInsertion(false);
                }

                else
                {
                    timeTable = new Timetable(names, track, roomArr.Count, labArr.Count);
                }
                progInsert.Visible = true;
                bgSlotPool.WorkerReportsProgress = true;
                progInsert.Minimum = 0;
                progInsert.Maximum = crsHash.getClashHeads().Count;
                GC.Collect();
                //timeTable.getCombinations(MasterDom.getDomList(), roomArr.Count, labArr.Count);

                DateTime startThread = DateTime.Now;
                //timeTable.generateSlotPool(MasterDom.getDomList(), roomArr.Count, labArr.Count, ref progInsert);
                //updateSlotPoolGrid();

                bgSlotPool.RunWorkerCompleted += delegate
                {
                    DateTime endThread = DateTime.Now;
                    TimeSpan duration = endThread - startThread;
                    MessageBox.Show(String.Format("Slots created in {0}mins {1}seconds", duration.Minutes, duration.Seconds));
                    progInsert.Visible = false;
                    updateSlotPoolGrid();

                };

                //bgSlotPool.RunWorkerAsync();
            }
        }

        private void updateSlotPoolGrid()
        {
            dgvSchedCrs.Rows.Clear();
            dgvSchedCrs.Columns.Clear();
            for (int i = 0; i <= timeTable.pool.Count(); i++)
            {
                var dgCol = new DataGridViewTextBoxColumn();
                dgCol.HeaderText = String.Format("Slot {0}:", i + 1);
                //dgCol.CellType = DataGridViewTextBoxCell
                dgCol.SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvSchedCrs.Columns.Add(dgCol);
                //temp = temp.next;
            }
            dgvSchedCrs.Refresh();
            dgvSchedCrs.Update();
            dgvSchedCrs.Rows.Add(roomArr.Count + labArr.Count);

            for (int k = 0; k < roomArr.Count; k++)
            {
                dgvSchedCrs.Rows[k].HeaderCell.Value = String.Format("{0}", roomArr[k].ToString());
                dgvSchedCrs.Rows[k].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                //dgvTimeTable.Rows[k].HeaderCell.
            }
            int prev = roomArr.Count;
            for (int j = roomArr.Count; j < prev + labArr.Count; j++)
            {
                dgvSchedCrs.Rows[j].HeaderCell.Value = String.Format("{0}", labArr[j - prev].ToString());
                dgvSchedCrs.Rows[j].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            }

            int col = 0;
            for (int r = 0; r < roomArr.Count; r++)
            {
                for (int l = 0; l < labArr.Count + 1; l++)
                {
                    Slot temp = timeTable.pool.getHead(r, l);
                    while (temp != null)
                    {
                        int row = 0;
                        foreach (Room rm in temp.rooms)
                        {
                            if (rm != null)
                                dgvSchedCrs.Rows[row].Cells[col].Value = rm.getAls();
                            row++;
                        }

                        foreach (Room lb in temp.labs)
                        {
                            if (lb != null)
                                dgvSchedCrs.Rows[row].Cells[col].Value = lb.getAls();
                            row++;
                        }
                        temp = temp.next;
                        col++;
                    }
                }
            }
        }

        private void initGridViewTrack()
        {
            var dgCol1 = new DataGridViewTextBoxColumn();
            dgCol1.HeaderText = "Course Code";
            dgvSchedCrs.Columns.Add(dgCol1);

            var dgCol2 = new DataGridViewTextBoxColumn();
            dgCol2.HeaderText = "Course Description";
            dgvSchedCrs.Columns.Add(dgCol2);

            var dgCol3 = new DataGridViewTextBoxColumn();
            dgCol3.HeaderText = "Section";
            dgvSchedCrs.Columns.Add(dgCol3);

            var dgCol4 = new DataGridViewTextBoxColumn();
            dgCol4.HeaderText = "Hours per Day";
            dgvSchedCrs.Columns.Add(dgCol4);

            var dgCol5 = new DataGridViewTextBoxColumn();
            dgCol5.HeaderText = "Classes per Week";
            dgvSchedCrs.Columns.Add(dgCol5);

            var dgCol6 = new DataGridViewTextBoxColumn();
            dgCol6.HeaderText = "Inserted";
            dgvSchedCrs.Columns.Add(dgCol6);
        }

        private void insertCrs(string val)
        {
            timeTable.addRoomCrs(cmbSelDay.SelectedItem.ToString(), dgvTimeTable.Columns[dgvTimeTable.SelectedCells[0].ColumnIndex].HeaderText,
                dgvTimeTable.SelectedCells[0].RowIndex, val);

            updateTimeTableGrid(cmbSelDay.SelectedItem.ToString());
        }

        private void SubmenuItem_Click(object sender, EventArgs e)
        {
            //var clickedMenuItem = sender as MenuItem;
            insertCrs(sender.ToString());
        }
        #endregion

        // background worker events
        #region

        //bgClash worker events
        #region
        /// <summary>
        /// event to determine clashes between different courses
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bgClash_DoWork(object sender, DoWorkEventArgs e)
        {
            crsHash.initClashSet(crsHash.getCount());
            Section clist = new Section();
            Section ctree = new Section();
            BackgroundWorker worker = sender as BackgroundWorker;
            //List<Course>
            //int i=0;
            foreach (Course c in crsList)
            {
                List<Section> sec = new List<Section>();
                sec = crsHash.getSectionList(c.getID());
                foreach (Section s in sec)
                {
                    crsHash.populateeClashSet(c.getID(), c.getName(), s.getID());
                    MasterDom.insertEntry(c.getID(), s.getID(), c.getAlias());
                    if (c.getCredits() == 1)
                        track.insertEntry(c.getID(), new HashNode<trackNode>(c.getID(), s.getID(), 3, new trackNode(3, 3)));
                    else if (c.getCredits() != 1)
                        track.insertEntry(c.getID(), new HashNode<trackNode>(c.getID(), s.getID(), c.getCredits(), new trackNode(c.getCredits(), 1)));
                    //mDom.Add(new DomainNode(c.getID(), s.getID(), c.getAlias()));
                    //cmbSlot.Items.Add(c.getID() + " " + s.getID());
                    //gvClsCourse.Rows.Add(c.getID(), c.getName(), s.getID());
                }
            }
            List<Clash> list = crsHash.getClashHeads();
            foreach (Clash c in list)
            {
                gvClsCourse.Rows.Add(c.CrsID, c.CrsName, c.secID);
            }

            //Thread.Sleep(10000);
            //int chk = 0;
            foreach (Clash c in list)
            {
                clist = crsHash.getSection(c.CrsID, c.secID);
                foreach (Clash d in list)
                {
                    if (c == d)
                        continue;
                    ctree = crsHash.getSection(d.CrsID, d.secID);
                    crsHash.InsertClash(c, clist, d, ctree);
                }
                //chk++;
                //txtSecCount.Text = chk.ToString();
            }

            for (int i = 0; i < gvClsCourse.Rows.Count - 1; i++)
            {
                gvClsCourse.Rows[i].Cells[3].Value = crsHash.getClashesCount(gvClsCourse.Rows[i].Cells[0].Value.ToString(), gvClsCourse.Rows[i].Cells[2].Value.ToString()).ToString();
                worker.ReportProgress((i / (gvClsCourse.Rows.Count - 1)) * 100);
            }
        }

        /// <summary>
        /// to change value of progress bar as the clashes are recognized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bgClash_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progInsert.Value = e.ProgressPercentage;
        }
        #endregion

        //bgSaved worker events
        #region
        private void bgSaved_DoWork(object sender, DoWorkEventArgs e)
        {
            string path = Path.GetDirectoryName(fileName);
            //string alsPath = path + "alias.scd";
            string aPath = path + @"\alias.txt";
            if (File.Exists(aPath))
            {
                string[] alsList = File.ReadAllLines(aPath);
                foreach (string strAls in alsList)
                {
                    string[] crs = strAls.Split('-');
                    crsHash.setAls(crs[0], crs[1]);
                }
            }

            string batchPath = path + "\\batch.txt";
            if (File.Exists(batchPath))
            {
                string[] bList = File.ReadAllLines(batchPath);
                foreach (string bLine in bList)
                {
                    string[] val = bLine.Split('-');
                    Batch b = new Batch(Int32.Parse(val[0].ToString()), Color.FromName(val[1]));
                    b.setRepeat(bool.Parse(val[2].ToString()));
                    batchList.Add(b);
                }

            }
            string crsBatchPath = path + "\\courseBatch.txt";
            if (File.Exists(crsBatchPath))
            {
                string[] cbList = File.ReadAllLines(crsBatchPath);
                foreach (string cbLine in cbList)
                {
                    string[] val = cbLine.Split('-');
                    if (val[2].ToString() == "0")
                        continue;

                    else
                    {
                        Batch b = new Batch();
                        string[] chk = val[2].Split(' ');
                        b.setBatch(Int32.Parse(chk[0].ToString()));
                        if (chk.Length > 1)
                            b.setRepeat(true);

                        foreach (Batch bt in batchList)
                        {
                            if ((b.getBatch() == bt.getBatch()) && (b.getRepeat() == bt.getRepeat()))
                            {
                                b.setColor(bt.getColor());
                                break;
                            }
                        }

                        if (val[1].ToString() == "All")
                            crsHash.setBatch(val[0].ToString(), b);

                        else
                            crsHash.setBatch(val[0].ToString(), val[1].ToString(), b);
                    }
                }
            }
        }
        #endregion

        #endregion

        //all events to handle controls on the Courses tab
        #region

        //courses datagridview events
        #region
        /// <summary>
        /// event called when user clicks on a course in the Courses Datagrid view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCrs_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //int ind = dgvCrs.SelectedCells[0].RowIndex;
            if (e.RowIndex > -1)
            {
                DataGridViewRow row = dgvCrs.Rows[e.RowIndex];
                string id = row.Cells[0].Value.ToString();
                //id = dgvCrs.SelectedRows[0].Cells[0].Value.ToString();
                //txtTemp.Text = id.ToString();
                dgvCrsSec.Rows.Clear();
                dgvCrsSec.Refresh();
                List<Section> secData = new List<Section>();
                secData = crsHash.getSectionList(id.ToString());

                foreach (Section s in secData)
                {
                    dgvCrsSec.Rows.Add(s.getID());
                }
                dgvCrsSec.Sort(dgvColsecID, ListSortDirection.Ascending);
                dgvSecStu.Rows.Clear();
                try
                {
                    Batch bTemp = crsHash.getBatch(id);
                    string val = bTemp.getBatch().ToString();
                    if (bTemp.getRepeat() == true)
                        val += " R";
                    cmbBatch.SelectedIndex = cmbBatch.FindString(val);
                }
                catch
                {
                    cmbBatch.SelectedIndex = -1;
                }
                string str = track.ToString();
            }
        }

        /// <summary>
        /// event called when a user enters an alias for a course
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCrs_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 3)
            {
                int col = e.ColumnIndex;
                if (dgvCrs.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                    crsHash.setAls(dgvCrs.Rows[e.RowIndex].Cells[0].Value.ToString(), dgvCrs.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
            }

            updateMDom();
        }
        #endregion

        /// <summary>
        /// event to show students and instructors of a section when clicked on Section Gridview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgvCrsSec_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = dgvCrsSec.Rows[e.RowIndex];
            string sec = row.Cells[0].Value.ToString();
            //id = dgvCrs.SelectedRows[0].Cells[0].Value.ToString();
            //txtTemp.Text = id.ToString();
            string crsID = dgvCrs.Rows[dgvCrs.SelectedCells[0].RowIndex].Cells[0].Value.ToString();
            //txtTemp.Text = sec;
            txtSecIns.Text = crsHash.getSecIns(crsID, sec);
            List<Student> stus = crsHash.getSecStu(crsID, sec);
            TreeNode stTree = new TreeNode();
            stTree.Nodes.Add(crsHash.getSecStuTree(crsID, sec));
            dgvSecStu.Rows.Clear();
            //txtStuCount.Text = stus.Count.ToString();
            foreach (Student s in stus)
            {
                dgvSecStu.Rows.Add(s.getSID().ToString());
            }
            tvStu.Nodes.Clear();
            tvStu.Nodes.Add(stTree);
            dgvSecStu.Refresh();
        }

        /// <summary>
        /// to assign a batch to the selected Course Section(s)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAssignBatch_Click(object sender, EventArgs e)
        {
            Batch b = decryptBatch(cmbBatch.SelectedItem.ToString());
            try
            {
                string id = dgvSecStu.Rows[dgvSecStu.SelectedCells[0].RowIndex].Cells[0].Value.ToString();
                crsHash.setBatch(dgvCrs.Rows[dgvCrs.SelectedCells[0].RowIndex].Cells[0].Value.ToString(), dgvCrsSec.Rows[dgvCrsSec.SelectedCells[0].RowIndex].Cells[0].Value.ToString(), b);
            }

            catch
            {
                crsHash.setBatch(dgvCrs.Rows[dgvCrs.SelectedCells[0].RowIndex].Cells[0].Value.ToString(), b);
            }
        }
        #endregion

        //all events to handle controls on the Clashes tab
        #region
        /// <summary>
        /// to show the courses associated with the course clicked in the Clash DataGridview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gvClsCourse_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;
            DataGridViewRow row = gvClsCourse.Rows[e.RowIndex];
            String CrsID = row.Cells[0].Value.ToString();
            String secID = row.Cells[2].Value.ToString();
            List<Clash> list = crsHash.getClashList(CrsID, secID);
            gvSelCrsClash.Rows.Clear();
            gvSelCrsClash.Refresh();
            //MasterDom.ToString();
            foreach (Clash c in list)
            {
                DataGridViewRow ClashRow = (DataGridViewRow)gvSelCrsClash.Rows[0].Clone();
                ClashRow.Cells[0].Value = c.CrsID;
                ClashRow.Cells[1].Value = c.CrsName;
                ClashRow.Cells[2].Value = c.secID;
                //gvSelCrsClash.Rows.Add(c.CrsID, c.CrsName, c.secID);
                if (c.type == "Instructor")
                    for (int i = 0; i < 3; i++)
                        ClashRow.Cells[i].Style.BackColor = Color.Salmon;

                else if (c.type == "Section")
                    for (int i = 0; i < 3; i++)
                        ClashRow.Cells[i].Style.BackColor = Color.LightSalmon;

                else if (c.type == "Student")
                    for (int i = 0; i < 3; i++)
                        ClashRow.Cells[i].Style.BackColor = Color.MistyRose;

                gvSelCrsClash.Rows.Add(ClashRow);
            }
        }

        /// <summary>
        /// to show the students associated with the clash
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gvSelCrsClash_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
                return;
            dvClashStu.Rows.Clear();
            DataGridViewRow row = gvSelCrsClash.Rows[e.RowIndex];
            String CrsID = row.Cells[0].Value.ToString();
            String secID = row.Cells[2].Value.ToString();
            String headCrs = gvClsCourse.Rows[gvClsCourse.SelectedCells[0].RowIndex].Cells[0].Value.ToString();
            String headSec = gvClsCourse.Rows[gvClsCourse.SelectedCells[0].RowIndex].Cells[2].Value.ToString();
            List<Int32> stulist = crsHash.getStuClashList(headCrs, headSec, CrsID, secID);
            if (stulist == null)
                return;
            for (int i = 0; i < stulist.Count; i++)
                dvClashStu.Rows.Add(stulist[i].ToString());

            dvClashStu.Refresh();
        }

        /// <summary>
        /// to let user enter a new batch 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbBatch_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbBatch.SelectedIndex == 0)
            {
                BatchSelector b = new BatchSelector();
                b.showForEdit(ref batchList);
                cmbBatch.Items.Clear();
                cmbBatch.Items.Add("Add new batch..");
                foreach (Batch bt in batchList)
                {
                    string val = bt.getBatch().ToString();
                    if (bt.getRepeat() == true)
                        val += " R";
                    cmbBatch.Items.Add(val);
                }
            }

            else
            {
                //crsHash.setBatch(.SelectedRows[0].Cells[0].Value.ToString(), Int32.Parse(cmbBatch.SelectedItem.ToString()));
            }
        }
        #endregion

        //all events to handle controls in Schedule tab
        #region

        /// <summary>
        /// event called when Schedule tab is clicked to ask user for the type of timetable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab.Text == "Schedule")
            {
                if (timeTable == null)
                {
                    initDays();
                    updateSelDays();
                }

                else
                {
                    updateSelDays();
                }
            }

            if (tabControl1.SelectedTab.Text == "Insertion")
            {
                if (timeTable == null)
                    return;
                float totalC = 0;
                float insC = 0;
                List<Clash> list = crsHash.getClashHeads();
                //initGridViewTrack();
                int i = 0;
                dgvSchedCrs.Rows.Clear();
                foreach (Clash c in list)
                {
                    if (timeTable == null)
                        return;
                    dgvSchedCrs.Rows.Add();
                    HashNode<trackNode> trNode = timeTable.getCheckItem(c.CrsID, c.secID);
                    dgvSchedCrs.Rows[i].Cells[0].Value = trNode.crsID;
                    dgvSchedCrs.Rows[i].Cells[1].Value = c.CrsName;
                    dgvSchedCrs.Rows[i].Cells[2].Value = trNode.secID;
                    dgvSchedCrs.Rows[i].Cells[3].Value = trNode.obj.CperDay;
                    dgvSchedCrs.Rows[i].Cells[4].Value = trNode.obj.CperWeek;
                    dgvSchedCrs.Rows[i].Cells[5].Value = (trNode.obj.CperWeek - trNode.remClass).ToString();
                    totalC += trNode.obj.CperWeek;
                    insC += trNode.obj.CperWeek - trNode.remClass;
                    i++;
                }
                lblSuccess.Text = ((insC / totalC) * 100).ToString() + " %";
            }
        }

        // events behind buttons and combo boxes
        #region
        /// <summary>
        /// event called when user hits Reset button. the timetable and all objects associated are re-initialized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReset_Click(object sender, EventArgs e)
        {
            timeTable.Reset(MasterDom);
        }

        /// <summary>
        /// event called when user changes day in the days combo box. the timetable of the particular day is displayed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbSelDay_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSelDay.SelectedItem.ToString() == "Add Days...")
            {
                initDays();
                updateSelDays();
            }

            else
                updateTimeTableGrid(cmbSelDay.SelectedItem.ToString());

        }

        private void btnInsertCrs_Click(object sender, EventArgs e)
        {
            if (cmbSlot.SelectedIndex > -1)
            {
                insertCrs(cmbSlot.SelectedItem.ToString());
            }

        }

        // event called when user hits Generate Timetable button
        private void btnAutoTT_Click(object sender, EventArgs e)
        {
            progInsert.Visible = true;
            timeTable.generateTimeTable(ref progInsert);
            MessageBox.Show("Completed");
            updateTimeTableGrid(cmbSelDay.SelectedItem.ToString());
            progInsert.Visible = false;
        }
        #endregion

        // events of controls in Schedule Options to add or remove a slot
        #region
        /// <summary>
        /// event behind the Add Slot button to display options for adding slot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShowAddSlot_Click(object sender, EventArgs e)
        {
            slotInsertion(true);
        }

        /// <summary>
        /// event behind the Rem Slot button to remove a particular slot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRemSlot_Click(object sender, EventArgs e)
        {
            string slotHead;
            try
            {
                slotHead = dgvTimeTable.Columns[dgvTimeTable.SelectedCells[0].ColumnIndex].HeaderText;
                if (chkSltAll.Checked == true)
                    timeTable.RemSlotAll(slotHead);

                else if (chkSltAll.Checked == false)
                    timeTable.RemSlot(Int32.Parse(cmbSelDay.SelectedIndex.ToString()), slotHead);
            }
            catch
            {
                MessageBox.Show("Please Select a Slot", "Error");
            }
            updateTimeTableGrid(cmbSelDay.SelectedItem.ToString());
        }
        
        /// <summary>
        /// event behind the start time combo box to set value of finish time automatically
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dtpSlotSTime_ValueChanged(object sender, EventArgs e)
        {
            dtpSlotETime.Value = dtpSlotSTime.Value.AddHours(1);
        }

        private void btnSlotCancel_Click(object sender, EventArgs e)
        {
            slotInsertion(false);
        }

        private void btnAddSlot_Click(object sender, EventArgs e)
        {
            roomArr.Clear();
            for (int i = 0; i < chkRooms.Length; i++)
            {
                if (!chkRooms[i].Checked)
                    continue;

                roomArr.Add(chkRooms[i].Text);
            }

            labArr.Clear();
            for (int i = 0; i < chkLabs.Length; i++)
            {
                if (!chkLabs[i].Checked)
                    continue;

                labArr.Add(chkLabs[i].Text);
            }
            updateMDom();
            Slot inNew = new Slot(roomArr.Count, labArr.Count, dtpSlotSTime.Value, dtpSlotETime.Value, MasterDom.getDomList(), batchList);
            if (chkBreak.Checked == true)
                inNew.breakTime = true;

            if (chkSltAll.Checked == false)
                timeTable.addDaySlot(cmbSelDay.SelectedItem.ToString(), inNew);

            else if (chkSltAll.Checked == true)
                timeTable.addDaySlotAll(inNew);
            updateTimeTableGrid(cmbSelDay.SelectedItem.ToString());
            slotInsertion(false);
        }
#endregion

        // mouse events for timetable gridview
        #region
        private Point source;
        private Point destination;
        private void dgvTimeTable_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                Point clientPoint = dgvTimeTable.PointToClient(new Point(e.X, e.Y));
                destination.X = dgvTimeTable.HitTest(clientPoint.X, clientPoint.Y).ColumnIndex;
                destination.Y = dgvTimeTable.HitTest(clientPoint.X, clientPoint.Y).RowIndex;
                if (destination.X == source.X)
                {
                    if (destination.Y != source.Y)
                        timeTable.swapRoom(cmbSelDay.SelectedItem.ToString(), dgvTimeTable.Columns[source.X].HeaderText, source.Y, destination.Y);
                }

                else
                {
                    MessageBox.Show("Cross Column operations not allowed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                updateTimeTableGrid(cmbSelDay.SelectedItem.ToString());
            }
            catch
            { return; }
        }

        private void dgvTimeTable_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void dgvTimeTable_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                source.Y = e.RowIndex;
                source.X = e.ColumnIndex;
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    if (dgvTimeTable.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
                        dgvTimeTable.DoDragDrop(dgvTimeTable.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), DragDropEffects.Move);
                }
            }

            catch
            {
                return;
            }
        }

        private void dgvTimeTable_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    ContextMenuStrip menu = new ContextMenuStrip();
                    menu.Items.Add("Insert");
                    menu.Items.Add("Soft Clash");
                    menu.Items.Add("Remove");

                    List<DomainNode> tempList = timeTable.getSlotDom(cmbSelDay.SelectedItem.ToString(), dgvTimeTable.Columns[dgvTimeTable.SelectedCells[0].ColumnIndex].HeaderText);
                    //cmbSlot.Items.Clear();
                    foreach (DomainNode dmN in tempList)
                        (menu.Items[0] as ToolStripMenuItem).DropDown.Items.Add("[" + dmN.Alias + "]" + dmN.clsID + " " + dmN.secID, null, new EventHandler(SubmenuItem_Click));

                    tempList = new List<DomainNode>();

                    tempList = timeTable.getStuSlotDom(cmbSelDay.SelectedItem.ToString(), dgvTimeTable.Columns[dgvTimeTable.SelectedCells[0].ColumnIndex].HeaderText);
                    foreach (DomainNode dsN in tempList)
                        (menu.Items[1] as ToolStripMenuItem).DropDown.Items.Add("[" + dsN.Alias + "]" + dsN.clsID + " " + dsN.secID, null, new EventHandler(SubmenuItem_Click));
                    menu.ItemClicked += (send, ev) =>
                    {
                        if (ev.ClickedItem.Text == "Remove")
                        {
                            timeTable.remRoomCrs(cmbSelDay.SelectedItem.ToString(), dgvTimeTable.SelectedCells[0].RowIndex, dgvTimeTable.Columns[dgvTimeTable.SelectedCells[0].ColumnIndex].HeaderText);
                        }
                        updateTimeTableGrid(cmbSelDay.SelectedItem.ToString());
                    };
                    try
                    {
                        if (dgvTimeTable.SelectedCells[0].Value.ToString() != "")
                        {
                            menu.Items[0].Enabled = false;
                            menu.Items[1].Enabled = false;
                            menu.Items[2].Enabled = true;
                        }
                    }

                    catch
                    {
                        menu.Items[0].Enabled = true;
                        menu.Items[1].Enabled = true;
                        menu.Items[2].Enabled = false;
                    }

                    menu.Show(dgvTimeTable, new Point(e.X, e.Y));
                }
            }

            catch
            { return; }
        }

        #endregion
        #endregion

        // Excel Sheet automation according to the timetable saved
        #region
        /// <summary>
        /// generating Excel Sheet from the data provided in the Timetable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onGenSchedule_Click(object sender, EventArgs e)
        {
            if (cmbDept.SelectedIndex < 0)
            {
                MessageBox.Show("Please Select a Department!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbDept.Focus();
                return;
            }

            if (cmbSem.SelectedIndex < 0)
            {
                MessageBox.Show("Please Select a Semester!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbSem.Focus();
                return;
            }
            Microsoft.Office.Interop.Excel.Application oXL;
            Microsoft.Office.Interop.Excel._Workbook oWB;
            Microsoft.Office.Interop.Excel._Worksheet oSheet;
            //Microsoft.Office.Interop.Excel.Range oRng;



            roomArr.Clear();
            for (int i = 0; i < chkRooms.Length; i++)
            {
                if (!chkRooms[i].Checked)
                    continue;

                roomArr.Add(chkRooms[i].Text);
            }

            labArr.Clear();
            for (int i = 0; i < chkLabs.Length; i++)
            {
                if (!chkLabs[i].Checked)
                    continue;

                labArr.Add(chkLabs[i].Text);
            }

            try
            {
                Int32 rows = roomArr.Count + labArr.Count;
                //Start Excel and get Application object.
                oXL = new Microsoft.Office.Interop.Excel.Application();
                oXL.Visible = true;
                //oXL.DisplayAlerts = false;

                //Get a new workbook.
                oWB = (Microsoft.Office.Interop.Excel._Workbook)(oXL.Workbooks.Add(Missing.Value));
                oSheet = (Microsoft.Office.Interop.Excel._Worksheet)oWB.ActiveSheet;

                int totalSlots = timeTable.getMaxDaySlot();
                char b = 'C';
                char[] colHeads = new char[totalSlots];
                for (int i = 0; i < totalSlots; i++)
                {
                    colHeads[i] = (char)((int)b + 1);
                    b = colHeads[i];
                    oSheet.get_Range(b + "1").ColumnWidth = 18;
                }
                //Microsoft.Office.Tools.Excel.NamedRange setColumnRowRange;
                var list = from element in batchList
                           orderby element.batch descending
                           select element;

                var bDistinct = from element in list
                                orderby element.batch descending
                                select element.batch;
                bDistinct = bDistinct.Distinct().ToList();

                int xlBInd = 1;
                foreach (int xlB in bDistinct)
                {
                    oSheet.get_Range("H" + xlBInd.ToString(), "H" + xlBInd.ToString()).Value = "Batch " + xlB.ToString();
                    oSheet.get_Range("H" + xlBInd.ToString(), "H" + xlBInd.ToString()).HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    oSheet.get_Range("H" + xlBInd.ToString(), "H" + xlBInd.ToString()).VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                    //oSheet.get_Range("H" + xlBInd.ToString(), "I" + xlBInd.ToString()).Interior.Color = xlB;
                    xlBInd++;
                }

                var colorList = (from element in list
                                 orderby element.batch descending
                                 select element.color).ToList();

                xlBInd = 1;
                for (int stB = 0; stB < list.Count(); stB++)
                {
                    if ((stB % 2) == 0)
                    {
                        oSheet.get_Range("H" + xlBInd.ToString(), "H" + xlBInd.ToString()).Interior.Color = colorList.ElementAt(stB);
                        oSheet.get_Range("H" + xlBInd.ToString(), "H" + xlBInd.ToString()).HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    }

                    else if ((stB % 2) != 0)
                    {
                        oSheet.get_Range("I" + xlBInd.ToString(), "I" + xlBInd.ToString()).Interior.Color = colorList.ElementAt(stB);
                        oSheet.get_Range("I" + xlBInd.ToString()).Value = "R";
                        oSheet.get_Range("I" + xlBInd.ToString(), "I" + xlBInd.ToString()).HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        xlBInd++;
                    }
                }
                oSheet.get_Range("A1").ColumnWidth = 2.57;      // day column
                oSheet.get_Range("B1").ColumnWidth = 2.57;      // class/lab column
                oSheet.get_Range("C1").ColumnWidth = 7.14;      // room column

                oSheet.get_Range("A1", "A5").RowHeight = 20;
                /*oSheet.get_Range("A2").RowHeight = 20;
                oSheet.get_Range("A3").RowHeight = 20;
                oSheet.get_Range("A4").RowHeight = 20;*/
                oSheet.get_Range("A1", "F1").Merge();
                oSheet.Cells[1, 1] = "National University";
                oSheet.get_Range("A1").Font.Bold = true;
                oSheet.get_Range("A1").Font.Size = "20";
                oSheet.get_Range("A1", "A2").Font.Name = "Berlin Sans FB";
                oSheet.get_Range("A1", "A4").HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                oSheet.get_Range("A1", "A4").VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                oSheet.get_Range("A2", "F2").Merge();
                oSheet.get_Range("A3", "F3").Merge();
                oSheet.get_Range("A4", "F4").Merge();
                oSheet.Cells[2, 1] = "of Computer and Emerging Sciences";
                oSheet.get_Range("A2").Font.Size = "14";
                String dept;
                int d = cmbDept.SelectedIndex;
                switch (d)
                {
                    case 0:
                        dept = "Computer Science";
                        break;

                    case 1:
                        dept = "Electrical Engineering";
                        break;

                    case 2:
                        dept = "Management Science";
                        break;

                    default:
                        dept = "";
                        break;
                }
                oSheet.Cells[3, 1] = "Department of " + dept;
                oSheet.get_Range("A3").Font.Size = "14";
                oSheet.get_Range("A3").Font.Bold = true;
                oSheet.get_Range("A3", "A4").Font.Name = "Arial";

                String sem;
                d = cmbSem.SelectedIndex;
                switch (d)
                {
                    case 0:
                        sem = "Fall";
                        break;

                    case 1:
                        sem = "Spring";
                        break;

                    case 2:
                        sem = "Summer";
                        break;

                    default:
                        sem = "";
                        break;
                }

                oSheet.Cells[4, 1] = sem + " " + DateTime.Today.Year + " Semester";
                oSheet.get_Range("A4").Font.Size = "12";
                oSheet.get_Range("A4").Font.Bold = true;
                oSheet.get_Range("A1", "A4").Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);

                oSheet.get_Range("K2", "K3").Merge();
                oSheet.get_Range("K2", "K3").Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                oSheet.get_Range("K2", "K3").Font.Size = "14";
                oSheet.get_Range("K2", "K3").Font.Bold = true;
                oSheet.get_Range("K2", "K3").Value = "Week";
                oSheet.get_Range("K2", "K3").HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                oSheet.get_Range("K2", "K3").VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                oSheet.get_Range("K2", "M3").Font.Name = "Arial";

                oSheet.get_Range("L2", "M3").Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                oSheet.get_Range("L2", "M3").Font.Size = "12";
                oSheet.get_Range("L2", "M3").Font.Bold = true;
                oSheet.get_Range("L2", "M3").HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                oSheet.get_Range("L2").Value = "From";
                oSheet.get_Range("L3").Value = "To";
                oSheet.get_Range("R2", "T2").Merge();

                oSheet.get_Range("A7", colHeads[colHeads.Length - 1].ToString() + "7").Font.Name = "Arial";
                oSheet.get_Range("A7", colHeads[colHeads.Length - 1].ToString() + "7").Font.Size = 10;
                oSheet.get_Range("A7", colHeads[colHeads.Length - 1].ToString() + "7").HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                //oSheet.Cells[7, 3] = "Room";
                //oSheet.Cells[7, 4] = "08:30 - 09:25";
                //oSheet.Cells[7, 6] = "09:30 - 10:25";
                //oSheet.Cells[7, 8] = "10:30 - 11:25";
                //oSheet.Cells[7, 11] = "11:50 - 12:45";
                //oSheet.Cells[7, 13] = "12:50 - 01:45";
                //oSheet.Cells[7, 16] = "2:30 - 3:25";
                //oSheet.Cells[7, 18] = "3:30 - 4:25";
                //oSheet.Cells[7, 20] = "4:30 - 5:25";
                oSheet.get_Range("A7", colHeads[colHeads.Length - 1].ToString() + "7").Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                DateTime dt = new DateTime();
                Int32 start = 8;
                Int32 end = 0;
                List<string> daynames = timeTable.getDayNames();
                rows++;
                for (int i = 0; i < timeTable.getDayCount(); i++)
                {
                    Slot temp = timeTable.getDayHead(daynames.ElementAt(i));
                    List<string> slotHeads = new List<string>();
                    while (temp != null)
                    {
                        if (temp.breakTime == false)
                            slotHeads.Add(temp.getHeader());

                        else slotHeads.Add(" ");
                        temp = temp.next;
                    }
                    end = start + (rows - 1);
                    roomArr.Sort();
                    labArr.Sort();
                    string[] arr = new string[rows];
                    //string[] arrLb = new string[labArr.Count];
                    int arI = 0;
                    for (; arI < roomArr.Count; arI++)
                        arr[arI] = roomArr.ElementAt(arI);

                    arr[arI] = "Labs";
                    arI++;
                    for (int arL = arI; arL < rows; arL++)
                        arr[arL] = labArr.ElementAt(arL - arI);

                    string[,] values = new string[rows, 2];
                    int r = 0;
                    for (; r < rows; r++)
                    {
                        values[r, 0] = arr[r];
                    }

                    oSheet.get_Range("A" + (start - 1), colHeads[colHeads.Length - 1].ToString() + (start - 1)).Font.Name = "Arial";
                    oSheet.get_Range("A" + (start - 1), colHeads[colHeads.Length - 1].ToString() + (start - 1)).Font.Size = 10;
                    oSheet.get_Range("A" + (start - 1), colHeads[colHeads.Length - 1].ToString() + (start - 1)).HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    oSheet.Cells[start - 1, 3] = "Room";
                    for (int h = 0; h < slotHeads.Count; h++)
                    {
                        string val = slotHeads.ElementAt(h);
                        oSheet.Cells[start - 1, h + 4] = val;
                    }
                    //oSheet.Cells[start - 1, 5] = slotHeads.ElementAt(1);
                    //oSheet.Cells[start - 1, 6] = slotHeads.ElementAt(2);
                    //oSheet.Cells[start - 1, 7] = slotHeads.ElementAt(3);
                    //oSheet.Cells[start - 1, 8] = slotHeads.ElementAt(4);
                    //oSheet.Cells[start - 1, 9] = slotHeads.ElementAt(5);
                    //oSheet.Cells[start - 1, 10] = slotHeads.ElementAt(6);
                    //oSheet.Cells[start - 1, 11] = slotHeads.ElementAt(7);
                    oSheet.get_Range("A" + (start - 1), colHeads[colHeads.Length - 1].ToString() + (start - 1)).Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;

                    oSheet.get_Range("C" + start.ToString(), "C" + end.ToString()).Value2 = values;             //rooms and labs
                    oSheet.get_Range("A" + start.ToString(), "A" + end.ToString()).Merge();
                    oSheet.get_Range("A" + start.ToString(), "A" + end.ToString()).Value = daynames[i];
                    //oSheet.get_Range("E" + start.ToString(), "E" + end.ToString()).Merge();
                    //oSheet.get_Range("G" + start.ToString(), "G" + end.ToString()).Merge();
                    //oSheet.get_Range("I" + start.ToString(), "I" + end.ToString()).Merge();//
                    //oSheet.get_Range("I" + start.ToString(), "J" + end.ToString()).Value = "Tea Break";
                    //oSheet.get_Range("I" + start.ToString(), "J" + end.ToString()).Orientation = Microsoft.Office.Interop.Excel.XlOrientation.xlUpward;
                    //oSheet.get_Range("I" + start.ToString(), "J" + end.ToString()).VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                    //oSheet.get_Range("I" + start.ToString(), "J" + end.ToString()).HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    //oSheet.get_Range("K" + start.ToString(), "K" + end.ToString()).Merge();
                    //oSheet.get_Range("M" + start.ToString(), "M" + end.ToString()).Merge();//
                    //oSheet.get_Range("M" + start.ToString(), "M" + end.ToString()).Value = "Lunch & Prayer Break";
                    //oSheet.get_Range("M" + start.ToString(), "M" + end.ToString()).Orientation = Microsoft.Office.Interop.Excel.XlOrientation.xlUpward;
                    //oSheet.get_Range("M" + start.ToString(), "M" + end.ToString()).VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                    //oSheet.get_Range("M" + start.ToString(), "M" + end.ToString()).HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                    //oSheet.get_Range("O" + start.ToString(), "O" + end.ToString()).Merge();
                    //oSheet.get_Range("Q" + start.ToString(), "Q" + end.ToString()).Merge();
                    //oSheet.get_Range("U" + start.ToString(), "U" + end.ToString()).Merge();
                    oSheet.get_Range("A" + start.ToString(), "A" + end.ToString()).Orientation = Microsoft.Office.Interop.Excel.XlOrientation.xlUpward;
                    oSheet.get_Range("A" + start.ToString(), "A" + end.ToString()).VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;

                    temp = timeTable.getDayHead(daynames.ElementAt(i));
                    int rowInd = start;
                    int colInd = 4;
                    int colClr = 0;
                    while (temp != null)
                    {
                        rowInd = start;
                        //colClr = 0;
                        if (temp.breakTime == true)
                        {
                            TimeSpan bTime = temp.getEndTime() - temp.getStartTime();
                            if (bTime.Minutes < 30)
                            {
                                oSheet.get_Range(colHeads[colClr].ToString() + rowInd.ToString(), colHeads[colClr].ToString() + (rowInd + roomArr.Count - 1).ToString()).Merge();
                                oSheet.get_Range(colHeads[colClr].ToString() + rowInd.ToString(), colHeads[colClr].ToString() + (rowInd + roomArr.Count - 1).ToString()).Value = "Tea Break";
                            }

                            else
                            {
                                oSheet.get_Range(colHeads[colClr].ToString() + rowInd.ToString(), colHeads[colClr].ToString() + (rowInd + roomArr.Count - 1).ToString()).Merge();
                                oSheet.get_Range(colHeads[colClr].ToString() + rowInd.ToString(), colHeads[colClr].ToString() + (rowInd + roomArr.Count - 1).ToString()).Value = "Lunch & Prayer Break";
                            }
                            oSheet.get_Range(colHeads[colClr].ToString() + rowInd.ToString(), colHeads[colClr].ToString() + (rowInd + roomArr.Count - 1).ToString()).Orientation = Microsoft.Office.Interop.Excel.XlOrientation.xlUpward;
                            oSheet.get_Range(colHeads[colClr].ToString() + rowInd.ToString(), colHeads[colClr].ToString() + (rowInd + roomArr.Count - 1).ToString()).VerticalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;
                            oSheet.get_Range(colHeads[colClr].ToString() + rowInd.ToString(), colHeads[colClr].ToString() + (rowInd + roomArr.Count - 1).ToString()).HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                        }
                        foreach (Room rm in temp.rooms)
                        {
                            if (rm == null)
                                continue;
                            oSheet.Cells[rowInd, colInd] = rm.getAls();
                            Batch bClr = crsHash.getBatch(rm.getCrs(), rm.getSec());
                            foreach (Batch bt in batchList)
                            {
                                if (bt.Compare(bClr) == true)
                                    oSheet.get_Range(colHeads[colClr] + rowInd.ToString(), colHeads[colClr] + rowInd.ToString()).Interior.Color = bt.getColor();
                            }
                            rowInd++;
                        }
                        rowInd = start + roomArr.Count + 1;
                        foreach (Room lb in temp.labs)
                        {
                            if (lb == null)
                                continue;
                            oSheet.Cells[rowInd, colInd] = lb.getAls();
                            Batch bClr = crsHash.getBatch(lb.getCrs(), lb.getSec());
                            foreach (Batch bt in batchList)
                            {
                                if (bt.Compare(bClr) == true)
                                    oSheet.get_Range(colHeads[colClr] + rowInd.ToString(), colHeads[colClr] + rowInd.ToString()).Interior.Color = bt.getColor();
                            }
                            rowInd++;
                        }
                        colInd++;
                        colClr++;
                        temp = temp.next;
                    }
                    start = end + 2;
                }

                start = 8;
                oSheet.get_Range("A" + start, colHeads[colHeads.Length - 1].ToString() + end).Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
            }
            catch { }
        }
        #endregion

        // events to save the work done before exitting the application
        #region
        private void Main_Menu_FormClosed(object sender, FormClosedEventArgs e)
        {
            /*
            Marshal.ReleaseComObject(oRng);
            Marshal.ReleaseComObject(worksheet);
            Marshal.ReleaseComObject(sheets);
            Marshal.ReleaseComObject(theWorkbook);*/
            ExcelObj.Quit();
            Marshal.ReleaseComObject(ExcelObj);
        }

        private void Main_Menu_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dlgR = MessageBox.Show("Do you want to save the changes?", "Smart Schedule", MessageBoxButtons.YesNoCancel);
            if (dlgR == System.Windows.Forms.DialogResult.Cancel)
                e.Cancel = true;

            else if (dlgR == System.Windows.Forms.DialogResult.Yes)
            {
                string path = Path.GetDirectoryName(fileName);

                // storing alias information
                string aliasPath = path + "\\alias.txt";
                List<string> alsList = new List<string>();
                for (int i = 0; i < dgvCrs.Rows.Count; i++)
                {
                    if (dgvCrs.Rows[i].Cells[3].Value == null)
                        continue;
                    alsList.Add(dgvCrs.Rows[i].Cells[0].Value.ToString() + "-" + dgvCrs.Rows[i].Cells[3].Value.ToString());
                }
                writeToFile(aliasPath, alsList.ToArray());

                string batchPath = path + "\\batch.txt";
                List<string> batches = new List<string>();
                foreach (Batch bt in batchList)
                {
                    batches.Add(String.Format("{0}-{1}-{2}", bt.getBatch(), bt.getColor().Name, bt.getRepeat()));
                }
                writeToFile(batchPath, batches.ToArray());

                string crsBatchPath = path + "\\courseBatch.txt";
                List<string> cBatch = crsHash.getBatchList();
                writeToFile(crsBatchPath, cBatch.ToArray());
            }
        }
        #endregion

    }
}
