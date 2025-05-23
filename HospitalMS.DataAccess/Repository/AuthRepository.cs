﻿using HospitalMS.DataAccess.Data;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository
{
    public class AuthRepository : Repository<ApplicationUser>, IAuthRepository
    {
        private ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ClaimsPrincipal _claimsPrincipal;

        public AuthRepository(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, 
            SignInManager<ApplicationUser> signInManager, ClaimsPrincipal claimsPrincipal) : base(db)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _claimsPrincipal = claimsPrincipal;
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

        public async Task<bool> RegisterDoctor(RegisterDoctorRequestDto registerDoctorRequestDto)
        {
            if (registerDoctorRequestDto.Password != registerDoctorRequestDto.ConfirmPassword)
            {
                return false;
            }

            var user = new ApplicationUser
            {
                UserName = registerDoctorRequestDto.Email,
                Email = registerDoctorRequestDto.Email,
                FirstName = registerDoctorRequestDto.FirstName,
                LastName = registerDoctorRequestDto.LastName,
            };

            var result = await _userManager.CreateAsync(user, registerDoctorRequestDto.Password);
            if (!result.Succeeded)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(registerDoctorRequestDto.Role) && await _roleManager.RoleExistsAsync(registerDoctorRequestDto.Role))
            {
                await _userManager.AddToRoleAsync(user, registerDoctorRequestDto.Role);
                return true;
            }

            return false;
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

        public async Task<(bool Success, IList<string> Roles)> Login(LoginRequestDto loginRequestDto)
        {
            var user = await _userManager.FindByEmailAsync(loginRequestDto.Email);
            if (user == null)
            {
                return (false, null);
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginRequestDto.Password, loginRequestDto.RememberMe, lockoutOnFailure: false);
            if (!result.Succeeded) 
            { 
                return (false, null);
            }

            var roles = await _userManager.GetRolesAsync(user);

            return (true, roles);
        }

        public async Task<(bool Success, string FirstName, string LastName, IList<string> Roles, string UserId)> PingAuth()
        {
            var userId = _claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return (false, null, null, null, null);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, null, null, null, null);
            }

            var firstName = user.FirstName;
            var lastName = user.LastName;
            var roles = await _userManager.GetRolesAsync(user);
            var userID = userId;

            return (true, firstName, lastName, roles, userID);
        }

        public async Task<bool> ResetPasswordCustom(AuthRequestDto resetPasswordRequestDto)
        {
            var email = resetPasswordRequestDto.Email;

            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var newPassword = $"{user.FirstName}{user.LastName}123!";

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded;
        }

        public async Task<string> GetUserId()
        {
            var userId = _claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            return userId;
        }

        public async Task<bool> Logout()
        {
            await _signInManager.SignOutAsync();
            return true;
        }
    }
}
