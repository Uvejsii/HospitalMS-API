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
                if (doctor.Image != null)
                {
                    var imagesFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Images");

                    if (doctorFromDb.ImageFileName != doctor.ImageFileName)
                    {
                        var existingFilePath = Path.Combine(imagesFolderPath, $"{doctorFromDb.ImageFileName}{doctorFromDb.ImageFileExtension}");
                        if (File.Exists(existingFilePath))
                        {
                            File.Delete(existingFilePath);
                        }
                    }

                    doctorFromDb.Image = doctor.Image;
                    doctorFromDb.ImageFileExtension = doctor.ImageFileExtension;
                    doctorFromDb.ImageFileSizeInBytes = doctor.ImageFileSizeInBytes;
                    doctorFromDb.ImageFileName = doctor.ImageFileName;

                    var newFilePath = Path.Combine(imagesFolderPath, $"{doctor.ImageFileName}{doctor.ImageFileExtension}");
                    using var stream = new FileStream(newFilePath, FileMode.Create);
                    await doctor.Image.CopyToAsync(stream);

                    var urlFilePath = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/Images/{doctor.ImageFileName}{doctor.ImageFileExtension}";
                    doctorFromDb.ImageFilePath = urlFilePath;
                }

                if (doctorFromDb.ImageFileName != doctor.ImageFileName)
                {
                    var imagesFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Images");

                    var existingFileName = Path.GetFileName(new Uri(doctorFromDb.ImageFilePath).AbsolutePath);
                    var existingFilePath = Path.Combine(imagesFolderPath, existingFileName);
                    var newFileName = $"{doctor.ImageFileName}{doctorFromDb.ImageFileExtension}";
                    var newFilePath = Path.Combine(imagesFolderPath, newFileName);

                    if (File.Exists(existingFilePath) && existingFilePath != newFilePath)
                    {
                        File.Move(existingFilePath, newFilePath);
                    }

                    doctorFromDb.ImageFileName = doctor.ImageFileName;
                    var urlFilePath = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/Images/{doctor.ImageFileName}{doctorFromDb.ImageFileExtension}";
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
                var imagesFolderPath = Path.Combine(_webHostEnvironment.ContentRootPath, "Images");
                var filePath = Path.Combine(imagesFolderPath, $"{doctorFromDb.ImageFileName}{doctorFromDb.ImageFileExtension}");
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

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
                var existingFileName = Path.GetFileName(new Uri(doctor.ImageFilePath).AbsolutePath);
                var existingFilePath = Path.Combine(imagesFolderPath, existingFileName);
                var newFileName = $"{doctor.ImageFileName}{doctor.ImageFileExtension}";
                var newFilePath = Path.Combine(imagesFolderPath, newFileName);

                try
                {
                    if (doctor.Image == null)
                    {
                        if (!string.IsNullOrEmpty(doctor.ImageFileName) && doctor.ImageFileName != existingFileName)
                        {
                            if (File.Exists(existingFilePath) && existingFilePath != newFilePath)
                            {
                                File.Move(existingFilePath, newFilePath);
                            }
                        }
                    }
                    else
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
                var newLocalFilePath = Path.Combine(imagesFolderPath, $"{doctor.ImageFileName}{doctor.ImageFileExtension}");

                if (File.Exists(newLocalFilePath))
                {
                    File.Delete(newLocalFilePath);
                }

                using var stream = new FileStream(newLocalFilePath, FileMode.Create);
                await doctor.Image.CopyToAsync(stream);
            }

            var urlFilePath = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.PathBase}/Images/{doctor.ImageFileName}{doctor.ImageFileExtension}";
            doctor.ImageFilePath = urlFilePath;

            Console.WriteLine($"Updated ImageFilePath: {doctor.ImageFilePath}");
            return doctor;
        }


        public async Task<bool> ValidateFileUpload(RegisterDoctorFullRequestDto registerDoctorFullRequestDto)
        {
            var allowedExtensions = new string[] { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(Path.GetExtension(registerDoctorFullRequestDto.Image.FileName)))
            {
                return await Task.FromResult(false);
            }

            if (registerDoctorFullRequestDto.Image.Length > 10485760)
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
