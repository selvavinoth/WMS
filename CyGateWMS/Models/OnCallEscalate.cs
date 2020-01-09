using System;
using System.ComponentModel.DataAnnotations;

namespace CyGateWMS.Models
{
    public class OnCallEscalate
    {
        [Key]
        public int OnCallEscalateId { get; set; }
        public string OnCallEscalateName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
