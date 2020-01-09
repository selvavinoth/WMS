using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyGateWMS.Models
{
    public class Allowance
    {
        [Key]
        public int AllowanceID { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int NumberOfDays { get; set; }
        public ApprovedStatus ApprovedStatus { get; set; }
        public int AllowanceTypeId { get; set; }
        [ForeignKey("AllowanceTypeId")]
        public AllowanceType AllowanceType { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string AssignedById { get; set; }
        public string AssignedToId { get; set; }
        public string CreatedById { get; set; }
        [ForeignKey("AssignedById")]
        public ApplicationUser AssignedBy { get; set; }
        [ForeignKey("AssignedToId")]
        public ApplicationUser AssignedTo { get; set; }
        [ForeignKey("CreatedById")]
        public ApplicationUser CreatedBy { get; set; }

        public int AssignedCategoryId { get; set; }
        [ForeignKey("AssignedCategoryId")]
        public Category AssignedCategory { get; set; }
        public List<Comments> Comments { get; set; }
        public string AllowanceDates { get; set; }
        public DateTime Month { get; set; }
    }
}
