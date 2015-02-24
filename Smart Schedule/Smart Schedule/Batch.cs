using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Smart_Schedule
{
    public class Batch
    {
        public int batch;
        public Color color;
        public bool repeat;

        public Batch()
        {
            repeat = false;
        }

        public Batch(int b, Color c)
        {
            batch = b;
            color = c;
            repeat = false;
        }

        public void setBatch(int b)
        { batch = b; }
        public void setColor(Color c)
        { color = c; }
        public void setRepeat(bool val)
        { repeat = val; }
        public int getBatch()
        { return batch; }
        public Color getColor()
        { return color; }
        public bool getRepeat()
        { return repeat; }
        public bool Compare(Batch compare)
        {
            if (compare == null)
                return false;
            if((this.getBatch() == compare.getBatch())&&(this.getColor() == compare.getColor())&&(this.getRepeat() == compare.getRepeat()))
                return true;

            else
                return false;
        }
    }
}
