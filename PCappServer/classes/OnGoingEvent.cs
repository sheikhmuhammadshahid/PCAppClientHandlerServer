using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCappServer.classes
{
    internal class OnGoingEvent
    {
        public  Question question { set; get; }

        public int eventId { set; get; }
        public string questionForTeam { set; get; }
        public string round { set; get;}
        public int questionNo  { set; get; }
        public int totalQuestions { set; get; }

    }
}
