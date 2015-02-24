using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Smart_Schedule
{
    class Instructor
    {
        private String insID;
        private String insName;

        public Instructor()
        {
            insID = "";
            insName = "";
        }

        public Instructor(String id, String name)
        {
            insID = id;
            insName = name;
        }

        public String getInsID()
        {
            return insID;
        }

        public String getInsName()
        {
            return insName;
        }
    }
}
