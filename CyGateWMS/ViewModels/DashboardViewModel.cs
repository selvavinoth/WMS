namespace CyGateWMS.ViewModels
{
    public class DashboardViewModel
    {
        public int ProductCount { get; set; }
        public int ReportCount { get; set; }
        public int UserCount { get; set; }
        public int AllowancesCount { get; set; }
        public int AllowancePending { get; set; }
        public int AllowanceApproved { get; set; }
        public int AllowanceClosed { get; set; }
        public int DepartmentCount { get; set; }

        public int OnCallSweden { get; set; }
        public int OnCallIndia { get; set; }
    }
}
