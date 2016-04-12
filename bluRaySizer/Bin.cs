using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace bluRaySizer
{
    class Bin : SimpleFileInfo
    {
        private double _maxSizeMb;

        public Bin(double maxSizeMb)
        {
            _maxSizeMb = maxSizeMb;
            this.Files = new List<SimpleFileInfo>();
        }

        public List<SimpleFileInfo> Files { get; set; }

        public double RemainingSizeMb
        {
            get
            {
                return _maxSizeMb - Files.Sum(f => f.SizeMb);
            }
        }

        ~Bin()
        {
            //Log.LogWrite("Bin destructor called.");
        }


    }
}
