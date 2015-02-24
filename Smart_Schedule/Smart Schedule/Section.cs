using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Smart_Schedule
{
    public class Section
    {
        private String clsID;
        private String clsName;

        private Instructor clsIns;
        private List<Student> clsStu = new List<Student>();
        private StudentTree clsStuTree;

        private Batch batch;
        private bool repeat;

        private static Int32 secCount = 0;

        public Section()
        {
            clsID = "";
            clsName = "";
            clsIns = new Instructor();
            clsStu = new List<Student>();
            clsStuTree = new StudentTree();
            secCount++;
            repeat = false;
            batch = new Batch();
        }

        public Section(System.Array row)
        {
            clsID = row.GetValue(1, 6).ToString();
            clsIns = new Instructor(row.GetValue(1, 7).ToString(), row.GetValue(1, 8).ToString());
            clsStu.Add(new Student(Int32.Parse(row.GetValue(1, 1).ToString())));
            clsStuTree = new StudentTree(Int32.Parse(row.GetValue(1, 1).ToString()));
            secCount++;
            repeat = false;
            batch = new Batch();
        }

        public void setID(System.Array row)
        {
            clsID = row.GetValue(1, 6).ToString();
        }

        public void setName(String n)
        {
            clsName = n;
        }

        public void InsertStu(Int32 id)
        { 
            clsStu.Add(new Student(id));
            clsStuTree.Add(id);
        }

        public List<Student> getSecStu()
        {
            return clsStu;
        }

        public TreeNode getSecStuTree()
        {
            return clsStuTree.getStudentTree();
        }
        public String getID() { return clsID; }
        public String getName() { return clsName; }
        public String getIns() { return clsIns.getInsName(); }
        public Int32 getSecCount() { return secCount; }
        public StudentTree getStuRoot() { return clsStuTree; }
        public void setBatch(Batch b) {
            batch.setBatch(b.getBatch());
            batch.setColor(b.getColor());
            batch.setRepeat(b.getRepeat());
        }
        public Batch getBatch() { return batch; }
    }
}
