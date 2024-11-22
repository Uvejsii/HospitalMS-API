using HospitalMS.DataAccess.Data;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository
{
    public class DoctorRepository : Repository<Doctor>, IDoctorRepository
    {
        private ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DoctorRepository(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor) : base(db)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Doctor?> UpdateAsync(int id, Doctor doctor)
        {
            var doctorFromDb = await _db.Doctors.FindAsync(id);

            if (doctorFromDb != null)
            {
                doctorFromDb.FirstName = doctor.FirstName;
                doctorFromDb.LastName = doctor.LastName;
                doctorFromDb.YearsOfExperience = doctor.YearsOfExperience;
                doctorFromDb.DepartamentId = doctor.DepartamentId;
                doctorFromDb.Email = doctor.Email;
                doctorFromDb.PhoneNumber = doctor.PhoneNumber;
                doctorFromDb.ConsultationFee = doctor.ConsultationFee;
                doctorFromDb.isAvailable = doctor.isAvailable;
                if (doctor.Image != null)
                {
                    doctorFromDb.Image = doctor.Image;
                }

                return doctorFromDb;
            }

            return null;
        }

        public async Task<bool> DeleteDoc(int? id)
        {
            var doctorFromDb = await _db.Doctors.FindAsync(id);

            if (doctorFromDb != null)
            {
                _db.Doctors.Remove(doctorFromDb);
                return true;
            }

            return false;
        }

        public async Task<Doctor?> UploadDrImage(Doctor doctor)
        {
            var localFilePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Images", $"{doctor.ImageFileName}{doctor.ImageFileExtension}");

            using var stream = new FileStream(localFilePath, FileMode.Create);
            await doctor.Image.CopyToAsync(stream);

            var urlFilePath = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/Images/{doctor.ImageFileName}{doctor.ImageFileExtension}";

            doctor.ImageFilePath = urlFilePath;

            return doctor;
        }
    }
}
