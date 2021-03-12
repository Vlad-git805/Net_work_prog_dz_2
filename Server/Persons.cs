using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Persons
    {
        public string Name { get; set; }
        public string Password { get; set; }

        public Persons(string name, string password)
        {
            Name = name;
            Password = password;
        }
    }
}
