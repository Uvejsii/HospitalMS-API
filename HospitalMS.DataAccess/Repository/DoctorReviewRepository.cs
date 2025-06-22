using HospitalMS.DataAccess.Data;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository
{
    public class DoctorReviewRepository : Repository<DoctorReview>, IDoctorReviewRepository
    {
        private ApplicationDbContext _db;
        public DoctorReviewRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public async Task<DoctorReview?> UpdateAsync(int id, UpdateDoctorReviewRequestDto updateDoctorReviewRequestDto)
        {
            var reviewFromDb = await _db.DoctorReviews.FindAsync(id);
            if (reviewFromDb != null && reviewFromDb.DoctorId == updateDoctorReviewRequestDto.DoctorId)
            {  
                reviewFromDb.Stars = updateDoctorReviewRequestDto.Stars;
                reviewFromDb.Comment = updateDoctorReviewRequestDto.Comment;
                reviewFromDb.UpdatedAt = DateTime.UtcNow;

                return reviewFromDb;
            }
            return null;
        }
    }
}
