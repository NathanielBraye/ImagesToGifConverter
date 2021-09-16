using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagesToGifConverter
{
    class HistoriqueLog
    {

        public int id;
        public string pathName;
        public string type;
        public DateTime date;
        public bool alive;
        public HistoriqueLog()
        {
        }

        public HistoriqueLog(int id, string pathName, string type, DateTime date)
        {
            this.id = id;
            this.pathName = pathName;
            this.type = type;
            this.date = date;
        }
        public override string ToString()
        {
            return id + "," + pathName + "," + type + "," + date + "\n";
        }



    }
}
