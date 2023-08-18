using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PCappServer.classes
{
    internal class User
    {
        public string Name { get; set; } 
        public Socket Socket { get; set; }

       public User(string name, Socket socket)
        {
            Name = name;
            Socket = socket;
        }
    }
}
