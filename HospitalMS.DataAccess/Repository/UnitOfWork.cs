using Amazon.S3;
using Amazon.S3.Util;
using HospitalMS.DataAccess.Data;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;

        public IDoctorRepository Doctor {  get; private set; }
        public IDepartamentRepository Departament { get; private set; }
        public IAuthRepository Auth { get; private set; }
        public IChatRepository Chat { get; private set; }
        public IDoctorReviewRepository DoctorReview { get; private set; }
        public IBookingRepository Booking { get; private set; }
        public IDoctorVacationRepository DoctorVacation { get; private set; }
        public UnitOfWork(ApplicationDbContext db, IAmazonS3 amazonS3, UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager,
            ClaimsPrincipal claimsPrincipal, IConfiguration configuration)
        {
            _db = db;
            Doctor = new DoctorRepository(_db, amazonS3);
            Departament = new DepartamentRepository(_db);
            Auth = new AuthRepository(_db, userManager, roleManager, signInManager, claimsPrincipal, configuration);
            Chat = new ChatRepository(_db);
            DoctorReview = new DoctorReviewRepository(_db);
            Booking = new BookingRepository(_db);
            DoctorVacation = new DoctorVacationRepository(_db);
        }

        public async Task SaveAsync() 
        { 
            await _db.SaveChangesAsync();
        }
    }
}
