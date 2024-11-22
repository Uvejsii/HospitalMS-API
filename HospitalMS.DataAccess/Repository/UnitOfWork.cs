using HospitalMS.DataAccess.Data;
using HospitalMS.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;

        public IDoctorRepository Doctor {  get; private set; }
        public IDepartamentRepository Departament { get; private set; }
        public UnitOfWork(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            Doctor = new DoctorRepository(_db, webHostEnvironment, httpContextAccessor);
            Departament = new DepartamentRepository(_db);
        }

        public async Task SaveAsync() 
        { 
            await _db.SaveChangesAsync();
        }
    }
}
