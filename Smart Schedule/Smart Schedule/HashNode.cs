using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Smart_Schedule
{
    public class HashNode<T>
    {
        public T obj;
        public HashNode<T> next;
        public string crsID;
        public string secID;
        public float remClass;
        public HashNode(string c, string s, float rC, T newObj)
        {
            crsID = c;
            secID = s;
            obj = newObj;
            next = null;
            remClass = rC;
        }

    }

    public class HashList<T>
    {
        public HashNode<T> head;

        public HashList()
        {
            head = null;
        }

        public void Add(HashNode<T> node)
        {
            if (head == null)
            {
                head = node;
            }

            else
            {
                HashNode<T> temp = head;
                while (temp.next != null)
                    temp = temp.next;

                temp.next = node;
            }
        }

        public HashNode<T> getItem(string cID, string sID)
        {
            HashNode<T> temp = head;
            while (temp != null)
            {
                if ((temp.crsID == cID) && (temp.secID == sID))
                    return temp;
                temp = temp.next;
            }
            return null;
        }

        public void chkAllInsert(ref List<string[]> list)
        {
            //List<string[]> list = new List<string[]>();
            HashNode<T> temp = head;
            while (temp != null)
            {
                if (temp.remClass != 0)
                {
                    string[] s = new string[3];
                    s[0] = temp.crsID;
                    s[1] = temp.secID;
                    s[2] = temp.remClass.ToString();
                    list.Add(s);
                }
                temp = temp.next;
            }
            //return list;
        }
    }

    public class HashTable<T>
    {
        //T obj;
        HashList<T>[] table = new HashList<T>[10];
        public static int count = 0;
        public HashTable()
        {
            for (int i = 0; i < 10; i++)
            {
                table[i] = new HashList<T>();
            }
        }

        public List<DomainNode> getAscDomain()
        {
            var list = from element in table
                       orderby element.head.remClass ascending
                       select element.head;

            CourseHash cHash = new CourseHash(1);
            List<DomainNode> dList = new List<DomainNode>();
            foreach (HashNode<T> hN in list)
            {
                DomainNode dN = new DomainNode(hN.crsID, hN.secID, cHash.getSecAls(hN.crsID, hN.secID));
                dList.Add(dN);
            }
            return dList;
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

        public List<string[]> chkAllInsert()
        {
            List<string[]> list = new List<string[]>();
            for (int i = 0; i < 10; i++)
            {
                table[i].chkAllInsert(ref list);
            }
            return list;
        }

        public void insertEntry(String cID, HashNode<T> node)
        {//to insert in the domain of the slot
            table[getHashVal(cID)].Add(node);
            count++;
            //count++;
        }

        public HashNode<T> getItem(string cID, string sID)
        {
            return table[getHashVal(cID)].getItem(cID, sID);
        }
    }

    public class trackNode
    {
        public int CperWeek;
        public float CperDay;

        public trackNode(int cpw, float cpd)
        {
            CperWeek = cpw;
            CperDay = cpd;
        }

        public void setCPD(float val)
        {
            CperDay = val;
        }
    }
}
