using HospitalMS.DataAccess.Data;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        public UnitOfWork(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor, 
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager,
            ClaimsPrincipal claimsPrincipal)
        {
            _db = db;
            Doctor = new DoctorRepository(_db, webHostEnvironment, httpContextAccessor);
            Departament = new DepartamentRepository(_db);
            Auth = new AuthRepository(_db, userManager, roleManager, signInManager, claimsPrincipal);
            Chat = new ChatRepository(_db);
        }

        public async Task SaveAsync() 
        { 
            await _db.SaveChangesAsync();
        }
    }
}
