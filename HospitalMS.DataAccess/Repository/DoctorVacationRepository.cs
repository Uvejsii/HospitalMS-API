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
    public class DoctorVacationRepository : Repository<DoctorVacation>, IDoctorVacationRepository
    {
        private ApplicationDbContext _db;
        public DoctorVacationRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<(bool Success, string? ErrorMessage)> RequestDoctorVacation(DoctorVacation doctorVacation)
        {
            if (doctorVacation.EndDate < doctorVacation.StartDate)
                return (false, "End date cannot be before start date.");

            int year = doctorVacation.StartDate.Year;
            int maxDays = (doctorVacation.VacationType == DoctorVacationType.AnnualLeave || doctorVacation.VacationType == DoctorVacationType.SickLeave) ? 20 : int.MaxValue;
            int daysRequested = (doctorVacation.EndDate - doctorVacation.StartDate).Days + 1;

            var requestedDays = await _db.DoctorVacations
                .Where(v => v.DoctorId == doctorVacation.DoctorId
                    && v.VacationType == doctorVacation.VacationType
                    && v.StartDate.Year == year)
                .SumAsync(v => EF.Functions.DateDiffDay(v.StartDate, v.EndDate) + 1);

            if (requestedDays + daysRequested > maxDays)
                return (false, $"Cannot request more than {maxDays} days of {doctorVacation.VacationType} in {year}.");

            var vacation = new DoctorVacation
            {
                DoctorId = doctorVacation.DoctorId,
                VacationType = doctorVacation.VacationType,
                StartDate = doctorVacation.StartDate,
                EndDate = doctorVacation.EndDate,
                IsApproved = null
            };

            _db.DoctorVacations.Add(vacation);

            return (true, null);
        }

        public async Task<bool> UpdateDoctorVacationStatus(int vacationId, bool status)
        {
            var vacation = await _db.DoctorVacations.FindAsync(vacationId);
            if (vacation == null)
                return false;

            if (status && (vacation.VacationType == DoctorVacationType.AnnualLeave || vacation.VacationType == DoctorVacationType.SickLeave))
            {
                int year = vacation.StartDate.Year;
                int maxDays = 20;
                int daysRequested = (vacation.EndDate - vacation.StartDate).Days + 1;

                var approvedDays = await _db.DoctorVacations
                    .Where(v => v.DoctorId == vacation.DoctorId
                        && v.VacationType == vacation.VacationType
                        && v.IsApproved == true
                        && v.StartDate.Year == year)
                    .SumAsync(v => EF.Functions.DateDiffDay(v.StartDate, v.EndDate) + 1);

                if (approvedDays + daysRequested > maxDays)
                    return false;
            }

            vacation.IsApproved = status;

            return true;
        }

    }
}
