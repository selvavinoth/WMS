using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyGateWMS.Models
{
    public class Report
    {
        [Key]
        public int ReportID { get; set; }
        [Required]
        public string TicketNo { get; set; }
        [Required]
        public string Description { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string TimeSpent { get; set; }
        public int ShiftID { get; set; }
        [ForeignKey("ShiftID")]
        public Shift Shift { get; set; }
        public int CategoryID { get; set; }
        [ForeignKey("CategoryID")]
        public Category Category { get; set; }
        public int TypeID { get; set; }
        [ForeignKey("TypeID")]
        public Type Type { get; set; }
        public string UserID { get; set; }
        [ForeignKey("UserID")]
        public ApplicationUser User { get; set; }
        public int onCallEscalateId { get; set; }
        [ForeignKey("onCallEscalateId")]
        public OnCallEscalate onCallEscalate { get; set; }

    }
}
