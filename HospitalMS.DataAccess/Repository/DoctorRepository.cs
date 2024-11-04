using HospitalMS.DataAccess.Data;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
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
        public DoctorRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
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
                if (doctor.ImageIrl != null)
                {
                    doctorFromDb.ImageIrl = doctor.ImageIrl;
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
    }
}
