using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace HealthPortal.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        public virtual Identifiers Identifier { get; set; }

        public string PhysicianID { get; set; }

        public string DisplayName { get; set; }

        [ForeignKey("PhysicianID")]
        public virtual ApplicationUser PrimaryPhysician { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Identifiers> Identifiers { get; set; }
        public DbSet<Appointments> Appointments { get; set; }
        public DbSet<CheckUpResponse> CheckUpResponse { get; set; }
        public DbSet<MedicalHistory> MedicalHistory { get; set; }
        public DbSet<PrescriptionType> PrescriptionTypes { get; set; }
        public DbSet<Prescriptions> Prescriptions { get; set; }
        public DbSet<PrescriptionsMap> PrescriptionMap { get; set; }
        public DbSet<Diagnosis> Diagnoses { get; set; }
        public DbSet<DiagnosisMap> DiagnosisMap { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Thread> Threads { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PrescriptionsMap>()
                .HasKey(pm => new { pm.UserID, pm.PrescriptionID });
            modelBuilder.Entity<DiagnosisMap>()
                .HasKey(dm => new { dm.UserID, dm.DiagnosisID });

            base.OnModelCreating(modelBuilder);
        }
    }
}