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
        Task<Doctor?> UpdateAsync(int id, Doctor doctor);
        Task<bool> DeleteDoc(int? id);
        Task<Doctor?> UploadDrImage(Doctor doctor);
        Task<bool> ValidateFileUpload(AddDoctorRequestDto addDoctorRequestDto);
        Task<bool> ValidateFileEdit(UpdateDoctorRequestDto updateDoctorRequestDto);
    }
}
