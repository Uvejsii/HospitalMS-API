using HospitalMS.DataAccess.Data;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository
{
    public class AuthRepository : Repository<ApplicationUser>, IAuthRepository
    {
        private ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthRepository(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : base(db)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<bool> RegisterAdmin(RegisterAdminRequestDto registerAdminRequestDto)
        {
            if (registerAdminRequestDto.Password != registerAdminRequestDto.ConfirmPassword)
            {
                return false;
            }

            var user = new ApplicationUser
            {
                UserName = registerAdminRequestDto.Email,
                Email = registerAdminRequestDto.Email,
                FirstName = registerAdminRequestDto.FirstName,
                LastName = registerAdminRequestDto.LastName,
            };

            var result = await _userManager.CreateAsync(user, registerAdminRequestDto.Password);
            if (!result.Succeeded)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(registerAdminRequestDto.Role) && await _roleManager.RoleExistsAsync(registerAdminRequestDto.Role))
            {
                await _userManager.AddToRoleAsync(user, registerAdminRequestDto.Role);
                return true;
            }

            return false;
        }

        public Task<bool> RegisterDoctor()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RegisterPatient(RegisterPatientRequestDto registerPatientRequestDto)
        {
            if (registerPatientRequestDto.Password != registerPatientRequestDto.ConfirmPassword)
            {
                return false;
            }

            var user = new ApplicationUser
            {
                UserName = registerPatientRequestDto.Email,
                Email = registerPatientRequestDto.Email,
                FirstName = registerPatientRequestDto.FirstName,
                LastName = registerPatientRequestDto.LastName,
            };  

            var result = await _userManager.CreateAsync(user, registerPatientRequestDto.Password);
            if (!result.Succeeded)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(registerPatientRequestDto.Role) && await _roleManager.RoleExistsAsync(registerPatientRequestDto.Role))
            {
                await _userManager.AddToRoleAsync(user, registerPatientRequestDto.Role);
                return true;
            }

            return false;
        }
    }
}
