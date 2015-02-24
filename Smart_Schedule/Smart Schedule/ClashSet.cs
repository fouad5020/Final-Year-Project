using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smart_Schedule
{
    public class Clash
    {
        public String CrsID;
        public String CrsName;
        public String secID;
        public Clash next;
        public String type;
        public String InsName;
        public List<Int32> stuList = new List<Int32>();

        public DomainNode toDom(string al)
        {
            DomainNode newNode = new DomainNode(CrsID, secID, al);
            return newNode;
        }

        public string getClashType()
        {
            return type;
        }
    }

    public class ClashRow
    {
        public Clash head;
        public Int32 count;
        public ClashRow() { head = null; count = 0; }
        public void Initialize(String crs, String name, String sec)
        {
            Clash node = new Clash();
            node.CrsID = crs;
            node.secID = sec;
            node.CrsName = name;
            head = node;
            count = 0;
        }
    }

    public class ClashSet
    { 
        private static ClashRow[] Set;
        private static int Size;

        public ClashSet()
        { }
        public ClashSet(int num)
        {
            Size = num;
            Set = new ClashRow[num];
            for (int i = 0; i < num; i++)
                Set[i] = new ClashRow();
        }

        public void Initialize(String crs, String name, String sec)
        {
            int i = 0;
            while (Set[i].head != null)
                i++;

            Set[i].Initialize(crs, name, sec);
        }

        public List<Clash> getHeads()
        {
            List<Clash> list = new List<Clash>();
            for (int i = 0; i < Size; i++)
            {
                list.Add(Set[i].head);
            }

            return list;
        }

        public List<DomainNode> getMinClashHeads()
        {
            var clashes = from element in Set
                        orderby element.count ascending
                        select element.head;

            CourseHash cHash = new CourseHash(1);
            List<DomainNode> dList = new List<DomainNode>();
            foreach(Clash c in clashes)
            {
                DomainNode dNode = new DomainNode(c.CrsID, c.secID, cHash.getSecAls(c.CrsID, c.secID));
                dList.Add(dNode);
            }
            //List<Clash> list = new List<Clash>(clashes.ToArray());
            return dList;            
        }

        public int averageClashCount()
        {
            int total = 0;
            foreach (ClashRow cR in Set)
            {
                total += cR.count;
            }

            return total / Set.Length;
        }
        public List<DomainNode> getMaxClashHeads()
        {
            var clashes = from element in Set
                          orderby element.count descending
                          select element.head;

            CourseHash cHash = new CourseHash(1);
            List<DomainNode> dList = new List<DomainNode>();
            foreach (Clash c in clashes)
            {
                DomainNode dNode = new DomainNode(c.CrsID, c.secID, cHash.getSecAls(c.CrsID, c.secID));
                dList.Add(dNode);
            }
            //List<Clash> list = new List<Clash>(clashes.ToArray());
            return dList;
        }
        public List<Clash> getClashes(String crsID, String secID)
        {
            Clash temp = new Clash();
            List<Clash> list = new List<Clash>();
            for (int i = 0; i < Size; i++)
            {
                temp = Set[i].head;
                if ((crsID == Set[i].head.CrsID) && (secID == Set[i].head.secID))
                {
                    temp = temp.next;
                    while (temp != null)
                    {
                        list.Add(temp);
                        temp = temp.next;
                    }
                    return list;
                }

                else continue;
            }
            return list;
        }

        public void InsertClash(Clash listC, Section list, Clash treeC, Section tree)
        {//compares 2 sections. list is from the ClashSet and tree is to be compared
            if (list.getIns() == tree.getIns())
            {// if an Instructor clash is found
                Clash node = new Clash();
                node.InsName = tree.getIns();
                node.type = "Instructor";
                node.stuList = null;
                node.secID = tree.getID();
                node.CrsID = treeC.CrsID;
                node.CrsName = treeC.CrsName;
                node.next = null;

                int i = 0;
                while (listC != Set[i].head)
                    i++;

                Clash temp = Set[i].head;
                Set[i].count++;
                while (temp.next != null)
                    temp = temp.next;

                temp.next = node;
            }

            else
            {
                List<Int32> clashStu = new List<Int32>();
                clashStu = getStuClash(list.getSecStu(), tree.getStuRoot());
                if (clashStu.Count > 0)
                {
                    Clash node = new Clash();
                    node.InsName = tree.getIns();
                    node.type = "Section";
                    node.stuList = null;
                    node.secID = tree.getID();
                    node.CrsID = treeC.CrsID;
                    node.CrsName = treeC.CrsName;
                    node.next = null;
                    if (clashStu.Count < 15)
                    {
                        node.type = "Student";
                        node.stuList = clashStu;
                    }

                    int i = 0;
                    while (listC != Set[i].head)
                        i++;

                    Clash temp = Set[i].head;
                    Set[i].count++;
                    while (temp.next != null)
                        temp = temp.next;

                    temp.next = node;
                }
            }
        }

        protected List<Int32> getStuClash(List<Student> list, StudentTree tree)
        {
            List<Int32> clashStu = new List<Int32>();
            int count = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (tree.detectStudent(list[i].getSID()) == true)
                {
                    clashStu.Add(list[i].getSID());
                    count++;
                }

                if (count > 15)
                    break;
            }
            return clashStu;
        }

        public List<Int32> getClashStuList(String headCrs, String headSec, String CrsID, String SecID)
        {
            Clash temp = Set[0].head;
            for (int i = 0; i < Size; i++)
            {
                temp = Set[i].head;
                if ((temp.CrsID == headCrs) && (temp.secID == headSec))
                    break;
            }

            while (temp != null)
            {
                if ((temp.CrsID == CrsID) && (temp.secID == SecID))
                    break;
                temp = temp.next;
            }

            return temp.stuList;
        }
        public Int32 getClashCount(String crsID, String SecID)
        {
            for (int i = 0; i < Size; i++)
            {
                if (Set[i].head.CrsID == crsID)
                    if (Set[i].head.secID == SecID)
                        return Set[i].count;

                
                    
            }
            return 0;
        }
    }
}
