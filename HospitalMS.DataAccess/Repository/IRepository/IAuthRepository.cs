using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository.IRepository
{
    public interface IAuthRepository : IRepository<ApplicationUser>
    {
        Task<bool> RegisterPatient(RegisterPatientRequestDto registerUserRequestDto);
        Task<bool> RegisterDoctor();
        Task<bool> RegisterAdmin(RegisterAdminRequestDto registerAdminRequestDto);
    }
}
