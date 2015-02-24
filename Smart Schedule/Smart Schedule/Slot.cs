using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Smart_Schedule
{
    public class Room
    {
        //private String rmName;
        private String CrsName;
        private String SecID;
        private String alias;
        private String LinkSlot;
        private int LinkRoom;

        //private static int rmNum = 0;
        //private static int lbNum = 0;
        public Room()
        {
            //rmName = rm;
            //CrsName = SecID = alias = "";
        }

        public Room(String crs, String sec, String als)
        {
            //rmName = rm;
            CrsName = crs;
            SecID = sec;
            alias = als;
            LinkSlot = null;
            LinkRoom = -1;
        }

        public string getRoomCrs()
        {
            return CrsName + " " + SecID;
        }

        public void setRoomCrs(string crsID, string secID, string als)
        {
            CrsName = crsID;
            SecID = secID;
            alias = als;
        }

        public string getAls()
        { return alias; }
        public string getCrs()
        { return CrsName; }
        public string getSec()
        { return SecID; }

        public void setLink(string slt, int rm)
        {
            LinkSlot = slt;
            LinkRoom = rm;
        }

        public bool isLink()
        {
            if ((LinkSlot != null) && (LinkRoom != -1))// if the room has a link, true is returned
                return true;

            else
                return false;
        }

    }

    public class Slot
    {
        //private String ID;
        private DateTime startTime;         // start time of a slot
        private DateTime endTime;           // end time of a slot
        public bool breakTime = false;             // to check whether the slot is a recess
        public Domain domain = new Domain();        // the domain for the slot consisting of all courses and their sections
        public Domain stuClashDom = new Domain();   // to record student clashes of a slot

        public Room[] rooms;                // array for all rooms and labs of a slot
        public Room[] labs;
        private int roomLeft;               // to keep track of rooms left
        private int labLeft;                // to keep track of labs left

        private int roomN;
        private int labN;
        //public List<Course> Domain;
        public Slot next;
        public bool MaxD = false;
        public bool MinD = true;
        List<Batch> priority = new List<Batch>();

        public List<Room> getClashRoom(string cID, string sID)
        {
            List<Room> clashingRooms = new List<Room>();
            CourseHash cH = new CourseHash(1);
            List<Clash> origClash = cH.getClashList(cID, sID);
            foreach (Room r in rooms)
            {
                if (r == null)
                    continue;
                foreach (Clash c in origClash)
                {
                    if ((r.getCrs() == c.CrsID) && (r.getSec() == c.secID))
                        clashingRooms.Add(new Room(c.CrsID, c.secID, cH.getSecAls(c.CrsID, c.secID)));
                }
            }

            foreach (Room l in labs)
            {
                if (l == null)
                    continue;
                foreach (Clash c in origClash)
                {
                    if ((l.getCrs() == c.CrsID) && (l.getSec() == c.secID))
                        clashingRooms.Add(new Room(c.CrsID, c.secID, cH.getSecAls(c.CrsID, c.secID)));
                }
            }
            return clashingRooms;
        }

        public List<Slot> bestOptions;
        public Slot()
        { }
        public Slot(int rmNum, int lbNum)
        {
            rooms = new Room[rmNum];
            roomLeft = roomN = rmNum;
            labs = new Room[lbNum];
            labLeft = labN = lbNum;
        }

        public void ReInitializeRoom()
        {
            rooms = new Room[roomN];
            roomLeft = roomN;
        }

        public void ReInitializeLab()
        {
            labs = new Room[labN];
            labLeft = labN;
        }
        public int getRoomCount() { return roomN; }
        public int getLabCount() { return labN; }

        public void setMax() { MaxD = true; MinD = false; }
        public void setMin() { MaxD = false; MinD = true; }
        public void setBatchPriority(List<Batch> bList)
        {
            priority = new List<Batch>();
            foreach (Batch b in bList)
                priority.Add(b);
        }
        public List<Batch> getBatchPriority() { return priority; }
        public Slot(int rmNum, int lbNum, DateTime s, DateTime e, List<DomainNode> dom, List<Batch> bList)
        {
            //ID = "S" + id;
            startTime = s;
            endTime = e;
            rooms = new Room[rmNum];
            roomLeft = roomN = rmNum;
            labs = new Room[lbNum];
            labLeft = labN = lbNum;
            foreach (DomainNode dn in dom)
            {
                domain.insertEntry(dn.clsID, dn.secID, dn.Alias);
                stuClashDom.insertEntry(dn.clsID, dn.secID, dn.Alias);
            }
            priority = new List<Batch>();
            foreach (Batch b in bList)
                priority.Add(b);
        }

        public Slot(int rmNum, int lbNum, DateTime s, DateTime e, bool brk)
        {
            startTime = s;
            endTime = e;
            rooms = new Room[rmNum];
            labs = new Room[lbNum];
            labLeft = labN = lbNum;
            breakTime = brk;
            roomLeft = roomN = rmNum;
        }

        public void Reset( List<DomainNode> dom)
        {
            rooms = new Room[roomN];
            labs = new Room[labN];
            roomLeft = roomN;
            labLeft = labN;
            domain = new Domain();
            stuClashDom = new Domain();
            foreach (DomainNode dN in dom)
            {
                domain.insertEntry(dN.clsID, dN.secID, dN.Alias);
                //stuClashDom.insertEntry(dN.clsID, dN.secID, dN.Alias);
            }
        }

        public void setRoomLeft(int rL)
        {
            roomLeft = rL;
        }

        public void setLabLeft(int lL)
        {
            labLeft = lL;
        }
        public List<Slot> getBestSlot(bool insertLab)
        {
            Slot tempSlot = this.Clone();
            //tempSlot.setDom(dList);
            if (insertLab == false)
                foreach (DomainNode dN in tempSlot.domain.getDomList())
                    if (dN.Alias.Contains("Lab"))
                        tempSlot.domain.removeCourse(dN.clsID, dN.secID);

            List<Slot> tempArray = new List<Slot>();
            tempArray.Add(tempSlot);
            while ((tempArray.ElementAt(0).domain.count > 0) && ((tempArray.ElementAt(0).getRoomLeft() > 0) || (tempArray.ElementAt(0).getLabLeft() > 0)))
            {
                List<Slot> returnList = new List<Slot>();

                returnList = calculateBest(tempArray);
                if (tempArray.Count == returnList.Count)
                {
                    bool situation = true;
                    for (int i = 0; i < tempArray.Count; i++)
                    {
                        if (tempArray.ElementAt(i).Compare(returnList.ElementAt(i)) == true)
                            continue;

                        else
                        {
                            situation = false;
                            break;
                        }
                    }
                    if (situation == true)
                        return tempArray;
                }
                tempArray = new List<Slot>();
                tempArray = returnList;
            }
            return tempArray;
        }

        protected List<Slot> calculateBest(List<Slot> tempArray)
        {
            List<Slot> bestSlots = new List<Slot>();
            foreach (Slot slt in tempArray)
            {
                List<Slot> best = new List<Slot>();
                for (int i = 0; i < slt.domain.count; i++)
                {
                    Slot temp = slt.Clone();
                    if (temp.domain.count > 0)
                    {
                        DomainNode dN = slt.domain.getDomList().ElementAt(i);
                        if (temp.addCrs(dN.clsID, dN.secID, dN.Alias) == false)
                            continue;
                    }

                    if (best.Count > 0)
                    {
                        if (best.ElementAt(0).domain.count == temp.domain.count)
                            best.Add(temp);

                        else if (best.ElementAt(0).domain.count < temp.domain.count)
                        {
                            best = new List<Slot>();
                            best.Add(temp);
                        }
                    }

                    else
                        best.Add(temp);
                }
                if (bestSlots.Count == 0)
                    bestSlots.AddRange(best);

                else if (best.Count > 0)
                {
                    if (bestSlots.ElementAt(0).domain.count == best.ElementAt(0).domain.count)
                        bestSlots.AddRange(best);

                    else if (bestSlots.ElementAt(0).domain.count < best.ElementAt(0).domain.count)
                    {
                        bestSlots = new List<Slot>();
                        bestSlots.AddRange(best);
                    }
                }
            }

            if (bestSlots.Count != 0)
            {
                tempArray = new List<Slot>();
                tempArray = bestSlots;
            }
            return tempArray;
        }

        public bool Compare(Slot slt)
        {// will return true if slots match
            if (slt.roomLeft != this.roomLeft)
                return false;

            if (slt.labLeft != this.labLeft)
                return false;
            foreach (Room r in this.rooms)
            {
                if (r == null) continue;
                if (slt.Contains(r.getCrs(), r.getSec()) == false)
                    return false;
            }

            foreach (Room l in this.labs)
            {
                if (l == null) continue;
                if (slt.Contains(l.getCrs(), l.getSec()) == false)
                    return false;
            }
            return true;
        }

        public bool Contains(string cID, string sID)
        {// will return true if the course is in the slot
            bool cont = false;
            foreach (Room r in rooms)
            {
                if (r == null)
                    continue;
                if ((r.getCrs() == cID) && (r.getSec() == sID))
                    return true;
            }

            foreach (Room l in labs)
            {
                if (l == null)
                    continue;
                if ((l.getCrs() == cID) && (l.getSec() == sID))
                    return true;
            }
            return cont;
        }
        public Slot Clone()
        {
            Slot slt = new Slot();
            slt.MinD = this.MinD;
            slt.MaxD = this.MaxD;
            slt.breakTime = this.breakTime;
            slt.endTime = this.endTime;
            slt.labLeft = this.labLeft;
            slt.next = null;
            slt.roomLeft = this.roomLeft;
            slt.labLeft = this.labLeft;
            slt.startTime = this.startTime;
            if (slt.breakTime == true)
                slt.rooms = null;
           
            slt.rooms = new Room[this.rooms.Length];
            slt.roomN = this.roomN;
            slt.labN = this.labN;
            for (int i = 0; i < rooms.Length; i++)
            {
                if (this.rooms[i] != null)
                    slt.rooms[i] = new Room(this.rooms[i].getCrs(), this.rooms[i].getSec(), this.rooms[i].getAls());
            }


            slt.labs = new Room[this.labs.Length];
            for (int j = 0; j < labs.Length; j++)
            {
                if (this.labs[j] != null)
                    slt.labs[j] = new Room(this.labs[j].getCrs(), this.labs[j].getSec(), this.labs[j].getAls());
            }

            List<DomainNode> dList = this.domain.getDomList();
            foreach (DomainNode dN in dList)
            {
                slt.domain.insertEntry(dN.clsID, dN.secID, dN.Alias);
            }
            foreach (DomainNode dNS in this.stuClashDom.getDomList())
                slt.stuClashDom.insertEntry(dNS.clsID, dNS.secID, dNS.Alias);

            foreach (Batch btc in this.priority)
                slt.priority.Add(btc);
            return slt;
        }

        public DateTime getStartTime()
        {
            return startTime;
        }

        public DateTime getEndTime()
        {
            return endTime;
        }

        public Slot(Slot s)
        {
            this.startTime = s.startTime;
            this.endTime = s.endTime;
            this.breakTime = s.breakTime;
            this.domain = s.domain;
            this.rooms = s.rooms;
            this.roomLeft = s.roomLeft;
            this.labLeft = s.labLeft;
            this.rooms = s.rooms;
            this.next = s.next;
            this.roomN = s.roomN;
            this.labN = s.labN;
        }

        public void swapRoom(int source, int destinatioin)
        {
            Room r = new Room();
            r = rooms[source];
            rooms[source] = rooms[destinatioin];
            rooms[destinatioin] = r;
        }

        public string getHeader()
        {
            return startTime.Hour.ToString() + ":" + startTime.Minute.ToString() + " - " + endTime.Hour.ToString() + ":" + endTime.Minute.ToString();
        }

        public List<DomainNode> getSortedBatch()
        {////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            CourseHash cHash = new CourseHash(1);
            List<DomainNode> retList = new List<DomainNode>();
            foreach (Batch bt in priority)
            {
                var list = from element in this.domain.getDomList()
                           where cHash.getBatch(element.clsID).Compare(bt) == true
                           select element;

                var dmList = list;
                if (MaxD == true)
                {
                    dmList = from element in list
                             orderby cHash.getClashCount(element.clsID, element.secID) descending
                             select element;
                }

                else
                {
                    dmList = from element in list
                             orderby cHash.getClashCount(element.clsID, element.secID) ascending
                             select element;
                }
                foreach (DomainNode dN in dmList)
                {
                    retList.Add(dN);
                }
            }
            return retList;
        }

        public void setDom(List<DomainNode> dom)
        {
            foreach (DomainNode dn in dom)
                domain.insertEntry(dn.clsID, dn.secID, dn.Alias);
        }

        public List<DomainNode> getDom()
        {
            return domain.getDomList();
        }

        public List<DomainNode> getStuDom()
        {
            return stuClashDom.getDomList();
        }
        public void RemoveCrs(string cID, string sID)
        {
            //domain.IndexOf();
            if (cID == "")
                return;
            domain.removeCourse(cID, sID);
            stuClashDom.removeCourse(cID, sID);
        }

        public void addRoomCrs(int i, Room insR, float factor)
        {
            if (rooms[i] != null)
                i++;
            rooms[i] = insR;
            Timetable temp = new Timetable();
            temp.getCheckItem(insR.getCrs(), insR.getSec()).remClass-=factor;
            roomLeft--;
        }

        public bool addCrs(string cID, string sID, string als)
        {
            if (this.domain.isCrsPresent(cID, sID) == false)
                return false;
            if (als.Contains("Lab") == true)
                return addLabCrs(cID, sID, als);

            else
                return addRoomCrs(cID, sID, als);
        }
        public bool addRoomCrs(string cID, string sID, string als)
        {
            if (roomLeft == 0)
                return false;

            else
            {
                foreach (Room rChk in this.rooms)
                {
                    if (rChk == null)
                        continue;
                    if((rChk.getCrs() == cID)&&(rChk.getSec() == sID))
                        return false;
                }
                CourseHash cH = new CourseHash(1);
                for (int i = 0; i < rooms.Length; i++)
                {
                    if (rooms[i] == null)
                    {
                        rooms[i] = new Room(cID, sID, als);
                        List<Clash> clashes = cH.getClashList(cID, sID);
                        this.domain.removeCourse(cID, sID);
                        this.stuClashDom.removeCourse(cID, sID);
                        foreach (Clash c in clashes)
                        {
                            if (c.type == "Student")
                            {
                                this.domain.removeCourse(c.CrsID, c.secID);
                                continue;
                            }
                            this.stuClashDom.removeCourse(c.CrsID, c.secID);
                            this.domain.removeCourse(c.CrsID, c.secID);
                        }
                        roomLeft--;
                        return true;
                    }
                }
            }
            return false;
        }
        public void addLabCrs(int i, Room insR, float factor)
        {
            while (labs[i] != null)
                i++;
            labs[i] = insR;
            Timetable temp = new Timetable();
            temp.getCheckItem(insR.getCrs(), insR.getSec()).remClass -= factor;
            labLeft--;
        }

        public bool addLabCrs(string cid, string sid, string als)
        {
            if (labLeft == 0)
                return false;

            if (this.domain.isCrsPresent(cid, sid) == false)
                return false;
            else
            {
                foreach (Room lbChk in this.labs)
                {
                    if (lbChk == null)
                        continue;
                    if ((lbChk.getCrs() == cid) && (lbChk.getSec() == sid))
                        return false;
                }
                CourseHash cH = new CourseHash(1);
                for (int i = 0; i < labs.Length; i++)
                {
                    if (labs[i] == null)
                    {
                        labs[i] = new Room(cid, sid, als);
                        List<Clash> clashes = cH.getClashList(cid, sid);
                        this.domain.removeCourse(cid, sid);
                        this.stuClashDom.removeCourse(cid, sid);
                        foreach (Clash c in clashes)
                        {
                            if (c.type == "Student")
                            {
                                if (stuClashDom.isCrsPresent(c.CrsID, c.secID) == false)
                                    if (domain.isCrsPresent(c.CrsID, c.secID) == true)
                                        stuClashDom.insertEntry(c.CrsID, c.secID, cH.getSecAls(c.CrsID, c.secID));
                                this.domain.removeCourse(c.CrsID, c.secID);
                                continue;
                            }
                            this.stuClashDom.removeCourse(c.CrsID, c.secID);
                            this.domain.removeCourse(c.CrsID, c.secID);
                        }
                        labLeft--;
                        return true;
                    }
                }
            }
            return false;
        }
        public void remRoomCrs(int i)
        {
            // getting the info of the selected room
            string cID = rooms[i].getCrs();
            string sID = rooms[i].getSec();
            string als = rooms[i].getAls();

            rooms[i] = null;                              // room has been set to null
            updDomAfterIns(cID, sID, als);

            roomLeft++;
        }

        public void updDomAfterIns(string cID, string sID, string als)
        {
            domain.insertEntry(cID, sID, als);                  // since the course will be removed from room, it should be placed back in the domain
            ClashSet s = new ClashSet();                        // the clashing courses were removed from domain, so we need them back in the domain
            List<Clash> clashList = s.getClashes(cID, sID);     // getting the clashing courses of the selected course
            CourseHash cl = new CourseHash(1);
            Timetable t = new Timetable();
            t.getCheckItem(cID, sID).remClass++;
            foreach (Clash c in clashList)
            {
                if (t.getCheckItem(c.CrsID, c.secID).remClass == 0)
                    continue;
                domain.insertEntry(c.CrsID, c.secID, cl.getSecAls(c.CrsID, c.secID));       // adding the clashing courses back in the domain
            }
            updSltAfterRem();

        }

        private void updSltAfterRem()
        {
            foreach (Room r in rooms)
            {// since all the clashing courses of the course has been added in the domain, there might be some courses that are causing a clash
                // with the already inserted courses
                if (r != null)
                    RemoveCrs(r.getCrs(), r.getSec());      // checking rooms and deleting the clashing courses of already inserted courses
            }

            foreach (Room r in labs)
            {// since all the clashing courses of the course has been added in the domain, there might be some courses that are causing a clash
                // with the already inserted courses
                if (r != null)
                    RemoveCrs(r.getCrs(), r.getSec());      // checking rooms and deleting the clashing courses of already inserted courses
            }
        }

        public void remLabCrs(int i)
        {
            string cID = labs[i].getCrs();
            string sID = labs[i].getSec();
            string als = labs[i].getAls();

            domain.insertEntry(cID, sID, als);
            ClashSet s = new ClashSet();
            List<Clash> clashList = s.getClashes(cID, sID);     // getting the clashing courses of the selected course
            CourseList cl = new CourseList(1);
            foreach (Clash c in clashList)
            {
                domain.insertEntry(c.CrsID, c.secID, cl.getSecAls(c.CrsID, c.secID));       // adding the clashing courses back in the domain
            }

            labs[i] = null;

            foreach (Room r in rooms)
            {// since all the clashing courses of the course has been added in the domain, there might be some courses that are causing a clash
                // with the already inserted courses
                if (r != null)
                    RemoveCrs(r.getCrs(), r.getSec());      // checking rooms and deleting the clashing courses of already inserted courses
            }

            foreach (Room r in labs)
            {// since all the clashing courses of the course has been added in the domain, there might be some courses that are causing a clash
                // with the already inserted courses
                if (r != null)
                    RemoveCrs(r.getCrs(), r.getSec());      // checking rooms and deleting the clashing courses of already inserted courses
            }

            labLeft--;
        }
        public bool CSPresent(string cID, string sID)
        {
            foreach (Room r in rooms)
            {
                if ((r.getCrs() == cID) && (r.getSec() == sID))
                    return true;
            }

            foreach(Room r in labs)
                if ((r.getCrs() == cID) && (r.getSec() == sID))
                    return true;

            return false;
        }

        public string getRoomAls(string cID, string sID)
        {
            for (int i = 0; i < rooms.Length; i++)
            {
                if ((rooms[i].getCrs() == cID) && (rooms[i].getSec() == sID))
                    return rooms[i].getAls();
            }

            for (int k = 0; k < labs.Length; k++)
            {
                if ((labs[k].getCrs() == cID) && (labs[k].getSec() == sID))
                    return labs[k].getAls();
            }
            return "";
        }

        public void autoPopulate()
        {
        }
        public void autoPopulateRoom()
        {
            int i = 0;
            List<DomainNode> dN;
            CourseHash h = new CourseHash(1);
            while ((roomLeft > 0) && (domain.getCount() > 0))
            { 
                dN = this.domain.getDomList();
                DomainNode d;
                int k = 0;
                while (true)
                {
                    d = dN.ElementAt(k);
                    if (d == null)
                        return;
                    if (d.Alias.Contains("Lab"))
                        k++;

                    else
                        break;
                }
                this.addRoomCrs(i, new Room(d.clsID, d.secID, d.Alias), 1);
                List<Clash> crsClsh = h.getClashList(d.clsID,d.secID);
                foreach (Clash c in crsClsh)
                {
                    domain.removeCourse(c.CrsID, c.secID);
                }
                domain.removeCourse(d.clsID,d.secID);
                i++;
            }
        }

        public void autoPopulateLab()
        {
            int i = 0;
            List<DomainNode> dN;
            CourseHash h = new CourseHash(1);
            while ((labLeft > 0) && (domain.getCount() > 0))
            {
                dN = this.domain.getDomList();
                DomainNode d;
                int k = 0;
                while (true)
                {
                    d = dN.ElementAt(k);
                    if (d == null)
                        return;
                    if (!d.Alias.Contains("Lab"))
                        k++;

                    else
                        break;
                }
                this.addLabCrs(i, new Room(d.clsID, d.secID, d.Alias), 1);
                List<Clash> crsClsh = h.getClashList(d.clsID, d.secID);
                foreach (Clash c in crsClsh)
                {
                    domain.removeCourse(c.CrsID, c.secID);
                }
                domain.removeCourse(d.clsID, d.secID);
                i++;
            }
        }

        public int getRoomLeft()
        {
            return roomLeft;
        }

        public int getLabLeft()
        {
            return labLeft;
        }

        public String toString()
        {
            string s = "";
            foreach (Room r in rooms)
            {
                if (r == null) continue;

                s += r.getCrs() + "," + r.getSec() + "," + r.getAls() + "|";
            }

            s += "$";
            foreach (Room l in labs)
            {
                if (l == null) continue;

                s += l.getCrs() + "," + l.getSec() + "," + l.getAls() + "|";
            }

            return s;
        }

        public bool checkLabInsert()
        {
            Slot temp = this;
            int ind = 0;
            while (temp != null)
            {
                ind++;
                if (temp.next != null)
                {
                    TimeSpan breakTime = temp.next.startTime - temp.endTime;
                    if (breakTime.Minutes > 20)
                        return false;
                }
                temp = temp.next;
                if (ind > 2)
                    break;
            }

            if (ind < 3)
                return false;
            else
                return true;
        }

    }

    public class Day
    {
        private String dayName;
        public Slot head;
        public int slotCount = 0;
        public Day()
        {
            head = null;
        }

        public Day(string dayN)
        {
            head = null;
            dayName = dayN;
        }

        public String getDayName()
        { return dayName; }

        public void Reset(Domain dom)
        {
            Slot temp = head;
            while (temp != null)
            {
                temp.Reset(dom.getDomList());
                temp = temp.next;
            }
        }

        public void setBatchPriority(string slt, List<Batch> bList)
        {
            Slot temp = head;
            while (temp != null)
            {
                if (temp.getHeader() == slt)
                    temp.setBatchPriority(bList);
                temp = temp.next;
            }
        }
        public List<Batch> getBatchPriority(string slt)
        {
            Slot temp = head;
            while (temp != null)
            {
                if (temp.getHeader() == slt)
                    return temp.getBatchPriority();

                temp = temp.next;
            }
            return null;
        }

        public void addRoomCrs(string slt, int i, string val, HashTable<trackNode> trackN)
        {
            Slot temp = head;
            CourseHash h = new CourseHash(1);
            while (temp != null)
            {
                //string val = cmbSlot.SelectedItem.ToString();
                string[] als = val.Split('[', ']');
                string[] cid = als[2].Split(' ');

                if ((temp.getHeader() == slt) && (temp.getRoomLeft() > 0))
                {
                    if (trackN.getItem(cid[0], cid[1]).obj.CperDay > 1)
                    {
                        if ((temp.next != null) && (temp.domain.isCrsPresent(cid[0], cid[1])))
                        { }
                    }
                    if (als[1] != "")
                        temp.addRoomCrs(i, new Room(cid[0], cid[1], als[1]), 1);

                    else
                        temp.addRoomCrs(i, new Room(cid[0], cid[1], cid[0] + " " + cid[1]), 1);

                    List<Clash> crsClsh = h.getClashList(cid[0], cid[1]);
                    foreach (Clash c in crsClsh)
                    {
                        //int ind = temp.domain.Single(r => r.clsID == c.CrsID);
                        temp.domain.removeCourse(c.CrsID, c.secID);
                    }
                    //temp.RemoveCrs(
                }
                temp.domain.removeCourse(cid[0], cid[1]);

                temp = temp.next;
            }
        }

        public bool addSlot(Slot slt)
        {
            if (slt.breakTime == true)
            {
                //slt.rooms = null;
                //slt.setRoomLeft(0);

                List<DomainNode> dList = slt.domain.getDomList();
                foreach (DomainNode dN in dList)
                {
                    if(dN.Alias.Contains("Lab"))
                        continue;

                    else
                        slt.domain.removeCourse(dN.clsID, dN.secID);
                }
                TimeSpan bTime = slt.getEndTime() - slt.getStartTime();
                if (bTime.Minutes < 30)
                {
                    //slt.labs = null;
                    //slt.setLabLeft(0);
                    foreach(DomainNode dn in dList)
                        slt.domain.removeCourse(dn.clsID, dn.secID);
                }
            }
            if (head == null)
                head = slt;

            else
            {
                Slot temp = head;

                if (slt.getEndTime() < temp.getStartTime())
                {
                    slt.next = head;
                    head = slt;
                    return true;
                }
                Slot prev = head;
                temp = head.next;
                while (temp != null)
                {
                    if ((prev.getEndTime() <= slt.getStartTime()) && (slt.getEndTime() <= temp.getStartTime()))
                    {
                        slt.next = temp;
                        prev.next = slt;
                        slotCount++;
                        return true;
                    }

                    else if (slt.getStartTime() > prev.getEndTime())
                    {
                        prev = temp;
                        temp = temp.next;
                    }

                    else
                    {
                        MessageBox.Show("Invalid slot", "Error");
                        return false;
                    }
                }
                if (slt.getStartTime() >= prev.getEndTime())
                    prev.next = slt;

                else
                {
                    MessageBox.Show("Invalid Slot", "Error");
                    return false;
                }
            }
            slotCount++;
            return true;
        }

        public void removeRoomCrs(int i, string headText)
        {
            Slot temp = head;
            string c = null;
            string s = null;
            string a = null;
            while (temp!=null)
            {
                if (temp.getHeader() == headText)
                {
                    c = temp.rooms[i].getCrs();
                    s = temp.rooms[i].getSec();
                    a = temp.rooms[i].getAls();
                    temp.remRoomCrs(i);
                    Slot iterate = head;
                    while (iterate != null)
                    {
                        foreach (Room r in iterate.rooms)
                            if (r != null)
                                if (Timetable.trackList.getItem(r.getCrs(), r.getSec()).obj.CperDay == 1)
                                    temp.domain.removeCourse(r.getCrs(), r.getSec());

                        foreach (Room l in iterate.labs)
                            if (l != null)
                                if (Timetable.trackList.getItem(l.getCrs(), l.getSec()).obj.CperDay == 1)
                                    temp.domain.removeCourse(l.getCrs(), l.getSec());
                        iterate = iterate.next;
                    }
                }

                temp = temp.next;
            }

            // updating all other slots
            temp = head;
            while (temp != null)
            {
                if (temp.getHeader() != headText)
                    temp.updDomAfterIns(c, s, a);
                temp = temp.next;
            }
        }

        public void swapRoom(string headText, int source, int destination)
        {
            Slot temp = head;

            while (temp.getHeader() != headText)
                temp = temp.next;

            temp.swapRoom(source, destination);
        }

        public List<DomainNode> getDomain(string sltH)
        {
            Slot temp = head;
            while (temp.getHeader() != sltH)
                temp = temp.next;

            return temp.getDom();
        }

        public List<DomainNode> getStuClashDomain(string sltH)
        {
            Slot temp = head;
            while (temp.getHeader() != sltH)
                temp = temp.next;

            return temp.getStuDom();

        }
        public void removeCrsSlotAll(string cID, string sID)
        {
            Slot temp = head;
            while (temp != null)
            {
                temp.domain.removeCourse(cID, sID);
                temp.stuClashDom.removeCourse(cID, sID);
                temp = temp.next;
            }
        }

        public void RemoveSlot(string headTxt)
        {
            if (head.getHeader() == headTxt)
            {
                Slot temp = head;
                head = head.next;
                temp.next = null;
                slotCount--;
            }

            else
            {
                Slot temp = head.next;
                Slot prev = head;
                while (temp != null)
                {
                    if (temp.getHeader() == headTxt)
                    {
                        prev.next = temp.next;
                        temp.next = null;
                        slotCount--;
                        return;
                    }
                    prev = temp;
                    temp = temp.next;
                }
            }
        }

        public bool getMax(string slt)
        {
            Slot temp = head;
            while (temp != null)
            {
                if (temp.getHeader() == slt)
                    return temp.MaxD;
                temp = temp.next;
            }

            return false;
        }
        public void setMax(string slt)
        {
            Slot temp = head;
            while (temp != null)
            {
                if (temp.getHeader() == slt)
                    temp.setMax();
                temp = temp.next;
            }
        }
        public void setMin(string slt)
        {
            Slot temp = head;
            while (temp != null)
            {
                if (temp.getHeader() == slt)
                    temp.setMin();
                temp = temp.next;
            }
        }
        public void autoPopRoom(HashTable<trackNode> trackN, int rem, int opt)
        { }
        /// <summary>
        /// for populating a day
        /// </summary>
        /// <param name="trackN"></param>
        /// <param name="rem"></param>
        public void autoPop(HashTable<trackNode> trackN, int rem)
        {////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            Slot temp = head;
            CourseHash h = new CourseHash(1);
            Timetable t = new Timetable();
            List<Room> connRoom = new List<Room>();
            while (temp != null)
            {
                if ((temp.breakTime == true) && (temp.domain.count == 0))
                {
                    temp = temp.next;
                    continue;
                }
                foreach (Room inRl in connRoom)
                {
                    if (temp.addCrs(inRl.getCrs(), inRl.getSec(), inRl.getAls()) == true) { }
                        //t.getCheckItem(inRl.getCrs(), inRl.getSec()).remClass--;
                }

                connRoom = new List<Room>();
                var list = from element in temp.domain.getDomList()
                           orderby t.getCheckItem(element.clsID, element.secID).remClass descending
                           select element;

                List<DomainNode> sltDomList = new List<DomainNode>();
                foreach (DomainNode dnList in list)
                {
                    sltDomList.Add(dnList);
                }
                //List<DomainNode> newDom = temp.getSortedBatch();
                //temp.domain = new Domain();
                //temp.setDom(newDom);
                if ((sltDomList.Count == 0) && (temp.breakTime == false))
                    return;
                int chkL = 0;
                bool enable = true;
                if (temp.checkLabInsert() == false)
                {
                    while (sltDomList.ElementAt(chkL).Alias.Contains("Lab") == true)
                    {
                        chkL++;

                        if (chkL == sltDomList.Count)
                        {
                            enable = false;
                            goto outside;
                        }
                    }
                    enable = true;
                }
            outside:

                if ((enable == true) && (temp.domain.count > 0))
                    temp.addCrs(sltDomList.ElementAt(chkL).clsID, sltDomList.ElementAt(chkL).secID, sltDomList.ElementAt(chkL).Alias);
                Slot insert = temp.getBestSlot(temp.checkLabInsert()).ElementAt(0);

                //temp.ReInitializeRoom();
                foreach (Room r in insert.rooms)
                {
                    if (r == null)
                        continue;
                    //if(t.getCheckItem(r.getCrs(), r.getSec()).obj.CperDay == 1)
                    temp.addRoomCrs(r.getCrs(), r.getSec(), r.getAls());
                    t.getCheckItem(r.getCrs(), r.getSec()).remClass -= 1;
                    t.contProp(r.getCrs(), r.getSec());
                    if (t.getCheckItem(r.getCrs(), r.getSec()).obj.CperDay > 1)
                    {
                        if(connRoom.Contains(r) == false)
                            connRoom.Add(r);
                        continue;
                    }
                    Slot iterate = head;
                    while (iterate != null)
                    {
                        iterate.RemoveCrs(r.getCrs(), r.getSec());
                        iterate = iterate.next;
                    }
                }

                //temp.ReInitializeLab();
                foreach (Room l in insert.labs)
                {
                    if (l == null)
                        continue;

                    temp.addLabCrs(l.getCrs(), l.getSec(), l.getAls());
                    t.getCheckItem(l.getCrs(), l.getSec()).remClass -= 1;
                    t.contProp(l.getCrs(), l.getSec());
                    if (t.getCheckItem(l.getCrs(), l.getSec()).obj.CperDay > 1)
                    {
                        connRoom.Add(l);
                        continue;
                    }
                    Slot iterate = head;
                    while (iterate != null)
                    {
                        iterate.RemoveCrs(l.getCrs(), l.getSec());
                        iterate = iterate.next;
                    }
                }
                /*foreach (DomainNode dNClash in temp.stuClashDom.getDomList())
                {
                    temp.domain.insertEntry(dNClash.clsID, dNClash.secID, dNClash.Alias);
                }*/
                List<Room> tempCount = new List<Room>();
                foreach (Room tempr in connRoom)
                {
                    if (tempCount.Contains(tempr) == false)
                        tempCount.Add(tempr);
                }

                connRoom = new List<Room>();
                foreach (Room rl in tempCount)
                {
                    if (t.getCheckItem(rl.getCrs(), rl.getSec()).remClass > 0)
                        connRoom.Add(rl);
                }
                temp = temp.next;
            }
        }
        public void autoPopRoom(HashTable<trackNode> trackN, int rem)
        {
            Slot temp = head;
            CourseHash h = new CourseHash(1);
            List<DomainNode> dL = temp.getDom();
            Timetable t = new Timetable();
            List<DomainNode> contCourse = new List<DomainNode>();       // to record courses that have more than one hour of class per day
            while (temp != null)
            {
                int i = 0;
                dL = temp.getDom();
                while ((temp.domain.getCount() > 0) && (temp.getRoomLeft() > 0))
                {
                    //DomainNode dN = dL.ElementAt(0);
                    DomainNode dN;
                    int k = 0;
                    while (true)
                    {
                        try
                        {
                            dN = dL.ElementAt(k);
                        }
                        catch { goto outside; }
                        if (dN == null)
                            return;
                        if ((dN.Alias.Contains("Lab")) || (t.getCheckItem(dN.clsID, dN.secID).remClass < rem))
                            k++;

                        else
                            break;
                    }
                    if (trackN.getItem(dN.clsID,dN.secID).obj.CperDay > 1)
                    {
                        //if ((temp.next != null) && (temp.domain.isCrsPresent(dL = te)))
                        { }
                    }
                    if (dN.Alias != "")
                        temp.addRoomCrs(i, new Room(dN.clsID, dN.secID, dN.Alias), 1);

                    else
                        temp.addRoomCrs(i, new Room(dN.clsID, dN.secID, dN.clsID + " " + dN.secID), 1);

                    Slot iterate = head;
                    while (iterate != null)
                    {
                        iterate.RemoveCrs(dN.clsID, dN.secID);
                        iterate = iterate.next;
                    }
                    List<Clash> crsClsh = h.getClashList(dN.clsID, dN.secID);
                    foreach (Clash c in crsClsh)
                    {
                        //int ind = temp.domain.Single(r => r.clsID == c.CrsID);
                        temp.domain.removeCourse(c.CrsID, c.secID);
                    }
                    //temp.RemoveCrs(
                    i++;
                    temp.domain.removeCourse(dN.clsID, dN.secID);
                    //if (trackN.getItem(dN.clsID, dN.secID).remClass == 0)
                        t.contProp(dN.clsID, dN.secID);

                    dL = temp.getDom();
                }
                if (temp.domain.count != 0)
                {// in case rooms are filled and  
                }
            outside:
                temp = temp.next;
            }
        }

        public void autoPopLab(HashTable<trackNode> trackN, int rem)
        {
            Slot temp = head;
            CourseHash h = new CourseHash(1);
            List<DomainNode> dL = temp.getDom();
            Timetable t = new Timetable();
            while (temp != null)
            {
                int i = 0;
                dL = temp.getDom();
                while ((temp.domain.getCount() > 0) && (temp.getLabLeft() > 0))
                {
                    //DomainNode dN = dL.ElementAt(0);
                    DomainNode dN;
                    int k = 0;
                    while (true)
                    {
                        try
                        {
                            dN = dL.ElementAt(k);
                        }
                        catch { goto outside; }
                        if (dN == null)
                            return;
                        if ((!dN.Alias.Contains("Lab")) || (t.getCheckItem(dN.clsID, dN.secID).remClass < rem))
                            k++;

                        else
                            break;
                    }
                    if (trackN.getItem(dN.clsID, dN.secID).obj.CperDay > 1)
                    {
                        //if ((temp.next != null) && (temp.domain.isCrsPresent(dL = te)))
                        { }
                    }
                    if (dN.Alias != "")
                        temp.addLabCrs(i, new Room(dN.clsID, dN.secID, dN.Alias), 1);

                    else
                        temp.addLabCrs(i, new Room(dN.clsID, dN.secID, dN.clsID + " " + dN.secID), 1);

                    Slot iterate = head;/*
                    while (iterate != null)
                    {
                        iterate.RemoveCrs(dN.clsID, dN.secID);
                        iterate = iterate.next;
                    }*/
                    List<Clash> crsClsh = h.getClashList(dN.clsID, dN.secID);
                    foreach (Clash c in crsClsh)
                    {
                        //int ind = temp.domain.Single(r => r.clsID == c.CrsID);
                        temp.domain.removeCourse(c.CrsID, c.secID);
                    }
                    //temp.RemoveCrs(
                    i++;
                    temp.domain.removeCourse(dN.clsID, dN.secID);
                    //if (trackN.getItem(dN.clsID, dN.secID).remClass == 0)
                    t.contProp(dN.clsID, dN.secID);

                    dL = temp.getDom();
                }
            outside:
                temp = temp.next;
            }
        }

        public void reAllocateRooms()
        {
            Slot temp = head;
            while (temp != null)
            {
                if (temp.getRoomLeft() != 0)
                {
                    List<DomainNode> remCrs = temp.stuClashDom.getDomList();
                    if (remCrs.Count != 0)
                    {
                        for (int i = 0; i < remCrs.Count; i++)
                        {
                            List<Room> clashingRoom = temp.getClashRoom(remCrs.ElementAt(i).clsID, remCrs.ElementAt(i).secID);
                            if (clashingRoom.Count == 1)
                            {// if there is only one clashing course
                                Slot availableSlot = head;
                                while (availableSlot != null)
                                {// looking for available slot for the course in clashingRoom to be inserted
                                    if (availableSlot.getRoomLeft() > 0)
                                    {
                                        List<Room> crs = availableSlot.getClashRoom(clashingRoom.ElementAt(0).getCrs(), clashingRoom.ElementAt(0).getSec());
                                        if (crs.Count == 0)
                                        {//if the slot has no issues by inserting the course in clashingRoom course
                                            availableSlot.addCrs(clashingRoom.ElementAt(0).getCrs(), clashingRoom.ElementAt(0).getSec(), clashingRoom.ElementAt(0).getAls());

                                            // now adding the course from domain in the previous slot
                                            int ind = 0;
                                            Timetable t = new Timetable();
                                            foreach (Room r in temp.rooms)
                                            {
                                                if (r == null)
                                                {
                                                    ind++;
                                                    continue;
                                                }

                                                if ((r.getCrs() == clashingRoom.ElementAt(0).getCrs()) && (r.getSec() == clashingRoom.ElementAt(0).getSec()))
                                                {
                                                    temp.rooms[ind] = new Room(remCrs.ElementAt(i).clsID, remCrs.ElementAt(i).secID, remCrs.ElementAt(i).Alias);
                                                    temp.domain.removeCourse(remCrs.ElementAt(i).clsID, remCrs.ElementAt(i).secID);
                                                    t.getCheckItem(remCrs.ElementAt(i).clsID, remCrs.ElementAt(i).secID).remClass--;
                                                    t.contProp(remCrs.ElementAt(i).clsID, remCrs.ElementAt(i).secID);
                                                }
                                                ind++;
                                            }
                                        }
                                    }
                                    availableSlot = availableSlot.next;
                                }
                            }
                        }
                    }
                }

                temp = temp.next;
            }
        }
    }

    public class Timetable
    {
        private static Day[] ttDays;
        public static HashTable<trackNode> trackList;
        private static int dayNum;
        public SlotPool pool;
        public static string[] array;
        public List<String> slotList = new List<string>();
        private int threadCount;
        //public static System.IO.TextWriter file = new System.IO.StreamWriter(@"C:\WriteLines2.txt");
        private BackgroundWorker[] workers;
        private SlotPool chkPool;
        private int BreakPoint;
        private Slot[] best;
        public List<DomainNode> MaxDom;
        public List<DomainNode> MinDom;
        private int maxDomCount = 0;
        public Timetable()
        {
            
        }

        
        public Timetable(int i, HashTable<trackNode> trN, string editOpt, int rooms, int labs)
        {
            pool = new SlotPool(rooms, labs);
            ttDays = new Day[i];
            trackList = trN;
            dayNum = i;
            array = new string[CourseHash.clashSet.getHeads().Count];
            threadCount = 0;
            chkPool = new SlotPool(rooms, labs);
            MaxDom = CourseHash.clashSet.getMaxClashHeads();
            MinDom = CourseHash.clashSet.getMinClashHeads();
        }

        public Timetable(List<String> names, HashTable<trackNode> trN, int rooms, int labs)
        {
            pool = new SlotPool(rooms, labs);
            chkPool = new SlotPool(rooms, labs);
            ttDays = new Day[names.Count];
            trackList = trN;
            dayNum = names.Count;
            array = new string[CourseHash.clashSet.getHeads().Count];
            for (int i = 0; i < dayNum; i++)
            {
                ttDays[i] = new Day(names[i]);
            }
            for (int i = 0; i < CourseHash.clashSet.getHeads().Count; i++)
                array[i] = i.ToString();
            threadCount = 0;
            MaxDom = CourseHash.clashSet.getMaxClashHeads();
            MinDom = CourseHash.clashSet.getMinClashHeads();
        }

        public int getDayCount() { return dayNum; }
        public void addDaySlotAll(Slot slt)
        {
            for (int i = 0; i < dayNum; i++)
            {
                if(ttDays[i].addSlot(slt.Clone()) == false)
                    return;
            }
        }
        public void addDaySlot(string dayN, Slot slt)
        {
            for (int i = 0; i < dayNum; i++)
            {
                if (ttDays[i].getDayName() == dayN)
                {
                    ttDays[i].addSlot(slt.Clone());
                    return;
                }
            }
        }
        public HashNode<trackNode> getCheckItem(string crs, string sec)
        {
            return trackList.getItem(crs, sec);
        }

        
        public void swapRoom(string day, string slt, int source, int dest)
        {
            for (int i = 0; i < dayNum; i++)
            {
                if (ttDays[i].getDayName() == day)
                {
                    ttDays[i].swapRoom(slt, source, dest);
                    return;
                }
            }
        }

        public List<String> getDayNames()
        {
            List<String> names = new List<string>();
            for (int i = 0; i < dayNum; i++)
            {
                names.Add(ttDays[i].getDayName());
            }
            return names;
        }

        public void addRoomCrs(string day, string slt, int i, string val)
        {
            for (int j = 0; j < dayNum; j++)
            {
                if (ttDays[j].getDayName() == day)
                {
                    ttDays[j].addRoomCrs(slt, i, val, trackList);
                    break;
                }
            }

            string[] als = val.Split('[', ']');
            string[] cid = als[2].Split(' ');
            contProp(cid[0],cid[1]);
        }

        public void contProp(string cid, string sid)
        {
            if (trackList.getItem(cid,sid).remClass == 0)
            {
                for (int k = 0; k < dayNum; k++)
                    ttDays[k].removeCrsSlotAll(cid,sid);
            }
        }

        public void remRoomCrs(string dayN, int i, string headText)
        {
            for (int j = 0; j < dayNum; j++)
            {
                if (ttDays[j].getDayName() == dayN)
                {
                    ttDays[j].removeRoomCrs(i, headText);

                    return;
                }
            }
        }

        public Slot getDayHead(string dayN)
        {
            for (int j = 0; j < dayNum; j++)
            {
                if (ttDays[j].getDayName() == dayN)
                {
                    return ttDays[j].head;
                }
            }
            return null;
        }

        public List<DomainNode> getSlotDom(string dayN, string sltHead)
        {
            for (int j = 0; j < dayNum; j++)
            {
                if (ttDays[j].getDayName() == dayN)
                {
                    return ttDays[j].getDomain(sltHead);
                }
            }
            return null;
        }

        public List<DomainNode> getStuSlotDom(string dayN, string sltHead)
        {
            for (int j = 0; j < dayNum; j++)
            {
                if (ttDays[j].getDayName() == dayN)
                {
                    return ttDays[j].getStuClashDomain(sltHead);
                }
            }
            return null;
        }
        public void Reset(Domain mDom)
        {
            for (int i = 0; i < dayNum; i++)
            {
                ttDays[i].Reset(mDom);
            }

            foreach (DomainNode dN in mDom.getDomList())
            {
                trackList.getItem(dN.clsID, dN.secID).remClass = trackList.getItem(dN.clsID, dN.secID).obj.CperWeek;
            }
        }
        public void generateTimeTable(ref ProgressBar prog)
        {
            prog.Minimum = 0;
            prog.Maximum = dayNum - 1;
            prog.Visible = true;
            int rem = 3;
            for (int i = 0; i < dayNum; i++)
            {
                ttDays[i].autoPop(trackList, rem);
                ///////////////////////////////////////////////////////////////////////////////////////
                prog.Value = i;
            }
            float percent = this.getPercent();
            ReAllocate();
            while (percent != this.getPercent())
            {
                percent = this.getPercent();
                ReAllocate();
            }

        }

        public void ReAllocate()
        {
            float percent = this.getPercent();
            if (this.getPercent() < 100)
            {
                for (int i = 0; i < dayNum; i++)
                {
                    ttDays[i].reAllocateRooms();
                }
            }
        }
        public float getPercent()
        {
            List<Clash> list = CourseHash.clashSet.getHeads();
            //initGridViewTrack();
            int i = 0;
            float totalC = 0;
            float insC = 0;
            foreach (Clash c in list)
            {
                HashNode<trackNode> trNode = this.getCheckItem(c.CrsID, c.secID);
                totalC += trNode.obj.CperWeek;
                insC += trNode.obj.CperWeek - trNode.remClass;
                i++;
            }
            return ((insC / totalC) * 100);
        }
        public void getCombinations(List<DomainNode> dL, int rm, int lm)
        {
            var combinations = Combinations(0, rm + lm);
        }

        public static IEnumerable<string> Combinations(int start, int level)
        {
            for (int i = start; i < array.Length; i++)
                if (level == 1)
                    yield return array[i];
                else
                    foreach (string combination in Combinations(i + 1, level - 1))
                        yield return String.Format("{0} {1}", array[i], combination);
        }

        public void generateMaxClashSlotPool(int roomN, int labN)
        {
            Slot slt = new Slot(roomN, labN);
            slt.setDom(MaxDom);

            int avg = CourseHash.clashSet.averageClashCount();
            foreach (DomainNode dN in MaxDom)
            {
                if (CourseHash.clashSet.getClashCount(dN.clsID, dN.secID) < avg)
                    slt.domain.removeCourse(dN.clsID, dN.secID);
            }
            workers = new BackgroundWorker[slt.domain.count];
            best = new Slot[slt.domain.count];
            for (int f = 0; f < slt.domain.count; f++)
            {
                workers[f] = new BackgroundWorker();
                best[f] = new Slot(roomN,labN);
                workers[f].DoWork += threadFunc;
                workers[f].WorkerSupportsCancellation = true;
                workers[f].RunWorkerCompleted += delegate
                {
                    //Debug.Print("Exiting thread: " + threadCount.ToString());
                    threadCount--;
                };
                Thread.Sleep(20);
                List<object> param = new List<object>();
                param.Add(f);
                param.Add(slt.Clone());
                workers[f].RunWorkerAsync(param);
            }
            Thread.Sleep(100);
            while (threadCount > 0)
            {
                continue;
            }
        }
        public void generateSlotPool(List<DomainNode> mDom, int roomN, int labN)
        {
            Slot slt = new Slot(roomN, labN);
            slt.setDom(mDom);
            GC.Collect();
            //ThreadPool.SetMaxThreads(20, 20);
            //ThreadPool.SetMinThreads(10, 10);
            workers = new BackgroundWorker[slt.domain.count];
            best = new Slot[slt.domain.count];
            for (int f = 0; f < slt.domain.count; f++)
            {
                workers[f] = new BackgroundWorker();
                best[f] = null;
                workers[f].DoWork += threadFunc;
                workers[f].WorkerSupportsCancellation = true;
                workers[f].RunWorkerCompleted += delegate
                {
                    //Debug.Print("Exiting thread: " + threadCount.ToString());
                    threadCount--;
                };
                //Thread.Sleep(20);
                List<object> param = new List<object>();
                param.Add(f);
                param.Add(slt.Clone());
                workers[f].RunWorkerAsync(param);
            }
            Thread.Sleep(100);
            while (threadCount > 0)
            {
                continue;
            }
            //sltList.ToString();
        }

        public void threadFunc(object sender, DoWorkEventArgs e)
        {
            threadCount++;
            BackgroundWorker worker = sender as BackgroundWorker;
            List<object> list = e.Argument as List<object>;
            int index = (int)list[0];
            Slot slt = (Slot)list[1];
            int workerIndex = (int)list[0];
            permuteSlots(slt, index, index);
        }

        protected bool allowExec()
        {
            int total = MaxDom.Count;
            for (int i = 0; i < total; i++)
            {// check if all indexes are initialized
                if (best[i] == null)
                    i = 0;
            }

            best.ToString();
            return true;
        }

        protected bool permuteSlots(Slot slt, int index, int wIndex)
        {
            
            if (chkPool.checkDuplicate(slt) == true)
                return false;

            if (slt.getRoomLeft() > slt.domain.count)
                return false;

            //Debug.Print(tb);
            //file.WriteLine(tb + "Inside: " + index.ToString());
            bool rmIns = false;
            bool lbIns = false;
            List<DomainNode> dList = slt.domain.getDomList();
            DomainNode dN = new DomainNode();
            if (index < dList.Count)
                dN = dList.ElementAt(index);

            else
                dN = dList.ElementAt(dList.Count - 1);
            
            if (!dN.Alias.Contains("Lab"))
            {
                rmIns = slt.addRoomCrs(dN.clsID, dN.secID, dN.Alias);
            }

            else
                lbIns = slt.addLabCrs(dN.clsID, dN.secID, dN.Alias);
            
            //rmIns = slt.addRoomCrs(dN.clsID, dN.secID, dN.Alias);           // to be commented
            //GC.Collect();
            if (best[wIndex] == null)
            {
                best[wIndex] = new Slot(slt);
            }

            if (best[wIndex].getRoomLeft() > slt.getRoomLeft())
            {
                best[wIndex] = new Slot(slt);
            }

            while (allowExec() != true)
            {
                Thread.Sleep(10);
            }

            if ((best[wIndex].getRoomLeft() == 0) || (best[wIndex].domain.count == 0))
            {
                workers[wIndex].CancelAsync();
                workers[wIndex].Dispose();
                return true;
            }

            if ((slt.getLabLeft() == 0) && (slt.getRoomLeft() > 0))       // remove comment
            {
                rmIns = true;
                List<DomainNode> tempL = slt.getDom();
                foreach (DomainNode tempN in tempL)
                {
                    if (tempN.Alias.Contains("Lab"))
                        slt.domain.removeCourse(tempN.clsID, tempN.secID);
                }
            }// remove comment
            //GC.Collect();
            if ((slt.domain.count == 0) || ((slt.getRoomLeft() == 0) && (slt.getLabLeft() == 0)))     //remove comment//if ((slt.domain.count == 0) || (slt.getRoomLeft() == 0))
            {

                return pool.Insert(slt);
            }

            if ((lbIns == true) || (rmIns == true))           // remove comment //if(rmIns == true)
            {
                for (int i = 0; i < slt.domain.count; i++)
                {
                    permuteSlots(slt.Clone(), i, wIndex);

                }
                chkPool.Insert(slt.Clone());
            }

            else
            {

                    return pool.Insert(slt.Clone());
                    
            }

            return true;
        }

        public void RemSlot(int i, string headTxt)
        {
            ttDays[i].RemoveSlot(headTxt);
        }

        public void RemSlotAll(string headTxt)
        {
            for (int i = 0; i < dayNum; i++)
            {
                ttDays[i].RemoveSlot(headTxt);
            }
        }

        public int getMaxDaySlot()
        {
            int max = 0;
            for (int i = 0; i < dayNum; i++)
            {
                if (max < ttDays[i].slotCount)
                    max = ttDays[i].slotCount;
            }

            return max;
        }

        public List<Batch> getBatchPriority(int d, string s)
        {
            return ttDays[d].getBatchPriority(s);
        }

        public bool getMaxVal(int d, string s)
        {
            return ttDays[d].getMax(s);
        }

        public void setSlotMaxVal(int d, string s)
        {
            ttDays[d].setMax(s);
        }

        public void setSlotMinVal(int d, string s)
        {
            ttDays[d].setMin(s);
        }

        public void setBatchPriority(int d, string s, List<Batch> bList)
        {
            ttDays[d].setBatchPriority(s, bList);
        }
    }
}
