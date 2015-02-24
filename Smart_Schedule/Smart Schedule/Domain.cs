using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Smart_Schedule
{
    public class DomainNode
    {
        public String clsID;
        public String secID;
        public String Alias;

        //public static int count = 0;
        public DomainNode domNext;

        public DomainNode()
        {
            clsID = secID = Alias = null;
            domNext = null;
            //++count;
        }

        public DomainNode(String cID, String sID, String als)
        {
            clsID = cID;
            secID = sID;
            Alias = als;
            domNext = null;
            //++count;
        }

        public DomainNode Clone()
        {
            DomainNode dN = new DomainNode(this.clsID, this.secID, this.Alias);
            dN.domNext = null;
            return dN;
        }
    }

    class DomainList
    {
        private DomainNode head;

        public DomainList()
        {
            head = null;
        }

        public bool Add(string cID, string sID, string als)
        {
            if (head == null)
            {
                DomainNode temp = new DomainNode(cID, sID, als);
                head = temp;
                return true;
            }

            else
            {
                DomainNode temp = head;
                while (temp.domNext != null)
                {
                    if ((temp.clsID == cID) && (temp.secID == sID))
                        return false;
                    temp = temp.domNext;
                }

                DomainNode newNode = new DomainNode(cID, sID, als);
                temp.domNext = newNode;
                return true;
            }
        }

        public bool Remove(string cID, string sID)
        {
            bool deleted = false;
            if (head != null)
            {
                if ((head.clsID == cID) && (head.secID == sID))
                {
                    DomainNode temp = head;
                    head = head.domNext;
                    temp.domNext = null;
                    deleted = true;
                }

                else
                {
                    DomainNode prev = head;
                    DomainNode temp = head.domNext;
                    while (temp != null)
                    {
                        if ((temp.clsID == cID) && (temp.secID == sID))
                        {
                            prev.domNext = temp.domNext;
                            temp = null;
                            deleted = true;
                            break;
                        }
                        temp = temp.domNext;
                        prev = prev.domNext;
                    }
                }
            }
            return deleted;
        }

        public void addToList(ref List<DomainNode> list)
        {// to add items in list to be returned for making new slot
            DomainNode temp = head;
            while (temp != null)
            {
                list.Add(temp.Clone());
                temp = temp.domNext;
            }
        }

        public bool isCrsPresent(string cID, string sID)
        {// to see if an item is present in domain
            DomainNode temp = head;
            while (temp != null)
            {
                if ((temp.clsID == cID) && (temp.secID == sID))
                    return true;

                temp = temp.domNext;
            }
            return false;
        }
    }
    public class Domain
    {
        //private int rooms;
        private DomainList[] domTable = new DomainList[10];
        public int count;
        public Domain()
        {
            for (int i = 0; i < 10; i++)
            {
                domTable[i] = new DomainList();
            }
            count = 0;
        }

        public Domain Clone()
        {
            Domain dom = new Domain();
            return dom;
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

        public void insertEntry(String cID, String sID, String als)
        {//to insert in the domain of the slot
            if (domTable[getHashVal(cID)].Add(cID, sID, als) == true)
                count++;
        }

        public void removeCourse(String cID, String sID)
        {
            if (domTable[getHashVal(cID)].Remove(cID, sID) == true)
                count--;
        }

        public List<DomainNode> getDomList()
        {
            List<DomainNode> list = new List<DomainNode>();
            for (int i = 0; i < 10; i++)
            {
                domTable[i].addToList(ref list);
            }
            return list;
        }

        public bool isCrsPresent(string c, string s)
        {
            return domTable[getHashVal(c)].isCrsPresent(c, s);
        }

        public int getCount()
        {
            return count;
        }

    }
}
