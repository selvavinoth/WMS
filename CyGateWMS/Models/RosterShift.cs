using System;
using System.ComponentModel.DataAnnotations;

namespace CyGateWMS.Models
{
    public class RosterShift
    {
        [Key]
        public int RosterShiftId { get; set; }
        public string RosterShiftName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
