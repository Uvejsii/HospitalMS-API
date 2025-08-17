using Amazon.S3;
using Amazon.S3.Model;
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
        private readonly IAmazonS3 _amazonS3;

        public DoctorRepository(ApplicationDbContext db, IAmazonS3 amazonS3) : base(db)
        {
            _db = db;
            _amazonS3 = amazonS3;
        }

        public async Task<Doctor?> UpdateAsync(int id, UpdateDoctorRequestDto updateDoctorRequestDto)
        {
            var doctorFromDb = await _db.Doctors.FindAsync(id);
            if (doctorFromDb != null)
            {
                doctorFromDb.FirstName = updateDoctorRequestDto.FirstName;
                doctorFromDb.LastName = updateDoctorRequestDto.LastName;
                doctorFromDb.YearsOfExperience = updateDoctorRequestDto.YearsOfExperience;
                doctorFromDb.DepartamentId = updateDoctorRequestDto.DepartamentId;
                doctorFromDb.PhoneNumber = updateDoctorRequestDto.PhoneNumber;
                doctorFromDb.ConsultationFee = updateDoctorRequestDto.ConsultationFee;
                doctorFromDb.isAvailable = updateDoctorRequestDto.isAvailable;
                doctorFromDb.Languages = updateDoctorRequestDto.Languages;

                if (doctorFromDb.ImageFilePath != null && updateDoctorRequestDto.ImageFilePath != null)
                {
                    string bucketName = "hms-main-files";
                    string region = "us-east-1";
                    string s3Url = $"https://{bucketName}.s3.{region}.amazonaws.com/{updateDoctorRequestDto.ImageFilePath}";
                    var uri = new Uri(doctorFromDb.ImageFilePath);
                    string key = uri.AbsolutePath.TrimStart('/');

                    var request = new DeleteObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key
                    };

                    await _amazonS3.DeleteObjectAsync(request);

                    doctorFromDb.ImageFilePath = s3Url;
                }
            }

            return doctorFromDb;
        }

        //We dont need this aymore
        public async Task<bool> DeleteDoc(int? id)
        {
            var doctorFromDb = await _db.Doctors.FindAsync(id);

            if (doctorFromDb != null)
            {
                if (!string.IsNullOrEmpty(doctorFromDb.ApplicationUserId))
                {
                    var appUser = await _db.ApplicationUsers.FindAsync(doctorFromDb.ApplicationUserId);
                    if (appUser != null)
                    {
                        _db.ApplicationUsers.Remove(appUser);
                        _db.Doctors.Remove(doctorFromDb);

                        if (!string.IsNullOrEmpty(doctorFromDb.ImageFilePath))
                        {
                            string bucketName = "hms-main-files";
                            var uri = new Uri(doctorFromDb.ImageFilePath);
                            string key = uri.AbsolutePath.TrimStart('/');

                            var request = new DeleteObjectRequest
                            {
                                BucketName = bucketName,
                                Key = key
                            };

                            await _amazonS3.DeleteObjectAsync(request);
                        }
                    }
                }

                return true;
            }

            return false;
        }

        public async Task<bool> UpdateDocStatus(int? id, bool status)
        {
            var doctorFromDb = await _db.Doctors.FindAsync(id);
            if (doctorFromDb != null)
            {
                doctorFromDb.isActive = status;
                _db.Doctors.Update(doctorFromDb);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
