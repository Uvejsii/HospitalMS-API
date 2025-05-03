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
        Task<bool> RegisterDoctor(RegisterDoctorRequestDto registerDoctorRequestDto);
        Task<bool> RegisterAdmin(RegisterAdminRequestDto registerAdminRequestDto);
        Task<(bool Success, IList<string> Roles)> Login(LoginRequestDto loginRequestDto);
        Task<(bool Success, string FirstName, string LastName, IList<string> Roles, string UserId)> PingAuth();
        Task<bool> ResetPasswordCustom(AuthRequestDto resetPasswordRequestDto);
        Task<string> GetUserId();
        Task<bool> Logout();
    }
}
