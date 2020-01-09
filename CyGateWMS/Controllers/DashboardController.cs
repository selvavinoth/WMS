using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CyGateWMS.Models;
using CyGateWMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CyGateWMS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly CygateWMSContext context;
        private readonly UserManager<ApplicationUser> userManager;
        public DashboardController(CygateWMSContext context, UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }
        // GET: Dashboard
        public async Task<ActionResult> Index()
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            ViewBag.UserId = user.Id;
            List<Allowance> allowances = null;
            List<Report> reports = null;
            List<ApplicationUser> Users = null;

            if (User.IsInRole(Constants.ADMIN))
            {
                allowances = context.Allowance.Where(e=> e.Month.Month == DateTime.Today.AddMonths(-1).Month).ToList();
                reports = context.Report.Include(e => e.Type).Include(e => e.User).Where(e => e.StartTime > DateTime.Today.AddMonths(-1)).ToList();
                Users = userManager.Users.Where(e=>e.IsActive).ToList();
                ViewBag.DataPoints = JsonConvert.SerializeObject(GetDataForColumnCharts(reports), _jsonSetting);
                ViewBag.PieDataPoints = JsonConvert.SerializeObject(GetDataForPieCharts(reports), _jsonSetting);
            }
            if (User.IsInRole(Constants.FINANCE))
            {
                allowances = context.Allowance.Where(e => e.Month.Month == DateTime.Today.AddMonths(-1).Month).ToList();
                reports = context.Report.Include(e => e.Type).Include(e => e.User).Where(e => e.StartTime > DateTime.Today.AddMonths(-1) && e.User.IsActive).ToList();
                Users = userManager.Users.Where(e => e.CategoryId == user.CategoryId && e.IsActive).ToList();
                ViewBag.DataPoints = JsonConvert.SerializeObject(GetDataForColumnCharts(reports), _jsonSetting);
                ViewBag.PieDataPoints = JsonConvert.SerializeObject(GetDataForPieCharts(reports), _jsonSetting);
            }
            else if (User.IsInRole(Constants.TL))
            {
                allowances = context.Allowance.Where(e=>e.AssignedCategoryId == user.CategoryId && e.Month.Month == DateTime.Today.AddMonths(-1).Month).ToList();
                reports = context.Report.Include(e=>e.Type).Include(e => e.User).Where(e => e.CategoryID == user.CategoryId && e.User.IsActive && e.StartTime > DateTime.Today.AddMonths(-1)).ToList();
                Users = userManager.Users.Where(e => e.CategoryId == user.CategoryId && e.IsActive).ToList();
                ViewBag.DataPoints = JsonConvert.SerializeObject(GetDataForColumnCharts(Users, reports), _jsonSetting);
                ViewBag.PieDataPoints = JsonConvert.SerializeObject(GetDataForPieCharts(reports), _jsonSetting);
            }
            else if (User.IsInRole(Constants.USER))
            {
                allowances = context.Allowance.Where(e=>e.CreatedById == user.Id && e.Month.Month == DateTime.Today.AddMonths(-1).Month).ToList();
                reports = context.Report.Include(e => e.Type).Where(e => e.UserID == user.Id && e.StartTime > DateTime.Today.AddMonths(-1)).ToList();
                Users = userManager.Users.Where(e => e.Id == user.Id && e.IsActive).ToList();                
                ViewBag.DataPoints = JsonConvert.SerializeObject(GetDataForColumnCharts(userManager.Users.Where(e => e.CategoryId == user.CategoryId && e.IsActive).ToList(),
                    context.Report.Include(e=>e.User).Where(e => e.CategoryID == user.CategoryId && e.User.IsActive).ToList()), _jsonSetting);
                ViewBag.PieDataPoints = JsonConvert.SerializeObject(GetDataForPieCharts(reports), _jsonSetting);
            }
            int swedenEscalateId = context.onCallEscalates.SingleOrDefault(e => e.OnCallEscalateName.ToLower().Contains("sweden")).OnCallEscalateId;
            int indiaEscalateId = context.onCallEscalates.SingleOrDefault(e => e.OnCallEscalateName.ToLower().Contains("india")).OnCallEscalateId;
            DashboardViewModel dashboard = null;
            if (!User.IsInRole(Constants.USER))
            {
                dashboard = new DashboardViewModel()
                {
                    AllowancesCount = allowances.GroupBy(e => e.CreatedById).Count(),
                    AllowanceApproved = allowances.Where(e => e.ApprovedStatus == ApprovedStatus.APPROVED).GroupBy(e=>e.CreatedById).Count(),
                    AllowancePending = allowances.Where(e => e.ApprovedStatus == ApprovedStatus.PENDING).GroupBy(e => e.CreatedById).Count(),
                    ReportCount = reports.Count(),
                    DepartmentCount = context.Category.Count(),
                    UserCount = context.User.Count(),
                    OnCallIndia = reports.Where(e => e.onCallEscalateId == indiaEscalateId).Count(),
                    OnCallSweden = reports.Where(e => e.onCallEscalateId == swedenEscalateId).Count()
                };
            }
            else if (User.IsInRole(Constants.USER))
            {
                dashboard = new DashboardViewModel()
                {
                    AllowancesCount = allowances.Where(e => e.CreatedById == user.Id).Count(),
                    AllowanceApproved = allowances.Where(e => e.ApprovedStatus == ApprovedStatus.APPROVED).Count(),
                    AllowancePending = allowances.Where(e => e.ApprovedStatus == ApprovedStatus.PENDING).Count(),
                    ReportCount = reports.Count(),
                    DepartmentCount = context.Category.Count(),
                    UserCount = context.User.Count(),
                    OnCallIndia = reports.Where(e => e.onCallEscalateId == indiaEscalateId).Count(),
                    OnCallSweden = reports.Where(e => e.onCallEscalateId == swedenEscalateId).Count()
                };
            }
            return View(dashboard);
        }

        private List<DataPoint> GetDataForColumnCharts(List<ApplicationUser> users, List<Report> reports)
        {
            _dataPoints = new List<DataPoint>();
            
            foreach (var item in users)
            {
                int count = reports.Where(e => e.UserID == item.Id).ToList().Count();
                _dataPoints.Add(new DataPoint(count, item.Name));

            }

            return _dataPoints;
        }
        private List<DataPoint> GetDataForColumnCharts(List<Report> reports)
        {
            _dataPoints = new List<DataPoint>();

            var categories = context.Category.Distinct();
            foreach (var item in categories)
            {
                int count = reports.Where(e => e.CategoryID == item.CategoryId).ToList().Count();
                _dataPoints.Add(new DataPoint(count, item.CategoryName));

            }
            return _dataPoints;
        }
        private List<DataPoint> GetDataForPieCharts(List<Report> reports)
        {
            _dataPoints = new List<DataPoint>();
            List<Models.Type> types = context.Type.Where(e=>e.IsActive).ToList();
            foreach (var item in types)
            {
                int count = reports.Where(e => e.TypeID == item.TypeId).ToList().Count();                
                _dataPoints.Add(new DataPoint(count, item.TypeName, item.TypeName));

            }
            return _dataPoints;
        }
        private static Random random = new Random(DateTime.Now.Millisecond);
        private static List<DataPoint> _dataPoints;
        JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
    }
}