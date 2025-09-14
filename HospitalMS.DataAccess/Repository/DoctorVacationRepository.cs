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

        public async Task<DoctorVacationStatsByTypeDto> GetDoctorVacationStatsByDoctorId(int doctorId)
        {
            var allVacations = await _db.DoctorVacations
                .Where(v => v.DoctorId == doctorId)
                .ToListAsync();

            var result = new DoctorVacationStatsByTypeDto
            {
                VacationTypeStats = Enum.GetValues(typeof(DoctorVacationType))
                    .Cast<DoctorVacationType>()
                    .Select(type =>
                    {
                        var vacationsOfType = allVacations.Where(v => v.VacationType == type).ToList();

                        var pending = vacationsOfType
                            .Where(v => v.IsApproved == null)
                            .Select(v => new DoctorVacationRequestInfoDto
                            {
                                Id = v.Id,
                                VacationType = v.VacationType,
                                StartDate = v.StartDate,
                                EndDate = v.EndDate,
                                RequestedDays = (v.EndDate - v.StartDate).Days + 1
                            })
                            .ToList();

                        var approved = vacationsOfType
                            .Where(v => v.IsApproved == true)
                            .Select(v => new DoctorVacationRequestInfoDto
                            {
                                Id = v.Id,
                                VacationType = v.VacationType,
                                StartDate = v.StartDate,
                                EndDate = v.EndDate,
                                RequestedDays = (v.EndDate - v.StartDate).Days + 1
                            })
                            .ToList();

                        var rejected = vacationsOfType
                            .Where(v => v.IsApproved == false)
                            .Select(v => new DoctorVacationRequestInfoDto
                            {
                                Id = v.Id,
                                VacationType = v.VacationType,
                                StartDate = v.StartDate,
                                EndDate = v.EndDate,
                                RequestedDays = (v.EndDate - v.StartDate).Days + 1
                            })
                            .ToList();

                        return new DoctorVacationTypeStatsDto
                        {
                            VacationType = type,
                            TotalRequestedDays = vacationsOfType.Sum(v => (v.EndDate - v.StartDate).Days + 1),
                            PendingRequestedDays = pending.Sum(p => p.RequestedDays),
                            ApprovedRequestedDays = approved.Sum(a => a.RequestedDays),
                            RejectedRequestedDays = rejected.Sum(r => r.RequestedDays),
                            Pending = pending,
                            Approved = approved,
                            Rejected = rejected
                        };
                    })
                    .ToList()
            };

            return result;
        }

        public async Task<bool> CheckIfDoctorVacationExists(int doctorId, DateTime startDate, DateTime endDate)
        {
            return await _db.DoctorVacations.AnyAsync(v =>
                v.DoctorId == doctorId &&
                v.StartDate <= endDate &&
                v.EndDate >= startDate);
        }
    }
}
