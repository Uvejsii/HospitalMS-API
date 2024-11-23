using HospitalMS.DataAccess.Data;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;
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
                doctorFromDb.ImageFileName = doctor.ImageFileName;
                if (doctor.Image != null)
                {
                    doctorFromDb.Image = doctor.Image;
                    doctorFromDb.ImageFileExtension = doctor.ImageFileExtension;
                    doctorFromDb.ImageFileSizeInBytes = doctor.ImageFileSizeInBytes;

                    var urlFilePath = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/Images/{doctor.ImageFileName}{doctor.ImageFileExtension}";
                    doctorFromDb.ImageFilePath = urlFilePath;
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
            var imagesFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Images");

            if (!string.IsNullOrEmpty(doctor.ImageFilePath))
            {
                var existingFilePath = Path.Combine(imagesFolderPath, Path.GetFileName(doctor.ImageFilePath));
                var newFilePath = Path.Combine(imagesFolderPath, $"{doctor.ImageFileName}{doctor.ImageFileExtension}");

                try
                {
                    if (existingFilePath != newFilePath)
                    {
                        if (File.Exists(existingFilePath))
                        {
                            File.Delete(existingFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new IOException("Error handling the existing image file.", ex);
                }
            }

            if (doctor.Image != null)
            {
                var localFilePath = Path.Combine(imagesFolderPath, $"{doctor.ImageFileName}{doctor.ImageFileExtension}");

                if (File.Exists(localFilePath))
                {
                    File.Delete(localFilePath);
                }

                using var stream = new FileStream(localFilePath, FileMode.Create);
                await doctor.Image.CopyToAsync(stream);
            }

            var urlFilePath = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/Images/{doctor.ImageFileName}{doctor.ImageFileExtension}";
            doctor.ImageFilePath = urlFilePath;

            return doctor;
        }

        public async Task<bool> ValidateFileUpload(AddDoctorRequestDto addDoctorRequestDto)
        {
            var allowedExtensions = new string[] { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(Path.GetExtension(addDoctorRequestDto.Image.FileName)))
            {
                return await Task.FromResult(false);
            }

            if (addDoctorRequestDto.Image.Length > 10485760)
            {
                return await Task.FromResult(false);

            }

            return await Task.FromResult(true);
        }

        public async Task<bool> ValidateFileEdit(UpdateDoctorRequestDto updateDoctorRequestDto)
        {
            var allowedExtensions = new string[] { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(Path.GetExtension(updateDoctorRequestDto.Image.FileName)))
            {
                return await Task.FromResult(false);
            }

            if (updateDoctorRequestDto.Image.Length > 10485760)
            {
                return await Task.FromResult(false);

            }

            return await Task.FromResult(true);
        }
    }
}
