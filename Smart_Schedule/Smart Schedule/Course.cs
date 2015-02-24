using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Smart_Schedule
{
    public class Course
    {
        private String crsID;           // to store the Course ID
        private String crsName;         // to store the Course Name
        private String crsAlias;        // the Name of the Course to be displayed on the Timetable
        private Int32 crsCredits;
        private bool lab;               // to determine whether the selected course is a lab or not

        private List<Section> crsSec;

        public Course next;
        public Course()
        {// default constructor will initialize all elements to blank
            crsAlias = "";
            crsID = "";
            crsName = "";
            lab = false;
            next = null;
            crsSec = new List<Section>();       // list of different Course Sections
        }

        public Course(String id)
        {//over-loaded constructor to set the ID on initialization
            crsID = id;
            next = null;
            crsSec = new List<Section>();
        }

        public Course(String id, String name)
        {//over-loaded constructor to set ID and Name on initialization
            crsID = id;
            crsName = name;
            next = null;
            crsCredits = 3;
            if (name.Contains("Lab"))
            {
                lab = true;
                crsCredits = 1;
            }
            crsSec = new List<Section>();
        }

        public void setAlias(String al)
        {//alias to be set by the user on custom basis
            crsAlias = al;
        }

        public void setBatch(Batch b)
        {
            foreach (Section sec in crsSec)
            {
                sec.setBatch(b);
            }
        }

        public void setBatch(Batch b, string secID)
        {
            foreach (Section sec in crsSec)
            {
                if (sec.getID() == secID)
                    sec.setBatch(b);
            }
        }

        public Batch getBatch()
        {
            Batch bt = crsSec[0].getBatch();
            foreach (Section sec in crsSec)
            {
                if (bt.Compare(sec.getBatch()))
                    continue;

                else
                    return null;
            }
            return bt;
        }

        public Batch getBatch(string secID)
        {
            foreach (Section sec in crsSec)
            {
                if (sec.getID() == secID)
                    return sec.getBatch();
            }
            return null;
        }

        public List<string> getBatchList(ref List<string> List)
        {
            Batch b = new Batch();
            b = this.getBatch();
            if (b != null)
            {
                string val = b.getBatch().ToString();
                if (b.getRepeat() == true)
                    val += " R";
                List.Add(String.Format("{0}-{1}-{2}", this.crsID, "All", val));
            }

            else
            {
                foreach (Section s in crsSec)
                {
                    b = this.getBatch(s.getID());
                    if (b == null)
                        continue;

                    else
                    {
                        string val = b.getBatch().ToString();
                        if (b.getRepeat() == true)
                            val += " R";
                        List.Add(String.Format("{0}-{1}-{2}", this.crsID, s.getID(), val));
                    }
                }
            }
            return List;
        }
        public void AddSec(System.Array row)
        {
            if (!checkDupSec(row.GetValue(1, 6).ToString()))
            {
                crsSec.Add(new Section(row));
            }

            else
            {
                foreach (Section s in crsSec)
                {
                    if (s.getID() == row.GetValue(1, 6).ToString())
                    {
                        s.InsertStu(Int32.Parse(row.GetValue(1, 1).ToString()));
                        return;
                    }
                }
            }
        }

        public bool checkDupSec(String sec)
        {
            for (int i = 0; i < crsSec.Count; i++)
            {
                if (crsSec[i].getID() == sec)
                    return true;
            }
            return false;
        }

        public List<Section> getCrsSec()
        {
            return crsSec;
        }

        public String getCrsIns(string sec)
        {
            foreach (Section s in crsSec)
            {
                if (s.getID() == sec)
                    return s.getIns();
            }
            return null;
        }

        public Section getSection(string sec)
        {
            foreach (Section s in crsSec)
            {
                if (s.getID() == sec)
                    return s;
            }
            return null;
        }
        public List<Student> getSecStu(string sec)
        {
            List<Student> t = new List<Student>();
            t = null;
            foreach (Section s in crsSec)
            {
                if (s.getID() == sec)
                    t = s.getSecStu();
            }
            return t;
        }

        public TreeNode getSecStuTree(string sec)
        {
            TreeNode t = new TreeNode();
            t = null;
            foreach (Section s in crsSec)
            {
                if (s.getID() == sec)
                    t = s.getSecStuTree();
            }
            return t;
        }

        public string getSecAls(string secID)
        {
            foreach (Section s in crsSec)
            {
                if (s.getID() == secID)
                {
                    return crsAlias + " " + secID[secID.Length-1].ToString();
                }
            }
            return "";
        }
        public String getID() { return crsID; }
        public String getName() { return crsName; }
        public String getAlias() { return crsAlias; }
        public Int32 getCredits() { return crsCredits; }
        public Int32 getCrsSecCount() { return crsSec[0].getSecCount(); }
    }

    public class CourseList
    {// a Link List of courses
        private Course head;

        public CourseList()
        {
            head = null;
        }

        public CourseList(int i)
        {// for global 
        }
        public bool checkDup(String k)
        {// checks if there already exists value k in the Link List
            Course temp = head;
            while (temp != null)
            {
                if (temp.getID() == k)
                {//if value is found, it will return true
                    return true;
                }
                temp = temp.next;   // else it will move to next Entry
            }
            return false;           // if no duplicate entry is found, the function will return false
        }

        public bool Add(System.Array row)
        {// the function will add a node in the existing list, and return true upon successful insertion
            if (!checkDup(row.GetValue(1, 4).ToString()))
            {// if there exists no duplicate value in list
                Course temp = new Course(row.GetValue(1, 4).ToString(), row.GetValue(1, 5).ToString());
                temp.next = head;
                head = temp;
                temp.AddSec(row);
                return true;
            }

            else
            {// if a course exists, check for section
                Course temp = head;
                while (temp.getID() != row.GetValue(1, 4).ToString())
                {
                    temp = temp.next;
                }

                temp.AddSec(row);
            }
            return false;       // if value is not inserted in the list
        }

        public List<Section> getCrsList(String k)
        {
            Course temp = head;
            while (temp.getID() != k)
                temp = temp.next;
            return temp.getCrsSec();
        }

        public String getCrsIns(String id, String sec)
        {
            Course temp = head;
            while (temp.getID() != id)
                temp = temp.next;

            return temp.getCrsIns(sec);
        }

        public List<Student> getStu(String id, String sec)
        {
            Course temp = head;
            while (temp.getID() != id)
                temp = temp.next;

            return temp.getSecStu(sec);
        }

        public Section getSection(String id, String sec)
        {
            Course temp = head;
            while (temp.getID() != id)
                temp = temp.next;

            return temp.getSection(sec);
        }

        public TreeNode getSecStuTree(String id, String sec)
        {
            Course temp = head;
            while (temp.getID() != id)
                temp = temp.next;

            return temp.getSecStuTree(sec);
        }

        public Int32 getCrsCredits(String id)
        {
            Course temp = head;
            while (temp.getID() != id)
                temp = temp.next;

            return temp.getCredits();
        }

        public Int32 getCrsSecCount() { return head.getCrsSecCount(); }

        public Course getList()
        {
            return head;
        }

        public void setBatch(string crsID, Batch b)
        {
            Course temp = head;
            while (temp.getID() != crsID)
                temp = temp.next;

            temp.setBatch(b);
        }

        public void setBatch(string crsID, string secID, Batch b)
        {
            Course temp = head;
            while (temp.getID() != crsID)
                temp = temp.next;

            temp.setBatch(b, secID);
        }

        public Batch getBatch(string crsID)
        {
            Course temp = head;
            while (temp.getID() != crsID)
                temp = temp.next;

            return temp.getBatch();
        }

        public Batch getBatch(string crsID, string secID)
        {
            Course temp = head;
            if (temp == null)
                return null;
            while (temp.getID() != crsID)
                temp = temp.next;

            return temp.getBatch(secID);
        }

        public List<string> getBatchList(ref List<string> List)
        {
            Course temp = head;
            while (temp != null)
            {
                temp.getBatchList(ref List);
                temp = temp.next;
            }
            return List;
        }
        public void setAlias(string crsID, string als)
        {
            if (head == null)
                return;
            Course temp = head;
            while (temp.getID() != crsID)
            {
                temp = temp.next;
                if (temp == null)
                    return;
            }

            temp.setAlias(als);
        }

        public string getAlias(string crsID)
        {
            Course temp = head;
            while (temp.getID() != crsID)
                temp = temp.next;

            return temp.getAlias();
        }

        public string getSecAls(string crsID, string secID)
        {
            Course temp = head;
            while (temp.getID() != crsID)
                temp = temp.next;

            return temp.getSecAls(secID);
        }

        public void removeCourse(string crsID)
        {
            Course temp = head;
            if (head.getID() == crsID)
            {
                head = head.next;
                temp.next = null;
            }

            else
            {
                Course prev = temp;
                while (temp != null)
                {
                    //Course prev = temp;
                    temp = temp.next;
                }
            }
        }
    }

    class CourseHash
    {
        private static CourseList[] table = new CourseList[10];
        public static ClashSet clashSet;

        public CourseHash()
        {
            for (int i = 0; i < 10; i++)
            {
                table[i] = new CourseList();
            }
        }

        public CourseHash(int i)
        { }
        public void populateeClashSet(String crs, String name, String sec)
        {
            clashSet.Initialize(crs, name, sec);
        }

        public void initClashSet(int count)
        {
            clashSet = new ClashSet(this.getCount());
        }

        public List<Clash> getClashHeads()
        {
            return clashSet.getHeads();
        }
        
        public void InsertClash(Clash c, Section clist, Clash d, Section ctree)
        {
            clashSet.InsertClash(c, clist, d, ctree);
        }

        public Int32 getClashesCount(String crsID, String SecID)
        {
            return clashSet.getClashCount(crsID, SecID);
        }
        public List<Clash> getClashList(String crsID, String secID)
        {
            return clashSet.getClashes(crsID, secID);
        }
        public Int32 getHashVal(String k)
        {// hash function to calculate the index of the table for insertion of the value
            //k = (String)k.Reverse();
            try
            {
                return Int32.Parse(k.ElementAt(k.Length - 1).ToString());
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return 0;
            }
        }

        public bool Insert(Int32 i, System.Array row)
        {// will add the new value in the hash table and return the status of insertion
            return table[i].Add(row);
        }

        public List<Section> getSectionList(String k)
        {
            return table[this.getHashVal(k)].getCrsList(k);
        }

        public String getSecIns(String Crsid, String secID)
        {
            return table[this.getHashVal(Crsid)].getCrsIns(Crsid, secID);
        }

        public List<Student> getSecStu(String Crsid, String secID)
        {
            return table[this.getHashVal(Crsid)].getStu(Crsid, secID);
        }

        public TreeNode getSecStuTree(String Crsid, String secID)
        {
            return table[this.getHashVal(Crsid)].getSecStuTree(Crsid, secID);
        }

        public Int32 getCrsCredits(String Crsid)
        {
            return table[this.getHashVal(Crsid)].getCrsCredits(Crsid);
        }

        public Section getSection(String Crsid, String secID)
        {
            return table[this.getHashVal(Crsid)].getSection(Crsid, secID);
        }
        public Int32 getCount()
        {// to get the total number of course sections.
            // since the secCount is static, so it can be retrieved from any location in the Set
            int i = 0;
            for (i = 0; i < 10; i++)
            {// traversing for the first valid entry in the hash table
                if (table[i].getList() == null)
                    continue;

                else break;
            }
            return table[i].getCrsSecCount();
            //return 0;
        }

        public List<Course> getCourseList()
        {
            List<Course> list = new List<Course>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(table[i].getList());
            }
            return list;
        }

        public Int32 getClashCount(String crsID, String secID)
        {
            return clashSet.getClashCount(crsID, secID);
        }

        public List<Int32> getStuClashList(String headCrs, String headSec, String CrsID, String SecID)
        {
            return clashSet.getClashStuList(headCrs, headSec, CrsID, SecID);
        }

        public void setBatch(string crsID, Batch b)
        {
            table[this.getHashVal(crsID)].setBatch(crsID, b);
        }

        public void setBatch(string crsID, string secID, Batch b)
        {
            table[this.getHashVal(crsID)].setBatch(crsID, secID, b);
        }

        public Batch getBatch(string crsID)
        {
            return table[this.getHashVal(crsID)].getBatch(crsID);
        }

        public Batch getBatch(string crsID, string secID)
        {
            return table[this.getHashVal(crsID)].getBatch(crsID, secID);
        }

        public void setAls(string crsID, string als)
        {
            table[this.getHashVal(crsID)].setAlias(crsID, als);
        }

        public string getAls(string crsID)
        {
            return table[this.getHashVal(crsID)].getAlias(crsID);
        }

        public string getSecAls(string crsID, string secID)
        {
            return table[this.getHashVal(crsID)].getSecAls(crsID, secID);
        }

        public List<string> getBatchList()
        {
            List<string> List = new List<string>();
            for (int i = 0; i < 10; i++)
                table[i].getBatchList(ref List);
            return List;
        }
    }
}
