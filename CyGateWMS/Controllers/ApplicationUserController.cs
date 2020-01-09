using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CyGateWMS.Models;
using CyGateWMS.Services;
using CyGateWMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CyGateWMS.Controllers
{
    [Authorize]
    public class ApplicationUserController : Controller
    {
        private readonly CygateWMSContext _context;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IEmailService emailService;


        public ApplicationUserController(CygateWMSContext context, SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IEmailService emailService)
        {
            this._context = context;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.emailService = emailService;
        }
        // GET: User
        public IActionResult Index()
        {
            List<UserViewModel> model = new List<UserViewModel>();
            var _user = userManager.GetUserAsync(HttpContext.User).Result;
            var category = _context.Category;

            if (User.IsInRole(Constants.ADMIN))
            {
                userManager.Users.ToList().ForEach(user =>
                {
                    string role = userManager.GetRolesAsync(user).Result.ToList().FirstOrDefault();
                    model.Add(new UserViewModel()
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        UserName = user.Name,
                        Role = role,
                        PhoneNumber = user.PhoneNumber,
                        EmployeeId = user.EmployeeId,
                        EmailConfirmed = user.EmailConfirmed,
                        CategoryName = category.Single(e => e.CategoryId == user.CategoryId).CategoryName
                    });
                });
            }
            else if (User.IsInRole(Constants.FINANCE))
            {
                userManager.Users.Where(e => e.CategoryId == _user.CategoryId).ToList().ForEach(user =>
                {
                    string role = userManager.GetRolesAsync(user).Result.ToList().FirstOrDefault();
                    model.Add(new UserViewModel()
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        UserName = user.Name,
                        Role = role,
                        PhoneNumber = user.PhoneNumber,
                        EmployeeId = user.EmployeeId,
                        CategoryName = category.Single(e => e.CategoryId == user.CategoryId).CategoryName
                    });
                });
            }
            else if (User.IsInRole(Constants.TL))
            {
                userManager.Users.Where(e=>e.CategoryId == _user.CategoryId).ToList().ForEach(user =>
                {
                    string role = userManager.GetRolesAsync(user).Result.ToList().FirstOrDefault();
                    model.Add(new UserViewModel()
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        UserName = user.Name,
                        Role = role,
                        PhoneNumber = user.PhoneNumber,
                        EmployeeId = user.EmployeeId,
                        CategoryName = category.Single(e => e.CategoryId == user.CategoryId).CategoryName
                    });
                });
            }
            else if (User.IsInRole(Constants.USER))
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View(model);
        }
        
        public IActionResult AllUsers()
        {
            List<UserViewModel> model = new List<UserViewModel>();
            var _user = userManager.GetUserAsync(HttpContext.User).Result;
            var category = _context.Category;

            userManager.Users.ToList().ForEach(user =>
            {
                string role = userManager.GetRolesAsync(user).Result.ToList().FirstOrDefault();
                model.Add(new UserViewModel()
                {
                    UserId = user.Id,
                    Email = user.Email,
                    UserName = user.Name,
                    Role = role,
                    PhoneNumber = user.PhoneNumber,
                    EmployeeId = user.EmployeeId,
                    CategoryName = category.Single(e => e.CategoryId == user.CategoryId).CategoryName
                });
            });

            return View(nameof(Index),model);
        }

        // GET: User/Details/5
        public ActionResult Details(string id)
        {
            if (id != null)
            {                
                var user = userManager.FindByIdAsync(id).Result;                
                var viewModel = new UserViewModel()
                {
                    UserId = user.Id,
                    UserName = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    Role = userManager.GetRolesAsync(user).Result.Single(),
                    CategoryName = _context.Category.Single(e => e.CategoryId == user.CategoryId).CategoryName,
                    IsCab  = user.IsCab,
                    IsRegularShift = user.IsRegularShift,
                    IsActive = user.IsActive,
                    EmployeeId = user.EmployeeId

                };
                return View(viewModel);
            }
            else
            {
                var user = userManager.GetUserAsync(HttpContext.User).Result;
                if (user?.Id != null)
                {
                    var viewModel = new UserViewModel()
                    {
                        UserId = user.Id,
                        UserName = user.Name,
                        PhoneNumber = user.PhoneNumber,
                        Email = user.Email,
                        Role = userManager.GetRolesAsync(user).Result.Single(),
                        IsCab = user.IsCab,
                        IsRegularShift = user.IsRegularShift,
                        IsActive = user.IsActive,
                        EmployeeId = user.EmployeeId
                    };
                    return View(viewModel);
                }
                return NotFound();
            }
        }

        // GET: User/Create
        [AllowAnonymous]
        public ActionResult Create()
        {
            UserViewModel model = new UserViewModel()
            {
                Categories = _context.Category.Where(e => e.IsActive && e.CategoryName != Constants.ADMIN).Select(e => new SelectListItem
                {
                    Text = e.CategoryName,
                    Value = e.CategoryId.ToString()

                }).ToList(),
            };
            model.Categories.Add(new SelectListItem { Text = "Select Department", Value = string.Empty, Selected = true });
            return View(model);
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser()
                {
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    Name = model.UserName,
                    CreatedOn = DateTime.Now,
                    Category = _context.Category.SingleOrDefault(e => e.CategoryId == Convert.ToInt32(model.CategoryId)),
                    IsActive = true,
                    IsRegularShift = false,
                    IsCab = false,
                    EmployeeId = model.EmployeeId,
                };
                IdentityResult result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    IdentityResult roleResult = await userManager.AddToRoleAsync(user, roleManager.Roles.Where(e => e.Name == Constants.USER).FirstOrDefault().Name);

                    var code = System.Net.WebUtility.HtmlEncode(await userManager.GenerateEmailConfirmationTokenAsync(user));
                    var callbackUrl = Url.Action("ConfirmEmail", "Account",
                    new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
                    await emailService.SendEmailAsync(model.Email, "Confirm your account",
                       GetConfirmPWDHtml(model.UserName, callbackUrl));


                    await signInManager.SignInAsync(user, isPersistent: false);
                    //if(signinResult.Succeeded)
                    //     return RedirectToAction(nameof(Index), "Dashboard");
                    //else
                    return View("RegisterConfirmation");
                }
                else
                {
                    string error = "";
                    foreach(var item in result.Errors)
                    {
                        error += item.Description + "\n";
                    }
                    ModelState.AddModelError("Password", error);
                }
            }
            model.Categories = _context.Category.Where(e => e.IsActive).Select(e => new SelectListItem
            {
                Text = e.CategoryName,
                Value = e.CategoryId.ToString(),
                Selected = e.CategoryId.ToString() == model.CategoryId ? true : false

            }).ToList();
            model.Categories.Add(new SelectListItem { Text = "Select Department", Value = string.Empty, Selected = string.IsNullOrEmpty(model.CategoryId) ? true : false });

            return View(model);
        }
        
        public ActionResult EmailConfirm(string id)
        {
            if (id != null)
            {
                var user = userManager.FindByIdAsync(id).Result;
                user.EmailConfirmed = true;
                _context.User.Update(user);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        private string GetConfirmPWDHtml(string name, string callbackUrl)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><body><p> Please confirm your account</p><p>Dear " + name + ",</p>");

            sb.Append("<p>Go to Confirm: <a href='" + callbackUrl + "'>link </a>.If you did not perform this operation, please contact your manager.</p>");


            sb.Append("<hr><p><font color='red'> ***This is an automatically generated message, please do not reply to this message * **</font></p></body></html>");
            return sb.ToString();
        }

        // GET: User/Edit/5
        public ActionResult Edit(string id)
        {
            if(userManager.GetUserId(HttpContext.User) == id || User.IsInRole(Constants.ADMIN))
            {
                var user = userManager.FindByIdAsync(id).Result;
                var viewModel = new UserViewModel()
                {
                    UserId = user.Id,
                    UserName = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    IsRegularShift = user.IsRegularShift,
                    IsCab = user.IsCab,
                    EmployeeId = user.EmployeeId,
                    ApplicationRoles = roleManager.Roles.Where(e => e.IsActive == true).Select(e => new SelectListItem
                    {
                        Text = e.Name,
                        Value = e.Id,
                        Selected = e.Name == userManager.GetRolesAsync(user).Result.FirstOrDefault() ? true : false
                    }).ToList(),
                    Categories = _context.Category.Where(e => e.IsActive == true).Select(e => new SelectListItem
                    {
                        Text = e.CategoryName,
                        Value = e.CategoryId.ToString(),
                        Selected = e.CategoryId == user.CategoryId ? true : false

                    }).ToList()
                };
                return View(viewModel);
            }
            else
            {
                return null;
            }            
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByIdAsync(id);
                if (user != null)
                {
                    user.Name = model.UserName;
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    user.IsActive = model.IsActive;
                    user.IsRegularShift = model.IsRegularShift;
                    user.IsCab = model.IsCab;
                    user.EmployeeId = model.EmployeeId;
                    user.Category = _context.Category.SingleOrDefault(e => e.CategoryId == Convert.ToInt32(model.CategoryId));

                    string existingRole = userManager.GetRolesAsync(user).Result.Single();
                    string existingRoleId = roleManager.Roles.Single(r => r.Name == existingRole).Id;
                    IdentityResult result = await userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(model.ApplicationRoleId) && existingRoleId != model.ApplicationRoleId)
                        {
                            IdentityResult roleResult = await userManager.RemoveFromRoleAsync(user, existingRole);
                            if (roleResult.Succeeded)
                            {
                                ApplicationRole applicationRole = await roleManager.FindByIdAsync(model.ApplicationRoleId);
                                if (applicationRole != null)
                                {
                                    IdentityResult newRoleResult = await userManager.AddToRoleAsync(user, applicationRole.Name);
                                    if (newRoleResult.Succeeded)
                                    {
                                        return RedirectToAction("Index");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: User/Delete/5
        [Authorize(Roles = Constants.ADMIN)]
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: User/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.ADMIN)]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            return RedirectToAction(nameof(Index));
        }
    }
}