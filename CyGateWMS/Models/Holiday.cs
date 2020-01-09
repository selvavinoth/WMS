using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CyGateWMS.Models
{
    public class Holiday
    {
        public int ID { get; set; }
        public string Month { get; set; }
        public DateTime Date { get; set; }
        public string Day { get; set; }
        public int Year { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
