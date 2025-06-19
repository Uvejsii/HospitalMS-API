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
        string GenerateJWTToken(ApplicationUser user, List<string> roles);
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
        Task<bool> RegisterPatient(RegisterPatientRequestDto registerUserRequestDto);
        Task<(bool Success, string AppUserId)> RegisterDoctor(RegisterDoctorFullRequestDto registerDoctorFullRequestDto);
        Task<bool> RegisterAdmin(RegisterAdminRequestDto registerAdminRequestDto);
        Task<(bool Success, string FirstName, string LastName, IList<string> Roles, string UserId)> PingAuth();
        Task<bool> ResetPasswordCustom(AuthRequestDto resetPasswordRequestDto);
        Task<string> GetUserIdFromToken(string token);
        Task<bool> Logout();
    }
}
