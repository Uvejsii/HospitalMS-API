using HospitalMS.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions) : base(dbContextOptions)
        {   
        }

        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Departament> Departaments { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<DoctorReview> DoctorReviews { get; set; }
        //public DbSet<Booking> Bookings { get; set; }
    }
}
