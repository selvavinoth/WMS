using System;
using System.ComponentModel.DataAnnotations;

namespace CyGateWMS.Models
{
    public class Type
    {
        [Key]
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public string FilterValue { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
