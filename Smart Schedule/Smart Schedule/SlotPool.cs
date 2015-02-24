using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smart_Schedule
{
    public class SlotList
    {
        private Slot head;
        private int count;

        public SlotList()
        {
            head = null;
            count = 0;
        }

        public Slot getHead() { return head; }
        public bool Insert(Slot slt)
        {// inserting in the Slot Pool
            if (head == null)
            {
                head = slt;
                return true;
            }

            else
            {
                Slot temp = head;
                while (temp.next != null)
                {
                    if (temp.Compare(slt) == true)      // duplicate slots will not be inserted
                        return false;

                    temp = temp.next;
                }

                if (temp.Compare(slt) == true)      // checking the last slot for duplication
                    return false;
                else
                    temp.next = slt;                // if no duplicate slots are found, the the slot is inserted
            }
            count++;
            return true;
        }

        public bool chekDuplicate(Slot slt)
        {
            Slot temp = head;
            while (temp != null)
            {
                if (temp.Compare(slt) == true)      // duplicate slots will not be inserted
                    return true;

                temp = temp.next;
            }

            return false;
        }
    }

    public class SlotTable
    {
        private SlotList[] list;
        private int count;
        public SlotTable(int labs)
        {
            list = new SlotList[labs];
            for (int i = 0; i < labs; i++)
                list[i] = new SlotList();

            count = 0;
        }

        public bool Insert(Slot slt)
        {
            bool res = list[slt.getLabLeft()].Insert(slt);
            if (res == true) count++;

            return res;
        }

        public Slot getHead(int lbLeft) { return list[lbLeft].getHead(); }

        public bool checkDuplicate(Slot slt)
        {
            return list[slt.getLabLeft()].chekDuplicate(slt);
        }
    }

    public class SlotPool
    {
        private SlotTable[] table;
        private int count;
        private int roomNum;

        public SlotPool(int rooms, int labs)
        {
            /*
            head = new SlotList[20];
            for (int i = 0; i < 20; i++)
                head[i] = new SlotList();*/
            table = new SlotTable[rooms+1];
            for (int i = 0; i < rooms; i++)
                table[i] = new SlotTable(labs+1);
            count = 0;
            roomNum = rooms;
        }

        public int Count()
        {
            return count;
        }

        public Slot getHead(int rmLeft, int lbLeft) { return table[rmLeft].getHead(lbLeft); }

        public bool Insert(Slot slt)
        {// inserting in the Slot Pool
            if (slt.getRoomLeft() == roomNum)
                return false;
            bool chk = table[slt.getRoomLeft()].Insert(slt);
            if (chk == true)
                count++;

            return chk;
        }

        public bool checkDuplicate(Slot slt)
        {
            if (slt.getRoomLeft() == roomNum)
                return false;
            return table[slt.getRoomLeft()].checkDuplicate(slt);
        }
    }
}
