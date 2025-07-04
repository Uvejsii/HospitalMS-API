﻿using HospitalMS.DataAccess.Data;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
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
        private readonly IConfiguration _configuration;

        public AuthRepository(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, 
            SignInManager<ApplicationUser> signInManager, ClaimsPrincipal claimsPrincipal, IConfiguration configuration) : base(db)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _claimsPrincipal = claimsPrincipal;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            var user = await _userManager.FindByEmailAsync(loginRequestDto.Email);

            if (user != null)
            {
                var checkPasswordResult = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

                if (checkPasswordResult)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles != null)
                    {
                        var jwtToken = GenerateJWTToken(user, roles.ToList());

                        var response = new LoginResponseDto
                        {
                            Email = user.Email ?? string.Empty,
                            Role = string.Join(",", roles),
                            Token = jwtToken,
                            Success = true,
                        };

                        return response;
                    }
                }
            }

            return new LoginResponseDto
            {
                Success = false,
                ErrorMessage = "Username or Password Incorrect"
            };
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

        public async Task<(bool Success, string AppUserId)> RegisterDoctor(RegisterDoctorFullRequestDto registerDoctorFullRequestDto)
        {
            if (registerDoctorFullRequestDto.Password != registerDoctorFullRequestDto.ConfirmPassword)
            {
                return (false, null);
            }

            var user = new ApplicationUser
            {
                UserName = registerDoctorFullRequestDto.Email,
                Email = registerDoctorFullRequestDto.Email,
                FirstName = registerDoctorFullRequestDto.FirstName,
                LastName = registerDoctorFullRequestDto.LastName,
            };

            var result = await _userManager.CreateAsync(user, registerDoctorFullRequestDto.Password);
            if (!result.Succeeded)
            {
                return (false, null);
            }

            if (!string.IsNullOrEmpty(registerDoctorFullRequestDto.Role) && await _roleManager.RoleExistsAsync(registerDoctorFullRequestDto.Role))
            {
                await _userManager.AddToRoleAsync(user, registerDoctorFullRequestDto.Role);
                return (true, user.Id);
            }

            return (false, null);
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

        public async Task<bool> Logout()
        {
            await _signInManager.SignOutAsync();
            return true;
        }

        public string GenerateJWTToken(ApplicationUser user, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Task<string> GetUserIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
            {
                return Task.FromResult<string>(null);
            }

            var userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);

            return Task.FromResult(userIdClaim?.Value);
        }
    }
}
