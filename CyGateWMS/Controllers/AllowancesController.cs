using CyGateWMS.Models;
using CyGateWMS.Services;
using CyGateWMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CyGateWMS.Controllers
{
    [Authorize]
    public class AllowancesController : Controller
    {
        private readonly CygateWMSContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailService emailService;
        public static AllowancesFilterViewModel _viewModel = new AllowancesFilterViewModel();
        private readonly IConfiguration Configuration;
        public static List<string> AllowanceDates = new List<string>();



        public AllowancesController(CygateWMSContext context, UserManager<ApplicationUser> userManager, IEmailService emailService, IConfiguration Configuration)
        {
            this.userManager = userManager;
            _context = context;
            this.emailService = emailService;
            this.Configuration = Configuration;
        }

        // GET: Allowances
        public async Task<IActionResult> Index(Boolean IsFromFilter=false)
        {
            if (IsFromFilter)
                return await Filter(_viewModel);
            AllowanceDates.Clear();
            AllowancesFilterViewModel viewModel = await BuildAllwancesFilterViewModel();            
            _viewModel = viewModel;
            _viewModel.IsFromFilter = false;           
            return View(viewModel);
        }
        public async Task<IActionResult> Pending()
        {            
            AllowancesFilterViewModel viewModel = await BuildAllwancesFilterViewModel();
            viewModel.AllowanceViewModelList.RemoveAll(e => e.Status != ApprovedStatus.PENDING);
        
            _viewModel = viewModel;
            ViewBag.IsPending = true;
            return View("Index", viewModel);
        }
        public async Task<IActionResult> Approved()
        {
            AllowancesFilterViewModel viewModel = await BuildAllwancesFilterViewModel();
            viewModel.AllowanceViewModelList.RemoveAll(e => e.Status != ApprovedStatus.APPROVED);

            _viewModel = viewModel;
            _viewModel.IsFromFilter = false;
            return View("Index", viewModel);
        }
        public async Task<IActionResult> Approve(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var allowance = await _context.Allowance
                .Include(a => a.AllowanceType).Include(e => e.CreatedBy).Include(e => e.AssignedCategory)
                .FirstOrDefaultAsync(m => m.AllowanceID == id);
            if (allowance == null)
            {
                return NotFound();
            }
            allowance.ApprovedStatus = ApprovedStatus.APPROVED;
            _context.Allowance.Update(allowance);
            _context.SaveChanges();
            await Task.Run(() =>  SendMail(allowance));
            if(_viewModel.IsFromFilter)
            {
                return await Filter(_viewModel);
            }
            return RedirectToAction(nameof(Index));
        }
        private string GetUserAllowanceHtml(Allowance allowance, string name)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><body><p>Dear " + name + ",</p>");

            sb.Append("<p> Your allowance amount "+ allowance.Price+ " has been "+ allowance.ApprovedStatus+"</p>");

            sb.Append("<hr><p><font color='red'> ***This is an automatically generated message, please do not reply to this message * **</font></p></body></html>");
            return sb.ToString();
        }
        private string GetTLAllowanceHtml(Allowance allowance, string tlName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><body><p>Dear " + tlName + ",</p>");

            sb.Append("<p> Allowance amount " + allowance.Price + " for "+ allowance.CreatedBy?.Name + " has been " + allowance.ApprovedStatus + " by you.</p>");

            sb.Append("<hr><p><font color='red'> ***This is an automatically generated message, please do not reply to this message * **</font></p></body></html>");
            return sb.ToString();
        }

        private string GetProcurementAllowanceHtml(Allowance allowance, string tlName)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><body><p>Dear " + tlName + ",</p>");

            sb.Append("<p> Allowance amount " + allowance.Price + " has been applied by " + allowance.CreatedBy?.Name + "</p>");

            sb.Append("<hr><p><font color='red'> ***This is an automatically generated message, please do not reply to this message * **</font></p></body></html>");
            return sb.ToString();
        }
        private string GetTeamAllowanceHtml(string depName, string name)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><body><p>Dear " + name + ",</p>");

            sb.Append("<p>"+ depName + " team allowance all has been approved.</p>");

            sb.Append("<hr><p><font color='red'> ***This is an automatically generated message, please do not reply to this message * **</font></p></body></html>");
            return sb.ToString();
        }
        private async Task SendMail(Allowance allowance = null)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            var users = _context.User.Where(e => e.CategoryId == user.CategoryId).ToList();

            if(allowance !=null)
            {
                await emailService.SendEmailAsync(allowance.CreatedBy?.Email, "Allowance " + allowance.ApprovedStatus,
                   GetUserAllowanceHtml(allowance, allowance.CreatedBy?.Name));

                await emailService.SendEmailAsync(user.Email, "Allowance " + allowance.ApprovedStatus,
                                GetTLAllowanceHtml(allowance, user.Name));
            }            

            List<Allowance> allowancesAll = new List<Allowance>();
            foreach (var item in users)
            {
                var allowances = _context.Allowance.Where(e => e.CreatedById == item.Id && e.ApprovedStatus.ToString() == Constants.PENDING).Include(a => a.AllowanceType).Include(e => e.AssignedBy)
                    .Include(e => e.AssignedCategory).Include(e => e.CreatedBy).ToList();
                allowancesAll.AddRange(allowances);
            }
            if (allowancesAll.Where(e => e.ApprovedStatus.ToString() == Constants.PENDING).ToList().Count == 0)
            {
                var financeTeam = userManager.Users.Where(e => e.Category.CategoryName == Constants.FINANCE);
                foreach (var finance in financeTeam)
                {
                    await emailService.SendEmailAsync(finance.Email, "Allowance Approved",
                        GetTeamAllowanceHtml(user.Category?.CategoryName, finance.Name));
                }
            }
        }
        public async Task<IActionResult> ApproveAll()
        {
            var user = await userManager.GetUserAsync(HttpContext.User);

            var users = _context.User.Where(e => e.CategoryId == user.CategoryId).ToList();
            foreach(var item in users)
            {
                var allowances = _context.Allowance.Where(e => e.CreatedById == item.Id && e.ApprovedStatus.ToString() == Constants.PENDING).Include(a => a.AllowanceType).Include(e => e.AssignedBy)
                    .Include(e => e.AssignedCategory).Include(e => e.CreatedBy).ToList();
                foreach(var allowance in allowances)
                {
                    allowance.ApprovedStatus = ApprovedStatus.APPROVED;
                    await Task.Run(() => SendMail(allowance));
                }
            }
            _context.SaveChanges();
            await Task.Run(() => SendMail());
            if (_viewModel.IsFromFilter)
            {
                return await Filter(_viewModel);
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<AllowancesFilterViewModel> BuildAllwancesFilterViewModel()
        {
            AllowancesFilterViewModel viewModel = new AllowancesFilterViewModel();
            var user = await userManager.GetUserAsync(HttpContext.User);
            var cygateWMSContext = _context.Allowance.Include(a => a.AllowanceType).Include(e => e.AssignedBy).Include(e => e.AssignedCategory).Include(e => e.CreatedBy)
                .Where(e=>e.Month.Month == DateTime.Today.AddMonths(-1).Month).ToList();
                        
            viewModel.AllowanceViewModel = await GetCreateViewModel();
            viewModel.AllowanceViewModel.Categories = _context.Category.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.CategoryName,
                Value = e.CategoryId.ToString(),
                Selected = e.CategoryId.ToString() == viewModel.AllowanceViewModel.CategoryID ? true : false
            }).ToList();
            viewModel.AllowanceViewModel.Users = userManager.Users.Where(e=>e.CategoryId == user.CategoryId).Select(e => new SelectListItem
            {
                Text = e.Name,
                Value = e.Id,
                Selected = e.Id == viewModel.AllowanceViewModel.UserID ? true : false

            }).ToList();
            if(User.IsInRole(Constants.ADMIN))
            {
                viewModel.AllowanceViewModel.Users = userManager.Users.Select(e => new SelectListItem
                {
                    Text = e.Name,
                    Value = e.Id,
                    Selected = e.Id == viewModel.AllowanceViewModel.UserID ? true : false

                }).ToList();
            }
            viewModel.AllowanceViewModel.ApprovedStatus = Enum.GetValues(typeof(ApprovedStatus)).Cast<ApprovedStatus>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString(),
                Selected = ((int)v).ToString() == viewModel.AllowanceViewModel.ApprovedStatusId ? true : false
            }).ToList();
            viewModel.AllowanceViewModel.Categories.Add(new SelectListItem { Text = "Select Category", Value = string.Empty, Selected = string.IsNullOrEmpty(viewModel.AllowanceViewModel?.CategoryID) ? true : false });
            viewModel.AllowanceViewModel.Users.Add(new SelectListItem { Text = "Select User", Value = string.Empty, Selected = string.IsNullOrEmpty(viewModel.AllowanceViewModel?.UserID) ? true : false });
            viewModel.AllowanceViewModel.ApprovedStatus.Add(new SelectListItem { Text = "Select Status", Value = string.Empty, Selected = string.IsNullOrEmpty(viewModel.AllowanceViewModel?.ApprovedStatusId) ? true : false });

            if (User.IsInRole(Constants.ADMIN) || User.IsInRole(Constants.FINANCE))
            {
                viewModel.Users = _context.Users.Where(e => e.IsActive).ToList();
                viewModel.AllowanceViewModelList = GetAllowanceViewModel(cygateWMSContext).OrderBy(e => e.ApprovedStatus).ThenBy(e => e.CreatedOn).ToList();
            }
            else if (User.IsInRole(Constants.TL))
            {
                viewModel.AllowanceViewModelList = GetAllowanceViewModel(cygateWMSContext.Where(e => e.AssignedCategoryId == user.CategoryId).OrderBy(e => e.ApprovedStatus).ThenBy(e => e.CreatedOn).ToList());
            }
            else if (User.IsInRole(Constants.USER))
            {
                viewModel.AllowanceViewModelList = GetAllowanceViewModel(cygateWMSContext.Where(e => e.CreatedById == user.Id).OrderBy(e => e.ApprovedStatus).ThenBy(e => e.CreatedOn).ToList());
            }
            return viewModel;
        }        
        // GET: Allowances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var allowance = await _context.Allowance
                .Include(a => a.AllowanceType).Include(e => e.CreatedBy).Include(e => e.AssignedCategory)
                .FirstOrDefaultAsync(m => m.AllowanceID == id);
            if (allowance == null)
            {
                return NotFound();
            }
            AllowanceViewModel viewModel = GetAllowance(allowance);
            if (_viewModel.IsFromFilter)
            {
                viewModel.IsFromFilter = _viewModel.IsFromFilter;
            }
            return View(viewModel);
        }
        public IActionResult Download()
        {
            StringBuilder sbRtn = new StringBuilder();


            // If you want headers for your file
            var header = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\""
                                       , "Serial No"
                                       , "Comments"
                                       , "Price"
                                       , "Number Of Days"
                                       , "Allowance Type"
                                       , "Status"
                                       , "Name"
                                       , "Created On"
                                       , "Allownance Dates"
                                      );
            sbRtn.AppendLine(header);
            int count = 0;
            _viewModel.AllowanceViewModelList.ForEach(data =>
            {
                var queryResults = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\""
                                             , ++count
                                             , data.Comments
                                             , data.Price
                                             , data.NumberOfDays
                                             , data.AllowanceType?.AllowanceTypeName
                                             , data.Status
                                             , data.CreatedBy?.Name
                                             , data.AllowanceDate
                                             , GetListOfDates(data.AllownanceDates));
                sbRtn.AppendLine(queryResults);
            });


            return File(System.Text.Encoding.ASCII.GetBytes(sbRtn.ToString()), "text/csv", "Allowances.csv");
        }
        public IActionResult FinanceDownload()
        {
            StringBuilder sbRtn = new StringBuilder();
            var header = "EMPLOYEEID,NAME,DEPARTMENT";
            var types = _viewModel.AllowanceViewModelList.GroupBy(e => e.AllowanceType.AllowanceTypeId).Select(g => new { id = g.First().AllowanceType.AllowanceTypeId, name = g.First().AllowanceType.AllowanceTypeName }).OrderBy(e => e.name).ToList();
            foreach (var type in types)
            {
                header += ",No Of " + type.name;
            }
            foreach (var type in types)
            {
                header += "," + type.name;
            }
            header += ",TOTAL";
            sbRtn.AppendLine(header);
            foreach (var user in _viewModel.Users.OrderBy(e=>e.EmployeeId))
            {
                List<AllowanceViewModel> allowances = _viewModel.AllowanceViewModelList.Where(e => e.UserID == user.Id && e.Status.ToString()==Constants.APPROVED).ToList();
                if (allowances.Count() > 0)
                {
                    var groupByType = allowances.GroupBy(e => e.AllowanceTypeId).Select(g => new { name = g.First().AllowanceType.AllowanceTypeName, price = g.Sum(a => a.Price) }).ToList();
                    var queryResults = user.EmployeeId + "," + user.Name.ToUpper();
                    queryResults += "," + _viewModel.AllowanceViewModel.Categories.Where(e => e.Value == user.CategoryId.ToString()).FirstOrDefault().Text;
                    foreach (var type in types)
                    {
                        List<AllowanceViewModel> temp = allowances.Where(e => e.AllowanceType.AllowanceTypeId == type.id && e.Status.ToString() == Constants.APPROVED).ToList();
                        if (temp?.Count>0)
                            queryResults += "," + temp.Sum(e=>e.AllownanceDates.Count());
                        else
                            queryResults += "," + string.Empty;
                    }
                    foreach (var type in types)
                    {
                        queryResults += "," + allowances.Where(e => e.AllowanceType.AllowanceTypeId == type.id && e.Status.ToString() == Constants.APPROVED).Sum(e => e.Price);
                    }
                    queryResults += "," + allowances.Where(e => e.UserID == user.Id && e.Status.ToString() == Constants.APPROVED).Sum(e => e.Price);
                    sbRtn.AppendLine(queryResults);
                }                
            }
            return File(System.Text.Encoding.ASCII.GetBytes(sbRtn.ToString()), "text/csv", "Allowances.csv");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Filter(AllowancesFilterViewModel viewModel=null)
        {
            if (viewModel == null)
                viewModel = _viewModel;
            var where = GetWhere(viewModel.AllowanceViewModel);
            var user = await userManager.GetUserAsync(HttpContext.User);
            List<Allowance> Allowance = null;
            if (User.IsInRole(Constants.ADMIN) || User.IsInRole(Constants.FINANCE))
            {
                viewModel.Users = _context.Users.Where(e => e.IsActive).ToList();
                Allowance = _context.Allowance.Where(where).Include(a => a.AllowanceType).Include(e => e.AssignedBy).Include(e => e.AssignedCategory).Include(e => e.CreatedBy).ToList();
            }
            else if (User.IsInRole(Constants.TL))
            {
                Allowance = _context.Allowance.Where(where).Include(a => a.AllowanceType).Include(e => e.AssignedBy).Include(e => e.AssignedCategory).Include(e => e.CreatedBy).Where(e=> e.AssignedCategoryId == user.CategoryId).ToList();
            }
            else
            {
                Allowance = _context.Allowance.Where(where).Include(a => a.AllowanceType).Include(e => e.AssignedBy).Include(e => e.AssignedCategory).Include(e => e.CreatedBy).
                Where(e => e.CreatedById == user.Id).ToList();
            }
            
            viewModel.AllowanceViewModelList = GetAllowanceViewModel(Allowance);
            //viewModel.AllowanceViewModel = new AllowanceViewModel();
            viewModel.AllowanceViewModel.Categories = _context.Category.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.CategoryName,
                Value = e.CategoryId.ToString(),
                Selected = e.CategoryId.ToString() == viewModel.AllowanceViewModel.CategoryID ? true : false
            }).ToList();
            viewModel.AllowanceViewModel.Users = userManager.Users.Select(e => new SelectListItem
            {
                Text = e.Name,
                Value = e.Id,
                Selected = e.Id == viewModel.AllowanceViewModel.UserID ? true : false

            }).ToList();
            viewModel.AllowanceViewModel.ApprovedStatus = Enum.GetValues(typeof(ApprovedStatus)).Cast<ApprovedStatus>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString(),
                Selected = ((int)v).ToString() == viewModel.AllowanceViewModel.ApprovedStatusId ? true : false
            }).ToList();
            viewModel.AllowanceViewModel.AllowanceTypes = _context.AllowanceType.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.AllowanceTypeName,
                Value = e.AllowanceTypeId.ToString()
            }).ToList();
            viewModel.AllowanceViewModel.Categories.Add(new SelectListItem { Text = "Select Category", Value = string.Empty, Selected = string.IsNullOrEmpty(viewModel.AllowanceViewModel?.CategoryID) ? true : false });
            viewModel.AllowanceViewModel.Users.Add(new SelectListItem { Text = "Select User", Value = string.Empty, Selected = string.IsNullOrEmpty(viewModel.AllowanceViewModel?.UserID) ? true : false });
            viewModel.AllowanceViewModel.ApprovedStatus.Add(new SelectListItem { Text = "Select Status", Value = string.Empty, Selected = string.IsNullOrEmpty(viewModel.AllowanceViewModel?.ApprovedStatusId) ? true : false });
            viewModel.AllowanceViewModel.AllowanceTypes.Add(new SelectListItem { Text = "Select Allowance Type", Value = string.Empty, Selected = string.IsNullOrEmpty(viewModel.AllowanceViewModel?.AllowanceTypeId) ? true : false });
            _viewModel = viewModel;
            _viewModel.IsFromFilter = true;
            return View("Index", viewModel);
        }

        public async Task<AllowancesFilterViewModel> GetFilter(AllowancesFilterViewModel viewModel)
        {
            var where = GetWhere(viewModel.AllowanceViewModel);
            var user = await userManager.GetUserAsync(HttpContext.User);
            List<Allowance> Allowance = null;
            if (User.IsInRole(Constants.ADMIN) || User.IsInRole(Constants.FINANCE))
            {
                Allowance = _context.Allowance.Where(where).Include(a => a.AllowanceType).Include(e => e.AssignedBy).Include(e => e.AssignedCategory).Include(e => e.CreatedBy).ToList();
            }
            else if (User.IsInRole(Constants.TL))
            {
                Allowance = _context.Allowance.Where(where).Include(a => a.AllowanceType).Include(e => e.AssignedBy).Include(e => e.AssignedCategory).Include(e => e.CreatedBy).Where(e => e.AssignedCategoryId == user.CategoryId).ToList();
            }
            else
            {
                Allowance = _context.Allowance.Where(where).Include(a => a.AllowanceType).Include(e => e.AssignedBy).Include(e => e.AssignedCategory).Include(e => e.CreatedBy).
                Where(e => e.CreatedById == user.Id).ToList();
            }

            viewModel.AllowanceViewModelList = GetAllowanceViewModel(Allowance);
            viewModel.AllowanceViewModel = new AllowanceViewModel();
            viewModel.AllowanceViewModel.Categories = _context.Category.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.CategoryName,
                Value = e.CategoryId.ToString(),
                Selected = e.CategoryId.ToString() == viewModel.AllowanceViewModel.CategoryID ? true : false
            }).ToList();
            viewModel.AllowanceViewModel.Users = userManager.Users.Select(e => new SelectListItem
            {
                Text = e.Name,
                Value = e.Id,
                Selected = e.Id == viewModel.AllowanceViewModel.UserID ? true : false

            }).ToList();
            viewModel.AllowanceViewModel.ApprovedStatus = Enum.GetValues(typeof(ApprovedStatus)).Cast<ApprovedStatus>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString(),
                Selected = ((int)v).ToString() == viewModel.AllowanceViewModel.ApprovedStatusId ? true : false
            }).ToList();
            viewModel.AllowanceViewModel.AllowanceTypes = _context.AllowanceType.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.AllowanceTypeName,
                Value = e.AllowanceTypeId.ToString()
            }).ToList();
            viewModel.AllowanceViewModel.Categories.Add(new SelectListItem { Text = "Select Category", Value = string.Empty, Selected = string.IsNullOrEmpty(viewModel.AllowanceViewModel?.CategoryID) ? true : false });
            viewModel.AllowanceViewModel.Users.Add(new SelectListItem { Text = "Select User", Value = string.Empty, Selected = string.IsNullOrEmpty(viewModel.AllowanceViewModel?.UserID) ? true : false });
            viewModel.AllowanceViewModel.ApprovedStatus.Add(new SelectListItem { Text = "Select Status", Value = string.Empty, Selected = string.IsNullOrEmpty(viewModel.AllowanceViewModel?.ApprovedStatusId) ? true : false });
            viewModel.AllowanceViewModel.AllowanceTypes.Add(new SelectListItem { Text = "Select Allowance Type", Value = string.Empty, Selected = string.IsNullOrEmpty(viewModel.AllowanceViewModel?.AllowanceTypeId) ? true : false });
            _viewModel = viewModel;
            _viewModel.IsFromFilter = true;
            return viewModel;
        }

        public async Task<IActionResult> Create()
        {
            return View(await GetCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AllowanceViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(HttpContext.User);
                var allowance = new Allowance();
                allowance.Description = viewModel.Comments;
                allowance.ApprovedStatus = ApprovedStatus.PENDING;
                allowance.AllowanceType = _context.AllowanceType.SingleOrDefault(e => e.AllowanceTypeId.ToString() == viewModel.AllowanceTypeId);
                allowance.AllowanceDates = JsonConvert.SerializeObject(AllowanceDates);
                allowance.NumberOfDays = AllowanceDates.Count();
                allowance.Price = viewModel.Price;
                allowance.CreatedOn = DateTime.Now;
                allowance.CreatedBy = user;
                allowance.AssignedBy = user;
                allowance.AssignedCategory = _context.Category.SingleOrDefault(e => e.CategoryId == user.CategoryId);
                allowance.Month = Convert.ToDateTime(AllowanceDates.FirstOrDefault());
                _context.Add(allowance);
                await _context.SaveChangesAsync();
                if(user.Category.CategoryName == "PROCUREMENT")
                {
                    ApplicationUser teamLeader = (from tl in _context.User
                                                 join role in _context.UserRoles
                                                 on tl.Id equals role.UserId
                                                 where tl.CategoryId == user.CategoryId
                                                 select tl).FirstOrDefault();

                    if(teamLeader?.Id != null)
                        await Task.Run(() => emailService.SendEmailAsync(teamLeader.Email, "Allowance " + allowance.ApprovedStatus,
                                GetProcurementAllowanceHtml(allowance, user.Name)));
                }
                return RedirectToAction(nameof(Index));
            }
            viewModel.AllowanceTypes = _context.AllowanceType.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.AllowanceTypeName,
                Value = e.AllowanceTypeId.ToString(),
                Selected = e.AllowanceTypeId.ToString() == viewModel.AllowanceTypeId ? true : false
            }).ToList();
            viewModel.ApprovedStatus = Enum.GetValues(typeof(ApprovedStatus)).Cast<ApprovedStatus>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString(),
                Selected = ((int)v).ToString() == viewModel.ApprovedStatusId ? true : false
            }).ToList();
            return View(viewModel);
        }
        [HttpPost]
        public IActionResult AddDate([FromBody]List<string> dates)
        {
            AllowanceDates = dates;
            return Ok();
        }

        // GET: Allowances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (id == null)
            {
                return NotFound();
            }

            var allowance = await _context.Allowance.FindAsync(id);
            if (allowance == null)
            {
                return NotFound();
            }
            var viewModel = GetAllowance(allowance);
            List<AllowanceType> allowanceTypes = _context.AllowanceType.Where(e => e.IsActive).ToList();

            List<AllowanceType> tempAllowanceTypes = new List<AllowanceType>();
            allowanceTypes.ForEach(model =>
            {
                model.catagoriesList = model.Categories != null ? JsonConvert.DeserializeObject<List<Category>>(model.Categories) : null;
                if (model.catagoriesList != null && model.catagoriesList.Exists(e => e.CategoryId == user.CategoryId))
                    tempAllowanceTypes.Add(model);
            });


            viewModel.AllowanceTypes = tempAllowanceTypes.Select(e => new SelectListItem
            {
                Text = e.AllowanceTypeName,
                Value = e.AllowanceTypeId.ToString(),
                Selected = e.AllowanceTypeId == allowance.AllowanceTypeId ? true : false
            }).ToList();
            viewModel.AllowanceTypesDetails = _context.AllowanceType.Where(e => e.IsActive).ToList();
            viewModel.ApprovedStatus = Enum.GetValues(typeof(ApprovedStatus)).Cast<ApprovedStatus>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString(),
                Selected = ((int)v).ToString() == ((int)allowance.ApprovedStatus).ToString() ? true: false
            }).ToList();
            viewModel.ExistingAllowances = _context.Allowance.Where(e => e.AssignedById == user.Id && e.CreatedOn > DateTime.Now.AddMonths(-3)).ToList();
            if (_viewModel.IsFromFilter)
            {
                viewModel.IsFromFilter = _viewModel.IsFromFilter;
            }
            return View(viewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,AllowanceViewModel viewModel)
        {
            AllowancesFilterViewModel filter = _viewModel;
            var user = await userManager.GetUserAsync(HttpContext.User);
            if (id != viewModel.AllowanceID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var allowance = await _context.Allowance.FindAsync(id);
                    allowance.ModifiedOn = DateTime.Now;
                    allowance.Description = viewModel.Comments;
                    allowance.NumberOfDays = AllowanceDates.Count();
                    allowance.AllowanceDates = JsonConvert.SerializeObject(AllowanceDates);
                    allowance.Price = viewModel.Price;
                    allowance.Month = viewModel.Month;
                    if (!string.IsNullOrEmpty(viewModel.ApprovedStatusId) && ((int)allowance.ApprovedStatus).ToString() != viewModel.ApprovedStatusId)
                        allowance.ApprovedStatus = Enum.GetValues(typeof(ApprovedStatus)).Cast<ApprovedStatus>().SingleOrDefault(e=> ((int)e).ToString() == viewModel.ApprovedStatusId);
                    allowance.AllowanceType = _context.AllowanceType.SingleOrDefault(e => e.AllowanceTypeId.ToString() == viewModel.AllowanceTypeId);
                    allowance.Description = viewModel.Comments;
                    allowance.Month = Convert.ToDateTime(AllowanceDates.FirstOrDefault());
                    _context.Update(allowance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AllowanceExists(viewModel.AllowanceID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if (_viewModel.IsFromFilter || viewModel.IsFromFilter)
                {
                    return await Filter(_viewModel);
                }
                return RedirectToAction(nameof(Index));
            }

            List<AllowanceType> allowanceTypes = _context.AllowanceType.Where(e => e.IsActive).ToList();

            List<AllowanceType> tempAllowanceTypes = new List<AllowanceType>();
            allowanceTypes.ForEach(model =>
            {
                model.catagoriesList = model.Categories != null ? JsonConvert.DeserializeObject<List<Category>>(model.Categories) : null;
                if (model.catagoriesList != null && model.catagoriesList.Exists(e => e.CategoryId == user.CategoryId))
                    tempAllowanceTypes.Add(model);
            });

            viewModel.AllowanceTypes = tempAllowanceTypes.Select(e => new SelectListItem
            {
                Text = e.AllowanceTypeName,
                Value = e.AllowanceTypeId.ToString(),
                Selected = e.AllowanceTypeId.ToString() == viewModel.AllowanceTypeId ? true : false
            }).ToList();
            viewModel.AllowanceTypesDetails = _context.AllowanceType.Where(e => e.IsActive).ToList();
            viewModel.ApprovedStatus = Enum.GetValues(typeof(ApprovedStatus)).Cast<ApprovedStatus>().Select(v => new SelectListItem
            {
                Text = v.ToString(),
                Value = ((int)v).ToString(),
                Selected = ((int)v).ToString() == viewModel.ApprovedStatusId ? true : false
            }).ToList();
            return View(viewModel);
        }

        // GET: Allowances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var allowance = await _context.Allowance
                .Include(a => a.AllowanceType).Include(e=>e.CreatedBy).Include(e=>e.AssignedCategory)
                .FirstOrDefaultAsync(m => m.AllowanceID == id);
            if (allowance == null)
            {
                return NotFound();
            }
            AllowanceViewModel viewModel = GetAllowance(allowance);
            viewModel.IsFromFilter = _viewModel.IsFromFilter;
            return View(viewModel);
        }

        
      

        // POST: Allowances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var allowance = await _context.Allowance.FindAsync(id);
            _context.Allowance.Remove(allowance);
            await _context.SaveChangesAsync();
            if (_viewModel.IsFromFilter)
            {
                return await Filter(_viewModel);
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AllowanceExists(int id)
        {
            return _context.Allowance.Any(e => e.AllowanceID == id);
        }

        private Expression<Func<Models.Allowance, bool>> GetWhere(AllowanceViewModel model)
        {
            Expression<Func<Models.Allowance, bool>> where = c => true;

            if (!string.IsNullOrEmpty(model.MonthId))
            {
                var prefix = where.Compile();
                where = c => prefix(c) && c.Month.Month.ToString() == model.MonthId;
            }
            else
            {
                var prefix = where.Compile();
                where = c => prefix(c) && c.Month > DateTime.Today.AddMonths(-6);
            }
            if (!string.IsNullOrEmpty(model.CategoryID))
            {
                var prefix = where.Compile();
                where = c => prefix(c) && c.AssignedCategoryId.ToString() == model.CategoryID;
            }
            if (!string.IsNullOrEmpty(model.AllowanceTypeId))
            {
                var prefix = where.Compile();
                where = c => prefix(c) && c.AllowanceTypeId.ToString() == model.AllowanceTypeId;
            }
            if (!string.IsNullOrEmpty(model.ApprovedStatusId))
            {
                var prefix = where.Compile();
                where = c => prefix(c) && c.ApprovedStatus == (ApprovedStatus)Enum.Parse(typeof(ApprovedStatus), model.ApprovedStatusId, true);
            }
            if (!string.IsNullOrEmpty(model.UserID))
            {
                var prefix = where.Compile();
                where = c => prefix(c) && c.CreatedById.ToString() == model.UserID;
            }
            
            return where;
        }

        private async Task<AllowanceViewModel> GetCreateViewModel()
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            var viewModel = new AllowanceViewModel();
            List<AllowanceType> allowanceTypes = _context.AllowanceType.Where(e => e.IsActive).ToList();
            viewModel.ExistingAllowances = _context.Allowance.Where(e=>e.AssignedById == user.Id && e.CreatedOn > DateTime.Now.AddMonths(-3)).ToList();
            List<AllowanceType> tempAllowanceTypes = new List<AllowanceType>();
                allowanceTypes.ForEach(model =>
            {
                model.catagoriesList = model.Categories != null ? JsonConvert.DeserializeObject<List<Category>>(model.Categories) : null;
                if (model.catagoriesList != null && model.catagoriesList.Exists(e => e.CategoryId == user.CategoryId))
                    tempAllowanceTypes.Add(model);
            });
            
            viewModel.AllowanceTypes = tempAllowanceTypes.Select(e => new SelectListItem
            {
                Text = e.AllowanceTypeName,
                Value = e.AllowanceTypeId.ToString()
            }).ToList();

            viewModel.AllowanceTypesDetails = _context.AllowanceType.Where(e => e.IsActive).ToList();
            if (User.IsInRole(Constants.ADMIN))
            {
                viewModel.AssignedToItems = _context.User.Where(e => e.IsActive).Select(e => new SelectListItem
                {
                    Text = e.Name,
                    Value = e.Id
                }).ToList();
            }
            else if (User.IsInRole(Constants.TL))
            {
                viewModel.AssignedToItems = _context.User.Where(e => e.CategoryId == user.CategoryId).Select(e => new SelectListItem
                {
                    Text = e.Name,
                    Value = e.Id
                }).ToList();
            }

            viewModel.AllowanceTypes.Add(new SelectListItem { Text = "Select Allowance Type", Value = string.Empty, Selected = true });

            if (viewModel.AssignedToItems != null && viewModel.AssignedToItems.Count > 0)
                viewModel.AssignedToItems.Add(new SelectListItem { Text = "Assign to", Value = string.Empty, Selected = true });
            return viewModel;
        }
        private List<AllowanceViewModel> GetAllowanceViewModel(List<Allowance> allowances)
        {
            List<AllowanceViewModel> allowancesViewModel = new List<AllowanceViewModel>();
            allowances.ForEach(e =>
            {
                allowancesViewModel.Add(GetAllowance(e));
            });

            return allowancesViewModel;

        }

        private AllowanceViewModel GetAllowance(Allowance allowance)
        {
            AllowanceViewModel ViewModel = new AllowanceViewModel()
            {
                AllowanceDate = allowance.CreatedOn,
                AllowanceID = allowance.AllowanceID,
                AllowanceType = allowance.AllowanceType,
                Status = allowance.ApprovedStatus,
                AssignedBy = allowance.AssignedBy,
                CreatedBy = allowance.CreatedBy,
                Comments = allowance.Description,
                Price = allowance.Price,
                CreatedOn = allowance.CreatedOn,
                AssignedCategory = allowance.AssignedCategory,
                AllowanceTypeId = allowance.AllowanceTypeId.ToString(),
                AllownanceDates = JsonConvert.DeserializeObject<List<string>>(allowance.AllowanceDates),
                AllownanceDatesjsonResult = allowance.AllowanceDates,
                NumberOfDays = allowance.NumberOfDays,
                Month = allowance.Month,
                UserID = allowance.CreatedById


            };
            return ViewModel;

        }
        private string GetListOfDates(List<string> Dates)
        {
            string date = string.Empty;
            Dates.ForEach(data => {
                date = date + data + ":";
            });
            return date;
        }
    }
}
