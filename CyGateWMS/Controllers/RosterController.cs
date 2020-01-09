using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CyGateWMS.Models;
using CyGateWMS.Services;
using CyGateWMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CyGateWMS.Controllers
{
    [Authorize]
    public class RosterController : Controller
    {
        private readonly CygateWMSContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailService emailService;
        private readonly IConfiguration configuration;
        private static RosterViewModel _rosterViewModel = new RosterViewModel();
        public RosterController(CygateWMSContext context, UserManager<ApplicationUser> userManager,
            IConfiguration configuration, IEmailService emailService)
        {
            this.userManager = userManager;
            this.context = context;
            this.emailService = emailService;
            this.configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.IsCurrentMonth = true;
            ViewBag.Date = DateTime.Now.ToString("MMMM") + "/" + DateTime.Now.Year;
            return View(await GetIndexRosterViewModel(DateTime.Now,DateTime.Now));
        }

        public async Task<IActionResult> Previous()
        {

            ViewBag.IsPreviousMonth = true;
            int monthInt = DateTime.Now.Month - 1;
            DateTime year = DateTime.Now;
            DateTime month = DateTime.Now.AddMonths(-1);
            ViewBag.month = monthInt;
            if (monthInt > 0)
            {
                ViewBag.Date = DateTime.Now.AddMonths(-1).ToString("MMMM") + "/" + DateTime.Now.Year;
            }
            else
            {
                ViewBag.Date = DateTime.Now.AddMonths(-1).ToString("MMMM") + "/" + DateTime.Now.AddYears(-1).Year;
                year = DateTime.Now.AddYears(-1);
            }
            
            return View(nameof(Index), await GetIndexRosterViewModel(year, month));
        }
        public async Task<IActionResult> Next()
        {

            ViewBag.IsNextMonth = true;
            int monthInt = DateTime.Now.Month + 1;
            DateTime year = DateTime.Now;
            DateTime month = DateTime.Now.AddMonths(1);
            ViewBag.month = month;
            if (monthInt > 0)
            {                
                ViewBag.Date = DateTime.Now.AddMonths(1).ToString("MMMM") + "/" + DateTime.Now.Year;
            }
            else
            {
                ViewBag.Date = DateTime.Now.AddMonths(1).ToString("MMMM") + "/" + DateTime.Now.AddYears(1).Year;
                year = DateTime.Now.AddYears(1);
            }

            return View(nameof(Index), await GetIndexRosterViewModel(year, month));
        }
        public async Task<RosterViewModel> GetIndexRosterViewModel(DateTime Year, DateTime Month)
        {
            RosterViewModel rosterViewModel = new RosterViewModel();
            rosterViewModel.Month = Month;
            var currentUser = await userManager.GetUserAsync(HttpContext.User);
            rosterViewModel.AllUsers = userManager.Users.ToList();
            List<Holiday> holidays = await context.Holiday.Where(e => e.IsActive == true && e.Year == DateTime.Now.Year).ToListAsync();

            rosterViewModel.Dates = GetDates(Year.Year, Month.Month);

            rosterViewModel.RosterShifts = context.RosterShifts.Where(e => e.IsActive).ToList();
            rosterViewModel.Categories = context.Category.Where(e => e.CategoryName != Constants.ADMIN && e.IsActive).ToList();
            if (User.IsInRole(Constants.TL))
            {
                rosterViewModel.Users = userManager.Users.Where(e => e.CategoryId == currentUser.CategoryId && e.Id != currentUser.Id).ToList();
            }
            else if(User.IsInRole(Constants.ADMIN))
            {
                rosterViewModel.Users = userManager.Users.Where(e => !e.IsRegularShift).ToList();
            }
            else
            {
                rosterViewModel.Users = userManager.Users.Where(e => e.Id == currentUser.Id).ToList();
            }

            foreach (ApplicationUser user in rosterViewModel.Users)
            {
                List<Roster> roster = context.Rosters.Include(e => e.RosterShift).Where(e => e.UserId == user.Id && e.Date.Month == Month.Month).ToList();
                if (roster.Count <= 0)
                {
                    foreach (var date in rosterViewModel.Dates)
                    {
                        var regularShift = rosterViewModel.RosterShifts.Where(e => e.RosterShiftName == Constants.R).FirstOrDefault();
                        if ((date.DayOfWeek.ToString() == Constants.SATURDAY || date.DayOfWeek.ToString() == Constants.SUNDAY) && user.IsRegularShift == true)
                        {
                            regularShift = rosterViewModel.RosterShifts.Where(e => e.RosterShiftName == Constants.OFF).FirstOrDefault();
                        }
                        Holiday holiday = holidays.Find(e => e.Date == date);
                        if (holiday != null && user.IsRegularShift == true)
                        {
                            context.Rosters.Add(new Roster
                            {
                                Date = date,
                                CreatedOn = DateTime.Now,
                                IsActive = true,
                                IsSelected = false,
                                User = user,
                                RosterShift = rosterViewModel.RosterShifts.Where(e => e.RosterShiftName == Constants.OFF).FirstOrDefault()
                            });
                        }
                        else
                        {
                            context.Rosters.Add(new Roster
                            {
                                Date = date,
                                CreatedOn = DateTime.Now,
                                IsActive = true,
                                IsSelected = false,
                                User = user,
                                RosterShift = user.IsRegularShift ? regularShift : null
                            });
                        }                        
                    }
                    context.SaveChanges();
                    roster = context.Rosters.Include(e => e.RosterShift).Where(e => e.UserId == user.Id && e.Date.Month == Month.Month).ToList();
                    roster.ForEach(item =>
                    {
                        item.RosterShifts = rosterViewModel.RosterShifts.Select(e => new SelectListItem
                        {
                            Text = e.RosterShiftName,
                            Value = e.RosterShiftId.ToString(),
                            Selected = item.RosterShift != null ? (item.RosterShift.RosterShiftId == e.RosterShiftId ? true : false) : false
                        }).ToList();
                        item.RosterShifts.Add(new SelectListItem { Text = "NA", Value = string.Empty, Selected = string.IsNullOrEmpty(item.RosterShift?.RosterShiftId.ToString()) ? true : false });
                    });
                    rosterViewModel.Rosters.AddRange(roster);
                }
                else
                {
                    roster.ForEach(item =>
                    {
                        item.RosterShifts = rosterViewModel.RosterShifts.Select(e => new SelectListItem
                        {
                            Text = e.RosterShiftName,
                            Value = e.RosterShiftId.ToString(),
                            Selected = item.RosterShift != null ? (item.RosterShift.RosterShiftId == e.RosterShiftId ? true : false) : false
                        }).ToList();
                        item.RosterShifts.Add(new SelectListItem { Text = "NA", Value = string.Empty, Selected = string.IsNullOrEmpty(item.RosterShift?.RosterShiftId.ToString()) ? true : false });
                    });
                    rosterViewModel.Rosters.AddRange(roster);
                }
            }
            await Task.Run(async () => { await GetRosterViewModel(Year, Month); });
            return rosterViewModel;
        }
        [HttpPost]
        [DisableRequestSizeLimit]
        public ActionResult Edit(RosterViewModel rosterViewModel)
        {
            try
            {

                if (rosterViewModel.Rosters != null)
                {
                    foreach (var roster in rosterViewModel.Rosters)
                    {
                        Roster _roster = context.Rosters.Include(e => e.RosterShift).Where(e => e.RosterId == roster.RosterId).FirstOrDefault();

                        if (_roster.RosterShift?.RosterShiftId.ToString() != roster.RosterShiftsId)
                        {
                            if (string.IsNullOrEmpty(roster.RosterShiftsId))
                            {
                                _roster.RosterShift = null;
                            }
                            else
                            {
                                _roster.RosterShift = context.RosterShifts.Where(e => e.RosterShiftId.ToString() == roster.RosterShiftsId).FirstOrDefault();
                            }
                        }
                        context.Rosters.Update(_roster);
                    }
                    context.SaveChanges();
                }
            }
            catch(System.Exception ex)
            {
                throw ex;
            }
            // await GetIndexRosterViewModel(true);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public FileResult Export(string GridHtml)
        {
            return File(Encoding.ASCII.GetBytes(GridHtml), "application/vnd.ms-excel", "ShiftRoster.xls");
        }

        [HttpPost]
        public async Task<ActionResult> Email([FromBody]string data, string month)
        {
            DateTime date = DateTime.Now;
            if(month == "C")
                date = DateTime.Now;
            else if( month == "P")
                date = DateTime.Now.AddMonths(-1);
            else if (month == "N")
                date = DateTime.Now.AddMonths(1);

            string mail = configuration.GetSection("EmailConfiguration:AllInBangalore").Value;
            string subject = "Shift Roster - "+ date.ToString("MMMM")+ " "+ date.Year + " V" + GetversionNo(month);
            await emailService.SendEmailAsync(mail, subject,"", data);
            return Ok();
        }
        private string GetversionNo(string monthString)
        {
            string roster = "";
            string version = "";
            DateTime date = DateTime.Now;
            if (monthString == "C")
            {
                version = configuration.GetSection("EmailConfiguration:RosterCurrentVersion").Value;
                if (string.IsNullOrEmpty(version))
                {
                    roster = "1";
                    configuration["EmailConfiguration:RosterCurrentVersion"] = date.Month + "/" + roster;
                }
                else
                {
                    string[] list = version.Split("/");
                    if (!string.IsNullOrEmpty(list[0]) && !string.IsNullOrEmpty(list[1]))
                    {
                        if (date.Month.ToString() == list[0])
                        {
                            roster = (Convert.ToInt32(list[1]) + 1).ToString();
                            configuration["EmailConfiguration:RosterCurrentVersion"] = DateTime.Now.Month + "/" + roster;
                        }
                    }
                    else
                    {
                        roster = "1";
                        configuration["EmailConfiguration:RosterCurrentVersion"] = date.Month + "/" + roster;
                    }
                }
            }
            else if (monthString == "P")
            {
                version = configuration.GetSection("EmailConfiguration:RosterPreviousVersion").Value;
                date = DateTime.Now.AddMonths(-1);
                if (string.IsNullOrEmpty(version))
                {
                    roster = "1";
                    configuration["EmailConfiguration:RosterPreviousVersion"] = date.Month + "/" + roster;
                }
                else
                {
                    string[] list = version.Split("/");
                    if (!string.IsNullOrEmpty(list[0]) && !string.IsNullOrEmpty(list[1]))
                    {
                        if (date.Month.ToString() == list[0])
                        {
                            roster = (Convert.ToInt32(list[1]) + 1).ToString();
                            configuration["EmailConfiguration:RosterPreviousVersion"] = DateTime.Now.Month + "/" + roster;
                        }
                    }
                    else
                    {
                        roster = "1";
                        configuration["EmailConfiguration:RosterPreviousVersion"] = date.Month + "/" + roster;
                    }
                }
            }
            else if (monthString == "N")
            {
                version = configuration.GetSection("EmailConfiguration:RosterNextVersion").Value;
                date = DateTime.Now.AddMonths(1);
                if (string.IsNullOrEmpty(version))
                {
                    roster = "1";
                    configuration["EmailConfiguration:RosterNextVersion"] = date.Month + "/" + roster;
                }
                else
                {
                    string[] list = version.Split("/");
                    if (!string.IsNullOrEmpty(list[0]) && !string.IsNullOrEmpty(list[1]))
                    {
                        if (date.Month.ToString() == list[0])
                        {
                            roster = (Convert.ToInt32(list[1]) + 1).ToString();
                            configuration["EmailConfiguration:RosterNextVersion"] = DateTime.Now.Month + "/" + roster;
                        }
                    }
                    else
                    {
                        roster = "1";
                        configuration["EmailConfiguration:RosterNextVersion"] = date.Month + "/" + roster;
                    }
                }
            }
            
            return roster;
        }
        private string GetVendorVersionNo()
        {
            string roster = "";
            string version = configuration.GetSection("EmailConfiguration:RosterVendorVersion").Value;
            if (string.IsNullOrEmpty(version))
            {
                roster = "1";
                configuration["EmailConfiguration:RosterVendorVersion"] = DateTime.Now.Month + "/" + roster;
                string temp = configuration.GetSection("EmailConfiguration:RosterVendorVersion").Value;
            }
            else
            {
                string[] list = version.Split("/");
                if (!string.IsNullOrEmpty(list[0]) && !string.IsNullOrEmpty(list[0]))
                {
                    if (DateTime.Now.Month.ToString() == list[0])
                    {
                        roster = (Convert.ToInt32(list[1]) + 1).ToString();
                        configuration["EmailConfiguration:RosterVendorVersion"] = DateTime.Now.Month + "/" + roster;
                    }
                }
                else
                {
                    roster = "1";
                    configuration["EmailConfiguration:RosterVendorVersion"] = DateTime.Now.Month + "/" + roster;
                }
            }
            return roster;
        }
        [HttpPost]
        public async Task<ActionResult> EmailToVendor([FromBody]string data)
        {
            string mail = configuration.GetSection("EmailConfiguration:RosterToVendor").Value;
            string subject = "Shift Roster - " + DateTime.Now.ToString("MMMM") + " " + DateTime.Now.Year + " V" + GetVendorVersionNo();
            await emailService.SendEmailAsync(mail, subject,"", data);
            return Ok();
        }
        [HttpGet]
        public async Task<ActionResult> HtmlVendor(string start, string end)
        {
            if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
            {
                string roster = Html(Convert.ToDateTime(start),Convert.ToDateTime(end));
                return Json(roster);
            }
            else
                return null;            
        }
        public string Html(DateTime startDate, DateTime endDate)
        {
            var sb = new StringBuilder();
            List<Roster> Rosters = context.Rosters.Include(e=>e.RosterShift).Where(e => e.Date >= startDate.Date && e.Date <= endDate.Date).ToList();
            List<DateTime> Dates = Rosters.GroupBy(e=>e.Date).Select(e=>e.First().Date).OrderBy(e=>e.Date).Distinct().ToList();

            sb.Append("<div class='row'><div class='col-md-12'><div class='table-responsive' style='overflow:auto;max-height: 500px'>");
            sb.Append("<div id='dataTables-example_wrapper' class='dataTables_wrapper form-inline' role='grid'><table class='table table-striped table-bordered table-hover dataTable no-footer' aria-describedby='dataTables-example_info'><thead>");
            sb.Append("<tr role='row'><th class='sorting_asc' tabindex='0' aria-controls='dataTables-example' rowspan = '1' colspan = '1' aria-sort='ascending' style = 'width: 151px;'> Date</th>");

            foreach (var item in Dates)
            {
                if (item.DayOfWeek.ToString() == Constants.SATURDAY || item.DayOfWeek.ToString() == Constants.SUNDAY)
                {
                    sb.Append("<th class='sorting_asc' tabindex='0' aria-controls='dataTables-example' rowspan='1' colspan='1' aria-sort='ascending' style='width: 151px;'><font color='red'>" + item.Day + "</font></th>");
                }
                else
                {
                    sb.Append("<th class='sorting_asc' tabindex='0' aria-controls='dataTables-example' rowspan='1' colspan='1' aria-sort='ascending' style='width: 151px;'>" + item.Day + "</th>");
                }
            }
            sb.Append("</tr><tr role='row'><th class='sorting_asc' tabindex='0' aria-controls='dataTables-example' rowspan='1' colspan='1' aria-sort='ascending' style='width: 151px;'>Day</th>");
            foreach (var item in Dates)
            {
                if (item.DayOfWeek.ToString() == Constants.SATURDAY || item.DayOfWeek.ToString() == Constants.SUNDAY)
                {
                    sb.Append("<th class='sorting_asc' tabindex='0' aria-controls='dataTables-example' rowspan='1' colspan='1' aria-sort='ascending' style='width: 151px;'><font color='red'>" + item.DayOfWeek + "</font></th>");
                }
                else
                {
                    sb.Append("<th class='sorting_asc' tabindex='0' aria-controls='dataTables-example' rowspan='1' colspan='1' aria-sort='ascending' style='width: 151px;'>" + item.DayOfWeek + "</th>");
                }
            }
            sb.Append("</tr></thead><tbody>");

            foreach (var catagory in context.Category.Where(e=>e.IsActive).ToList())
            {
                var users = userManager.Users.Where(e => e.CategoryId == catagory.CategoryId && e.IsCab).ToList();
                if (users.Count > 0)
                {
                    sb.Append("<tr style ='background-color:cadetblue'><td></td></tr>");
                    foreach (var user in users)
                    {
                        sb.Append("<tr class='gradeA even'><td class='sorting_1'>" + user.Name + "</td>");
                        {
                            var roster = Rosters.Where(e => e.UserId == user.Id).ToList();
                            foreach (var dateTime in Dates)
                            {
                                var item = roster.Where(e => e.Date.Date == dateTime.Date).FirstOrDefault();

                                if (item.RosterShift == null)
                                {
                                    sb.Append("<td><label>-</label></td>");
                                }
                                else
                                {
                                    if (item.RosterShift.RosterShiftName == Constants.OFF)
                                    {
                                        sb.Append("<td style='background-color:gray'><label>" + item.RosterShift?.RosterShiftName + "</label></td>");
                                    }
                                    else if (item.RosterShift.RosterShiftName == Constants.PL || item.RosterShift.RosterShiftName == Constants.CL || item.RosterShift.RosterShiftName == Constants.SL)
                                    {
                                        sb.Append("<td style='background-color:goldenrod'><label>" + item.RosterShift?.RosterShiftName + "</label></td>");
                                    }
                                    else
                                    {
                                        sb.Append("<td><label>" + item.RosterShift?.RosterShiftName + "</label></td>");
                                    }
                                }
                            }
                        }
                    }
                }
            }



            sb.Append("</tr></tbody></table></div</div></div</div>");
            return sb.ToString();
        }
        public IActionResult Report()
        {
            RosterViewModel model = _rosterViewModel;
            StringBuilder sbRtn = new StringBuilder();
            sbRtn.AppendLine(GetHeader(model).ToString());

            foreach (var catagory in model.Categories)
            {
                var users = model.AllUsers.Where(e => e.CategoryId == catagory.CategoryId).ToList();
                if (users.Count > 0)
                {
                    sbRtn.Append(catagory.CategoryName);
                    sbRtn.Append(Environment.NewLine);
                    for (int i = 0; i < users.Count; i++)
                    {
                        sbRtn.Append(users[i].Name);
                        {
                            var roster = model.Rosters.Where(e => e.UserId == users[i].Id).ToList();
                            foreach (var dateTime in model.Dates)
                            {
                                var item = roster.Where(e => e.Date.Day == dateTime.Day).FirstOrDefault();
                                var index = model.Rosters.IndexOf(item);
                                if (model.Rosters[index].RosterShift == null)
                                {
                                    sbRtn.Append(",-");
                                }
                                else
                                {
                                    sbRtn.Append("," + model.Rosters[index].RosterShift?.RosterShiftName);
                                }
                            }
                            foreach (var item in model.RosterShifts)
                            {
                                sbRtn.Append("," + roster.Where(e => e.RosterShift != null && e.RosterShift.RosterShiftId == item.RosterShiftId).Count());
                            }
                            sbRtn.Append("," + model.Dates.Count());
                            sbRtn.Append("," + roster.Where(e => e.RosterShift != null && (e.RosterShift.RosterShiftName == Constants.A ||
                                                    e.RosterShift.RosterShiftName == Constants.B || e.RosterShift.RosterShiftName == Constants.C || e.RosterShift.RosterShiftName == Constants.R)).Count());

                            sbRtn.Append("," + users[i].IsRegularShift);
                            sbRtn.Append(Environment.NewLine);
                        }
                    }
                }
            }
            sbRtn.Append(Environment.NewLine);
            foreach (var shift in model.RosterShifts.Where(e => e.RosterShiftName == Constants.A || e.RosterShiftName == Constants.B || e.RosterShiftName == Constants.C
                                           || e.RosterShiftName == Constants.R))
            {                
                sbRtn.Append(shift.RosterShiftName);

                foreach (var dateTime in model.Dates)
                {
                    sbRtn.Append("," + model.Rosters.Where(e => e.RosterShift != null && (e.RosterShift.RosterShiftId == shift.RosterShiftId &&
                     e.Date.Day == dateTime.Day)).Count());
                }
                sbRtn.Append(Environment.NewLine);
            }

            foreach (var catagory in model.Categories)
            {
                sbRtn.Append(Environment.NewLine);
                sbRtn.Append(catagory.CategoryName);
                sbRtn.Append(Environment.NewLine);
                foreach (var shift in model.RosterShifts.Where(e => e.RosterShiftName == Constants.A || e.RosterShiftName == Constants.B || e.RosterShiftName == Constants.C
                                                || e.RosterShiftName == Constants.R))
                {
                    sbRtn.Append(@shift.RosterShiftName);
                    foreach (var dateTime in model.Dates)
                    {
                        sbRtn.Append("," + model.Rosters.Where(e => e.RosterShift != null && e.RosterShift.RosterShiftId == shift.RosterShiftId &&
                                                  e.Date.Day == dateTime.Day && e.User.CategoryId == catagory.CategoryId).Count());
                    }
                    sbRtn.Append(Environment.NewLine);
                }
                
            }

            return File(System.Text.Encoding.ASCII.GetBytes(sbRtn.ToString()), "text/csv", "Shift Rosters.csv");
        }

        private async Task GetRosterViewModel(DateTime year, DateTime month)
        {
            var currentUser = await userManager.GetUserAsync(HttpContext.User);
            _rosterViewModel.Rosters.Clear();
            _rosterViewModel.Dates = GetDates(year.Year, month.Month);
            _rosterViewModel.RosterShifts = context.RosterShifts.Where(e => e.IsActive).ToList();
            _rosterViewModel.Categories = context.Category.Where(e => e.CategoryName != Constants.ADMIN && e.IsActive).ToList();
            _rosterViewModel.AllUsers = userManager.Users.ToList();
            foreach (ApplicationUser user in _rosterViewModel.AllUsers)
            {
                List<Roster> roster = context.Rosters.Include(e => e.RosterShift).Where(e => e.UserId == user.Id && e.Date.Month == DateTime.Now.Month).ToList();
                List<Holiday> holidays = await context.Holiday.Where(e => e.IsActive == true && e.Year == DateTime.Now.Year).ToListAsync();
                if (roster.Count <= 0)
                {
                    foreach (var date in _rosterViewModel.Dates)
                    {
                        var regularShift = _rosterViewModel.RosterShifts.Where(e => e.RosterShiftName == Constants.R).FirstOrDefault();
                        if (date.DayOfWeek.ToString() == Constants.SATURDAY || date.DayOfWeek.ToString() == Constants.SUNDAY)
                        {
                            regularShift = _rosterViewModel.RosterShifts.Where(e => e.RosterShiftName == Constants.OFF).FirstOrDefault();
                        }
                        Holiday holiday = holidays.Find(e => e.Date == date);
                        if (holiday != null)
                        {
                            context.Rosters.Add(new Roster
                            {
                                Date = date,
                                CreatedOn = DateTime.Now,
                                IsActive = true,
                                IsSelected = false,
                                User = user,
                                RosterShift = _rosterViewModel.RosterShifts.Where(e => e.RosterShiftName == Constants.OFF).FirstOrDefault()
                            });
                        }
                        else
                        {
                            context.Rosters.Add(new Roster
                            {
                                Date = date,
                                CreatedOn = DateTime.Now,
                                IsActive = true,
                                IsSelected = false,
                                User = user,
                                RosterShift = user.IsRegularShift ? regularShift : null
                            });
                        }
                    }
                    context.SaveChanges();
                    _rosterViewModel.Rosters.AddRange(context.Rosters.Include(e => e.RosterShift).Where(e => e.UserId == user.Id && e.Date.Month == DateTime.Now.Month).ToList());
                }
                else
                {
                    _rosterViewModel.Rosters.AddRange(roster);
                }
            }
        }
        private IEnumerable<DateTime> AllDatesBetween(DateTime start, DateTime end)
        {
            for (var day = start.Date; day <= end; day = day.AddDays(1))
                yield return day;
        }
        private List<DateTime> GetDates(int year, int month)
        {
            return Enumerable.Range(1, DateTime.DaysInMonth(year, month))  // Days: 1, 2 ... 31 etc.
                             .Select(day => new DateTime(year, month, day)) // Map each day to a date
                             .ToList(); // Load dates into a list
        }
        private StringBuilder GetHeader(RosterViewModel model)
        {
            StringBuilder header = new StringBuilder();
            header.Append("Date");
            foreach (var date in model.Dates)
            {
                header.Append(",");
                header.Append(date.Date.ToShortDateString());
            }
            header.Append(Environment.NewLine);
            header.Append("Day");
            foreach (var date in model.Dates)
            {
                header.Append(",");
                header.Append(date.DayOfWeek);
            }
            foreach (var item in model.RosterShifts)
            {
                header.Append(",");
                header.Append(item.RosterShiftName);
            }
            header.Append(",Total");
            header.Append(",Total Days Worked");
            header.Append(", Cab");
            header.Append(Environment.NewLine);
            return header;
        }
    }
}