using System;
using System.ComponentModel.DataAnnotations;

namespace CyGateWMS.Models
{
    public class Shift
    {
        [Key]
        public int ShiftId { get; set; }
        public string ShiftName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
