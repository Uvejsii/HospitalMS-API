using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository.IRepository
{
    public interface IDoctorRepository : IRepository<Doctor>
    {
        Task<Doctor?> UpdateAsync(int id, UpdateDoctorRequestDto updateDoctorRequestDto);
        Task<bool> DeleteDoc(int? id);
        Task<bool> UpdateDocStatus(int? id, bool status);
    }
}
