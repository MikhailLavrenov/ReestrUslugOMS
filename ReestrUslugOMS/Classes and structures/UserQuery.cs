using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReestrUslugOMS.Classes_and_structures
{
    public class UserQuery
    {
        public string Caption { get; set; }
        public string Description { get; set; }
        public string ProcName { get; set; }
        public bool WithPatient { get; set; }
        public int[] ColsWidth { get; set; }
    }
}

