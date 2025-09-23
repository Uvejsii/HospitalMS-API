using HospitalMS.DataAccess.Data;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public AuthRepository(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, 
            SignInManager<ApplicationUser> signInManager, IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : base(db)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
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
                            FirstName = user.FirstName ?? string.Empty,
                            LastName = user.LastName ?? string.Empty,
                            Email = user.Email ?? string.Empty,
                            Role = string.Join(",", roles),
                            Id = user.Id,
                            Token = jwtToken,
                            DoctorId = roles.Contains("Doctor") ? _db.Doctors.Where(d => d.ApplicationUserId == user.Id).Select(d => d.Id.ToString()).FirstOrDefault() : null,
                            Department = roles.Contains("Doctor") ? _db.Doctors
                                .Where(d => d.ApplicationUserId == user.Id)
                                .Include(d => d.Departament)
                                .Select(d => d.Departament.Name)
                                .FirstOrDefault() : null,
                            ProfileImage = roles.Contains("Doctor") ? _db.Doctors
                                .Where(d => d.ApplicationUserId == user.Id)
                                .Select(d => d.ImageFilePath)
                                .FirstOrDefault() : null,
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

            var firstName = registerDoctorFullRequestDto.FirstName;
            var lastName = registerDoctorFullRequestDto.LastName;

            var generatedPassword = $"{firstName}{lastName}123!";

            var user = new ApplicationUser
            {
                UserName = registerDoctorFullRequestDto.Email,
                Email = registerDoctorFullRequestDto.Email,
                FirstName = registerDoctorFullRequestDto.FirstName,
                LastName = registerDoctorFullRequestDto.LastName,
            };

            var result = await _userManager.CreateAsync(user, generatedPassword);
            if (!result.Succeeded)
            {
                return (false, null);
            }

            var role = !string.IsNullOrEmpty(registerDoctorFullRequestDto.Role) ? registerDoctorFullRequestDto.Role : "Doctor";
            if (!string.IsNullOrEmpty(role) && await _roleManager.RoleExistsAsync(role))
            {
                await _userManager.AddToRoleAsync(user, role);
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
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;

            if (claimsPrincipal == null)
            {
                return (false, null, null, null, null);
            }

            var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
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

        public async Task<int> GetTotalPatients()
        {
            var patientRole = await _roleManager.FindByNameAsync("Patient");
            if (patientRole == null)
            {
                return 0;
            }

            var userRoleIds = await _db.UserRoles
                .Where(ur => ur.RoleId == patientRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var patientCount = await _db.ApplicationUsers
                .CountAsync(u => userRoleIds.Contains(u.Id));

            return patientCount;
        }

        public async Task<List<AllUsersDto>> GetAllPatietns()
        {
            var users = await _db.ApplicationUsers.ToListAsync();
            var patientUsers = new List<AllUsersDto>();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, "Patient"))
                {
                    patientUsers.Add(new AllUsersDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email
                    });
                }
            }

            return patientUsers;
        }

        public async Task<List<AllAppUserDoctorsDto>> GetAllActiveAppUserDoctors()
        {
            var appDoctors = await _db.Doctors
                .AsNoTracking()
                .Where(d => d.isActive)
                .Select(d => new AllAppUserDoctorsDto
                {
                    ApplicationUserId = d.ApplicationUserId,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Email = d.Email,
                    ImageFilePath = d.ImageFilePath
                })
                .ToListAsync();

            return appDoctors;
        }

        public Task<bool> ChangePassword(ChangePasswordRequestDto changePasswordRequestDto)
        {
            var user = _userManager.FindByEmailAsync(changePasswordRequestDto.Email).Result;
            if (user == null)
            {
                return Task.FromResult(false);
            }
            var checkPasswordResult = _userManager.CheckPasswordAsync(user, changePasswordRequestDto.OldPassword).Result;
            if (!checkPasswordResult)
            {
                return Task.FromResult(false);
            }
            var result = _userManager.ChangePasswordAsync(user, changePasswordRequestDto.OldPassword, changePasswordRequestDto.NewPassword).Result;
            return Task.FromResult(result.Succeeded);
        }
    }
}
