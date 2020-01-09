using CyGateWMS.Models;
using CyGateWMS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace CyGateWMS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CygateWMSContext>(optionsBuilder => optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<ApplicationUser, ApplicationRole>(config =>
            {
                config.SignIn.RequireConfirmedEmail = true;
            }).AddEntityFrameworkStores<CygateWMSContext>().AddDefaultTokenProviders();
            //services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/LogIn");

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.LogoutPath = "/Account/SignOff";
                options.ExpireTimeSpan = TimeSpan.FromSeconds(5);
                options.SlidingExpiration = true;

            });
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly = true;
            });

            services.AddAuthorization(options =>
            {

                options.AddPolicy(Constants.ADMIN,
                    authBuilder =>
                    {
                        authBuilder.RequireRole(Constants.ADMIN);
                    });
                options.AddPolicy(Constants.USER,
                   authBuilder =>
                   {
                       authBuilder.RequireRole(Constants.USER);
                   });
                options.AddPolicy(Constants.TL,
                   authBuilder =>
                   {
                       authBuilder.RequireRole(Constants.TL);
                   });

            });
            services.Configure<FormOptions>(x => x.ValueCountLimit = 10000);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            // services.AddSingleton<IEmailConfiguration>(Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
            services.AddTransient<IEmailService, EmailService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, Microsoft.Extensions.Logging.ILoggerFactory loggerFactory, CygateWMSContext context)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (env.IsProduction() || env.IsStaging() || env.IsEnvironment("Staging_2"))
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }


            //app.UseExceptionHandler("/Home/Error");
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();
            //app.UseStatusCodePagesWithRedirects("/Home/Error");
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Account}/{action=Login}/{id?}");
            });
            DbInitializer.Initialize(context);
        }
    }
    public static class DbInitializer
    {

        public static void Initialize(CygateWMSContext context)
        {

            context.Database.EnsureCreated(); //if it(your database) doesn't exist ,it will be created
            context.Database.Migrate();
           
            //Roles
            if (!context.Roles.Any())
            {
                context.Roles.Add(new ApplicationRole() { Name = Constants.ADMIN, NormalizedName = Constants.ADMIN, CreatedDate = DateTime.Now, IsActive = true, Description = "Admin" });
                context.Roles.Add(new ApplicationRole() { Name = Constants.USER, NormalizedName = Constants.USER, CreatedDate = DateTime.Now, IsActive = true, Description = "User" });
                context.Roles.Add(new ApplicationRole() { Name = Constants.TL, NormalizedName = Constants.TL, CreatedDate = DateTime.Now, IsActive = true, Description = "Team Leader" });
                context.SaveChanges();
            }
            //Category
            if (!context.Category.Any())
            {
                context.Category.Add(new Category() { CategoryName = Constants.LINUX, CreatedOn = DateTime.Now, IsActive = true });
                context.Category.Add(new Category() { CategoryName = Constants.WINDOWS, CreatedOn = DateTime.Now, IsActive = true });
                context.Category.Add(new Category() { CategoryName = Constants.VMWARE, CreatedOn = DateTime.Now, IsActive = true });
                context.Category.Add(new Category() { CategoryName = Constants.NETWORKING, CreatedOn = DateTime.Now, IsActive = true });
                context.Category.Add(new Category() { CategoryName = Constants.ADMIN, CreatedOn = DateTime.Now, IsActive = true });
                context.Category.Add(new Category() { CategoryName = Constants.PROCUREMENT, CreatedOn = DateTime.Now, IsActive = true });
                context.Category.Add(new Category() { CategoryName = Constants.DEVELOPMENT, CreatedOn = DateTime.Now, IsActive = true });
                context.Category.Add(new Category() { CategoryName = Constants.FINANCE, CreatedOn = DateTime.Now, IsActive = true });
                context.SaveChanges();
            }
            //Type
            if (!context.Type.Any())
            {
                context.Type.Add(new Models.Type() { TypeName = Constants.ALERT, CreatedOn = DateTime.Now, IsActive = true });
                context.Type.Add(new Models.Type() { TypeName = Constants.CHAT, CreatedOn = DateTime.Now, IsActive = true });
                context.Type.Add(new Models.Type() { TypeName = Constants.DOCUMENTATION, CreatedOn = DateTime.Now, IsActive = true });
                context.Type.Add(new Models.Type() { TypeName = Constants.INCIDENT, CreatedOn = DateTime.Now, IsActive = true });
                context.Type.Add(new Models.Type() { TypeName = Constants.PHREPORT, CreatedOn = DateTime.Now, IsActive = true });
                context.Type.Add(new Models.Type() { TypeName = Constants.PROJECTS, CreatedOn = DateTime.Now, IsActive = true });
                context.Type.Add(new Models.Type() { TypeName = Constants.SCHEDULEDTASK, CreatedOn = DateTime.Now, IsActive = true });
                context.Type.Add(new Models.Type() { TypeName = Constants.TASK, CreatedOn = DateTime.Now, IsActive = true });
                context.Type.Add(new Models.Type() { TypeName = Constants.TICKET, CreatedOn = DateTime.Now, IsActive = true });
                context.SaveChanges();
            }
            //Shift
            if (!context.Shift.Any())
            {
                context.Shift.Add(new Shift() { ShiftName = Constants.GENERAL, CreatedOn = DateTime.Now, IsActive = true });
                context.Shift.Add(new Shift() { ShiftName = Constants.SHIFT_A, CreatedOn = DateTime.Now, IsActive = true });
                context.Shift.Add(new Shift() { ShiftName = Constants.SHIFT_B, CreatedOn = DateTime.Now, IsActive = true });
                context.Shift.Add(new Shift() { ShiftName = Constants.SHIFT_C, CreatedOn = DateTime.Now, IsActive = true });
                context.SaveChanges();
            }
            //onCallEscalates
            if (!context.onCallEscalates.Any())
            {
                context.onCallEscalates.Add(new OnCallEscalate() { OnCallEscalateName = Constants.NO, CreatedOn = DateTime.Now, IsActive = true });
                context.onCallEscalates.Add(new OnCallEscalate() { OnCallEscalateName = Constants.YES_INDIA, CreatedOn = DateTime.Now, IsActive = true });
                context.onCallEscalates.Add(new OnCallEscalate() { OnCallEscalateName = Constants.YES_SWEDEN, CreatedOn = DateTime.Now, IsActive = true });
                context.SaveChanges();
            }
            //onCallEscalates
            if (!context.AllowanceType.Any())
            {
                context.AllowanceType.Add(new AllowanceType() { AllowanceTypeName = Constants.HOLIDAY, AllowanceTypePrice = 1250, CreatedOn = DateTime.Now, IsActive = true });
                context.AllowanceType.Add(new AllowanceType() { AllowanceTypeName = Constants.NIGHT, AllowanceTypePrice = 400, CreatedOn = DateTime.Now, IsActive = true });
                context.SaveChanges();
            }
            //ShiftRoster
            if (!context.RosterShifts.Any())
            {
                context.RosterShifts.Add(new RosterShift() { RosterShiftName = Constants.A, CreatedOn = DateTime.Now, IsActive = true });
                context.RosterShifts.Add(new RosterShift() { RosterShiftName = Constants.B, CreatedOn = DateTime.Now, IsActive = true });
                context.RosterShifts.Add(new RosterShift() { RosterShiftName = Constants.C, CreatedOn = DateTime.Now, IsActive = true });
                context.RosterShifts.Add(new RosterShift() { RosterShiftName = Constants.R, CreatedOn = DateTime.Now, IsActive = true });
                context.RosterShifts.Add(new RosterShift() { RosterShiftName = Constants.OFF, CreatedOn = DateTime.Now, IsActive = true });
                context.RosterShifts.Add(new RosterShift() { RosterShiftName = Constants.PL, CreatedOn = DateTime.Now, IsActive = true });
                context.RosterShifts.Add(new RosterShift() { RosterShiftName = Constants.CL, CreatedOn = DateTime.Now, IsActive = true });
                context.RosterShifts.Add(new RosterShift() { RosterShiftName = Constants.SL, CreatedOn = DateTime.Now, IsActive = true });
                context.SaveChanges();
            }
            
        }
    }

}
