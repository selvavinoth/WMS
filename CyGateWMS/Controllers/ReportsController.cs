using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CyGateWMS.Models;
using Microsoft.AspNetCore.Identity;
using CyGateWMS.ViewModels;
using System.Linq.Expressions;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace CyGateWMS.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly CygateWMSContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private static ReportGenerationViewModel _reportVM = new ReportGenerationViewModel();

        public ReportsController(CygateWMSContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            this.userManager = userManager;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            ReportGenerationViewModel generationViewModel = new ReportGenerationViewModel();
            generationViewModel.Reports = await GetReports();
            _reportVM.Reports = generationViewModel.Reports;
            return View(GetIndexReportViewModel(generationViewModel));
        }        
        private async Task<List<Report>> GetReports()
        {
            var user = userManager.GetUserAsync(HttpContext.User).Result;
            List<Report> Reports = null;
            var cygateWMSContext = _context.Report.Include(r => r.Category).Include(r => r.Shift).Include(r => r.Type).Include(r => r.onCallEscalate).Include(e => e.User);
            if (User.IsInRole(Constants.ADMIN))
                Reports = await cygateWMSContext.Where(e=>e.StartTime > DateTime.Today.AddMonths(-1)).ToListAsync();
            else if (User.IsInRole(Constants.TL))
                Reports = await cygateWMSContext.Where(e => e.CategoryID == user.CategoryId && e.StartTime > DateTime.Today.AddMonths(-1)).ToListAsync();
            else if (User.IsInRole(Constants.USER))
                Reports = await cygateWMSContext.Where(e => e.UserID == user.Id && e.StartTime > DateTime.Today.AddMonths(-1)).ToListAsync();

            return Reports.OrderByDescending(e=>e.StartTime).ToList();
        }
        public async Task<IActionResult> OnCallSweden()
        {
            ReportGenerationViewModel generationViewModel = new ReportGenerationViewModel();   
             generationViewModel.Reports = await GetReports();
            int escalateId = _context.onCallEscalates.SingleOrDefault(e => e.OnCallEscalateName.ToLower().Contains("sweden")).OnCallEscalateId;
            generationViewModel.Reports.RemoveAll(e => e.onCallEscalateId != escalateId);
            _reportVM.Reports = generationViewModel.Reports;
            return View("Index", GetIndexReportViewModel(generationViewModel));
        }

        public async Task<IActionResult> OnCallIndia()
        {
            ReportGenerationViewModel generationViewModel = new ReportGenerationViewModel();
            generationViewModel.Reports = await GetReports();
            int escalateId = _context.onCallEscalates.SingleOrDefault(e => e.OnCallEscalateName.ToLower().Contains("india")).OnCallEscalateId;
            generationViewModel.Reports.RemoveAll(e => e.onCallEscalateId != escalateId);
            _reportVM.Reports = generationViewModel.Reports;
            return View("Index", GetIndexReportViewModel(generationViewModel));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Models.Report report = await _context.Report
                .Include(r => r.Category)
                .Include(r => r.Shift)
                .Include(r => r.Type)
                .Include(r => r.User)
                .Include(r => r.onCallEscalate)
                .FirstOrDefaultAsync(m => m.ReportID == id);
            ReportViewModel reportViewModel = new ReportViewModel()
            {
                ReportID = report.ReportID,
                Description = report.Description,
                TicketNo = report.TicketNo,
                CategoryName = report.Category?.CategoryName,
                ShiftName = report.Shift?.ShiftName,
                TypeName = report.Type?.TypeName,
                OnCallEscalateName = report.onCallEscalate?.OnCallEscalateName,
                StartTime = report.StartTime,
                ArrivalTime = report.ArrivalTime,
                EndTime = report.EndTime,
                UserName = report.User.Name,
                TimeSpent = report.TimeSpent
            };
            if (report == null)
            {
                return NotFound();
            }

            return View(reportViewModel);
        }

        // GET: Reports/Create
        public IActionResult Create()
        {
            return View(GetReportViewModel(new ReportViewModel()));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReportViewModel reportVM)
        {

            if (ModelState.IsValid && !string.IsNullOrEmpty(reportVM.CategoryID) && !string.IsNullOrEmpty(reportVM.ShiftID))
            {
                var report = new Models.Report
                {
                    TicketNo = reportVM.TicketNo,
                    Description = reportVM.Description,
                    User = userManager.GetUserAsync(HttpContext.User).Result,
                    Shift = _context.Shift.SingleOrDefault(e => e.ShiftId.ToString() == reportVM.ShiftID),
                    Type = _context.Type.SingleOrDefault(e => e.TypeId.ToString() == reportVM.TypeID),
                    Category = _context.Category.SingleOrDefault(e => e.CategoryId.ToString() == reportVM.CategoryID),
                    onCallEscalate = _context.onCallEscalates.SingleOrDefault(e => e.OnCallEscalateId.ToString() == reportVM.OnCallEscalateID),
                    StartTime = reportVM.StartTime,
                    EndTime = reportVM.EndTime,
                    ArrivalTime = reportVM.ArrivalTime,
                    TimeSpent = reportVM.TimeSpent

                };
                _context.Add(report);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            GetReportViewModel(reportVM);
            return View(reportVM);
        }
       
        // GET: Reports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            Models.Report report = await _context.Report.Include(e=>e.Category).Include(e=>e.Shift)
                .Include(e=>e.Type).Include(e=>e.User).Include(e => e.onCallEscalate).Where(e => e.ReportID == id).FirstOrDefaultAsync();
            //var report = reports.SingleOrDefault(e=>e.ReportID == id);
            if (report == null)
            {
                return NotFound();
            }
            return View(GetReportEditViewModel(report));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReportViewModel reportVM)
        {
            if (id != reportVM.ReportID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var report = await _context.Report
                                .Include(r => r.Category)
                                .Include(r => r.Shift)
                                .Include(r => r.Type)
                                .Include(r => r.User)
                                .Include(r => r.onCallEscalate)
                                .FirstOrDefaultAsync(m => m.ReportID == id);
                   if(User.IsInRole(Constants.ADMIN))
                        report.Category = _context.Category.SingleOrDefault(e => e.CategoryId.ToString() == reportVM.CategoryID);
                    report.TicketNo = reportVM.TicketNo;
                    report.Description = reportVM.Description;
                    report.ArrivalTime = reportVM.ArrivalTime != null ? reportVM.ArrivalTime : null;
                    report.TimeSpent = reportVM.TimeSpent;
                    report.StartTime = reportVM.StartTime;
                    report.EndTime = reportVM.EndTime != null ? reportVM.EndTime : null;
                    report.Shift = _context.Shift.Find(Convert.ToInt32(reportVM.ShiftID));
                    report.Type = _context.Type.Find(Convert.ToInt32(reportVM.TypeID));
                    report.onCallEscalate = _context.onCallEscalates.Find(Convert.ToInt32(reportVM.OnCallEscalateID));
                    _context.Update(report);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReportExists(reportVM.ReportID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            return View(reportVM);
        }

        // GET: Reports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Report
                .Include(r => r.Category)
                .Include(r => r.Shift)
                .Include(r => r.Type)
                .Include(r => r.User)
                .Include(r => r.onCallEscalate)
                .FirstOrDefaultAsync(m => m.ReportID == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: Reports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.Report.FindAsync(id);
            _context.Report.Remove(report);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Filter(ReportGenerationViewModel model)
        {            
            var where = GetWhere(model);

            if (User.IsInRole(Constants.ADMIN))
                model.Reports = _context.Report.Where(where)
                           .Include(r => r.Category).Include(r => r.Shift).Include(r => r.Type).Include(r => r.User).Include(r => r.onCallEscalate).ToList();
            else if (User.IsInRole(Constants.TL))
                model.Reports = _context.Report.Where(where)
                           .Include(r => r.Category).Include(r => r.Shift).Include(r => r.Type).Include(r => r.User).Include(r => r.onCallEscalate).ToList();
            else if (User.IsInRole(Constants.USER))
                model.Reports = _context.Report.Where(where)
                            .Include(r => r.Category).Include(r => r.Shift).Include(r => r.Type).Include(r => r.User).Include(r => r.onCallEscalate).ToList();

            _reportVM = model;
            return View("Index", GetIndexReportViewModel(model));
        }
        private Expression<Func<Models.Report, bool>> GetWhere(ReportGenerationViewModel model)
        {
            Expression<Func<Models.Report, bool>> where = c => true;
            var user = userManager.GetUserAsync(HttpContext.User).Result;

            
            if (model.StartTime != null)
            {
                var prefix = where.Compile();
                where = c => prefix(c) && c.StartTime >= model.StartTime;
            }
            else
            {
                var prefix = where.Compile();
                where = c => prefix(c) && c.StartTime > DateTime.Today.AddMonths(-1);
            }
            if (model.EndTime != null)
            {
                var prefix = where.Compile();
                where = c => prefix(c) && c.StartTime <= model.EndTime;
            }
            if (!string.IsNullOrEmpty(model.CategoryID) || User.IsInRole(Constants.TL))
            {
                var prefix = where.Compile();
                    if(User.IsInRole(Constants.TL))
                        where = c => prefix(c) && c.CategoryID == user.CategoryId;
                    else if (!string.IsNullOrEmpty(model.CategoryID))
                        where = c => prefix(c) && c.CategoryID.ToString() == model.CategoryID;
                }
            if (!string.IsNullOrEmpty(model.ShiftID))
            {
                var prefix = where.Compile();
                where = c => prefix(c) && c.ShiftID.ToString() == model.ShiftID;
            }
            if (!string.IsNullOrEmpty(model.OnCallEscalateID))
            {
                var prefix = where.Compile();
                where = c => prefix(c) && c.onCallEscalateId.ToString() == model.OnCallEscalateID;
            }
            if (!string.IsNullOrEmpty(model.TypeID))
            {
                var prefix = where.Compile();
                where = c => prefix(c) && c.TypeID.ToString() == model.TypeID;
            }
            if (!string.IsNullOrEmpty(model.UserID) || User.IsInRole(Constants.USER))
            {                
                if (User.IsInRole(Constants.USER))
                {
                    var prefix = where.Compile();
                    where = c => prefix(c) && c.UserID.ToString() == user.Id;
                }
                else if (!string.IsNullOrEmpty(model.UserID))
                {
                    var prefix = where.Compile();
                    where = c => prefix(c) && c.UserID.ToString() == model.UserID;
                }
            }
           return where;
        }
        public IActionResult Report()
        {
            StringBuilder sbRtn = new StringBuilder();
                      

            // If you want headers for your file
            var header = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\""
                                       ,"Serial No"
                                       ,"Ticket Number"
                                       , "Category"
                                       , "Shift"
                                       , "Type"
                                       , "On - call called"
                                       , "Arrival Time"
                                       , "Start Time"
                                       , "End Time"
                                       , "Total Time spent"
                                       , "Name"
                                       , "Notes / Description"
                                      );
            sbRtn.AppendLine(header);
            int count = 0;
            _reportVM.Reports.ForEach(data =>
           {
           var queryResults = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\""
                                        , ++count
                                        , data.TicketNo
                                        , data.Category?.CategoryName
                                        , data.Shift?.ShiftName
                                        , data.Type?.TypeName
                                        , data.onCallEscalate?.OnCallEscalateName
                                        , data.ArrivalTime
                                        , data.StartTime
                                        , data.EndTime
                                        , data.TimeSpent
                                        , _context.User.SingleOrDefault(e=>e.Id == data.UserID).Name
                                        , data.Description);
               sbRtn.AppendLine(queryResults);
           });


            return File(System.Text.Encoding.ASCII.GetBytes(sbRtn.ToString()), "text/csv", "Ticket Tracking.csv");
        }
        private bool ReportExists(int id)
        {
            return _context.Report.Any(e => e.ReportID == id);
        }
        private ReportViewModel GetReportViewModel(ReportViewModel reportVM)
        {
            reportVM.FilterTypes = _context.Type.ToList();
            reportVM.Shifts = _context.Shift.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.ShiftName,
                Value = e.ShiftId.ToString()
            }).ToList();
            reportVM.Types = _context.Type.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.TypeName,
                Value = e.TypeId.ToString(),
                Selected = e.TypeName == Constants.INCIDENT ? true : false
            }).ToList();
            if(User.IsInRole(Constants.ADMIN))
            {
                reportVM.Categories = _context.Category.Where(e => e.IsActive).Select(e => new SelectListItem
                {
                    Text = e.CategoryName,
                    Value = e.CategoryId.ToString()

                }).ToList();
            }
            else
            {
                reportVM.Categories = _context.Category.Where(e => e.IsActive && e.CategoryId == userManager.GetUserAsync(HttpContext.User).Result.CategoryId).Select(e => new SelectListItem
                {
                    Text = e.CategoryName,
                    Value = e.CategoryId.ToString()

                }).ToList();
            }
            
            reportVM.OnCallEscalates = _context.onCallEscalates.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.OnCallEscalateName,
                Value = e.OnCallEscalateId.ToString(),
                Selected = e.OnCallEscalateName == Constants.NO ? true:false
            }).ToList();

            //if (string.IsNullOrEmpty(reportVM?.TypeID))
            //    reportVM.Types.Add(new SelectListItem { Text = "Select Type", Value = string.Empty, Selected = true });
            if (string.IsNullOrEmpty(reportVM?.ShiftID))
                reportVM.Shifts.Add(new SelectListItem { Text = "Select Shift", Value = string.Empty, Selected = true });
            //if (string.IsNullOrEmpty(reportVM?.OnCallEscalateID))
            //    reportVM.OnCallEscalates.Add(new SelectListItem { Text = "Select OnCall", Value = string.Empty, Selected = true });


            return reportVM;
        }

        private DateTime? ConvertToYYYYMMDD(DateTime? date)
        {
            if (date.HasValue)
            {
                string temp = date.Value.ToString("yyyy-MM-dd HH:mm:ss");
                date = Convert.ToDateTime(temp);
            }
            return date;
        }
        private ReportViewModel GetReportEditViewModel(Models.Report report)
        {
            var user = userManager.GetUserAsync(HttpContext.User).Result;
            ReportViewModel reportVM = new ReportViewModel();
            reportVM.ReportID = report.ReportID;
            reportVM.TicketNo = report?.TicketNo;
            reportVM.Description = report?.Description;
            reportVM.StartTime = ConvertToYYYYMMDD(report?.StartTime);
            reportVM.EndTime = ConvertToYYYYMMDD(report?.EndTime);
            reportVM.ArrivalTime = ConvertToYYYYMMDD(report?.ArrivalTime);
            reportVM.TimeSpent = report?.TimeSpent;
            reportVM.ShiftName = report.Shift.ShiftName;
            reportVM.CategoryName = report.Category.CategoryName;
            reportVM.TypeName = report.Type.TypeName;
            reportVM.FilterTypes = _context.Type.ToList();

            reportVM.Shifts = _context.Shift.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.ShiftName,
                Value = e.ShiftId.ToString(),
                Selected = (e.ShiftId == report.ShiftID) ? true : false
            }).ToList();
            reportVM.Types = _context.Type.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.TypeName,
                Value = e.TypeId.ToString(),
                Selected = (e.TypeId == report.TypeID) ? true : false
            }).ToList();
            reportVM.OnCallEscalates = _context.onCallEscalates.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.OnCallEscalateName,
                Value = e.OnCallEscalateId.ToString(),
                Selected = (e.OnCallEscalateId == report.onCallEscalateId) ? true : false
            }).ToList();
            if (User.IsInRole(Constants.ADMIN))
            {
                reportVM.Categories = _context.Category.Where(e => e.IsActive).Select(e => new SelectListItem
                {
                    Text = e.CategoryName,
                    Value = e.CategoryId.ToString(),
                    Selected = (e.CategoryId == report.CategoryID) ? true : false
                }).ToList();
            }
            else
            {
                reportVM.Categories = _context.Category.Where(e => e.IsActive && e.CategoryId == user.CategoryId).Select(e => new SelectListItem
                {
                    Text = e.CategoryName,
                    Value = e.CategoryId.ToString(),
                    Selected = (e.CategoryId == report.CategoryID) ? true : false

                }).ToList();
            }

            return reportVM;
        }
        private ReportGenerationViewModel GetIndexReportViewModel(ReportGenerationViewModel report)
        {
            var user = userManager.GetUserAsync(HttpContext.User).Result;
            report.Shifts = _context.Shift.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.ShiftName,
                Value = e.ShiftId.ToString(),
                Selected = e.ShiftId.ToString() == report.ShiftID ? true : false
            }).ToList();
            report.Types = _context.Type.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.TypeName,
                Value = e.TypeId.ToString(),
                Selected = e.TypeId.ToString() == report.TypeID ? true : false
            }).ToList();
            report.Categories = _context.Category.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.CategoryName,
                Value = e.CategoryId.ToString(),
                Selected = e.CategoryId.ToString() == report.CategoryID ? true : false
            }).ToList();
            if (User.IsInRole(Constants.ADMIN))
            {
                report.Users = userManager.Users.Select(e => new SelectListItem
                {
                    Text = e.Name,
                    Value = e.Id,
                    Selected = e.Id == report.UserID ? true : false

                }).ToList();
            }
            else if (User.IsInRole(Constants.TL))
            {
                report.Users = userManager.Users.Where(e=>e.CategoryId == user.CategoryId).Select(e => new SelectListItem
                {
                    Text = e.Name,
                    Value = e.Id,
                    Selected = e.Id == report.UserID ? true : false

                }).ToList();
            }
            else { report.Users = new List<SelectListItem>(); }
            report.OnCallEscalaties = _context.onCallEscalates.Select(e => new SelectListItem
            {
                Text = e. OnCallEscalateName,
                Value = e.OnCallEscalateId.ToString(),
                Selected = e.OnCallEscalateId.ToString() == report.OnCallEscalateID ? true : false

            }).ToList();
            report.Categories.Add(new SelectListItem { Text = "Select Department", Value = string.Empty, Selected = string.IsNullOrEmpty(report?.CategoryID) ? true : false });
            report.Types.Add(new SelectListItem { Text = "Select Ticket Type", Value = string.Empty, Selected = string.IsNullOrEmpty(report?.TypeID) ? true : false });
            report.Shifts.Add(new SelectListItem { Text = "Select Shift", Value = string.Empty, Selected = string.IsNullOrEmpty(report?.ShiftID) ? true : false });
            report.OnCallEscalaties.Add(new SelectListItem { Text = "Select OnCall", Value = string.Empty, Selected = string.IsNullOrEmpty(report?.OnCallEscalateID) ? true : false });
            report.Users.Add(new SelectListItem { Text = "Select User", Value = string.Empty, Selected = string.IsNullOrEmpty(report?.UserID) ? true : false });

            return report;
        }
    }
}
