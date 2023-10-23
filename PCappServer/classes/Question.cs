using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCappServer.classes
{
    internal class Question
    {
        public string ques { set; get; }
        public string answer { set; get; }
        public string opt1 { set; get; }
        public string opt2 { set; get; }
        public string opt3 { set; get; }
        public string opt4 { set; get; }
        public string type { set; get; }
        public int eventId { set; get; }
    }
}
