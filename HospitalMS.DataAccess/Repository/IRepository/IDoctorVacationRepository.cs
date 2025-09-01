using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;

namespace HospitalMS.DataAccess.Repository.IRepository
{
    public interface IDoctorVacationRepository : IRepository<DoctorVacation>
    {
        Task<(bool Success, string? ErrorMessage)> RequestDoctorVacation(DoctorVacation doctorVacation);
        Task<bool> UpdateDoctorVacationStatus(int vacationId, bool status);
    }
}
