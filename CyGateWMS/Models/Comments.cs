using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyGateWMS.Models
{
    public class Comments
    {
        [Key]
        public int CommentsId { get; set; }
        public string Description { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public int AllowanceId { get; set; }
        [ForeignKey("AllowanceId")]
        public Allowance Allowance { get; set; }
    }
}
