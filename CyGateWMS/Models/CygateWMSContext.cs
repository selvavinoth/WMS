using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CyGateWMS.Models
{
    public partial class CygateWMSContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public CygateWMSContext(DbContextOptions<CygateWMSContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ApplicationUser> User { get; set; }
        public DbSet<ApplicationRole> Role { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Type> Type { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Report> Report { get; set; }
        public DbSet<Shift> Shift { get; set; }
        public DbSet<OnCallEscalate> onCallEscalates { get; set; }
        public DbSet<Allowance> Allowance { get; set; }
        public DbSet<AllowanceType> AllowanceType { get; set; }
        public DbSet<Roster> Rosters { get; set; }
        public DbSet<RosterShift> RosterShifts { get; set; }
        public DbSet<Comments> Comments { get; set; }
        public virtual DbSet<Holiday> Holiday { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            base.OnModelCreating(modelBuilder);            
        }
    }
}
