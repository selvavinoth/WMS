using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyGateWMS.Models
{
    public class AllowanceType
    {
        public int AllowanceTypeId { get; set; }
        [DataType(DataType.Currency)]
        public decimal AllowanceTypePrice { get; set; }
        public string AllowanceTypeName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public bool IsActive { get; set; }
        public string Categories { get; set; }

        [NotMapped]
        public List<Category> catagoriesList { get; set; }
    }
}
