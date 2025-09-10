using Amazon.S3;
using AutoMapper;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;
using HospitalMS_API.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HospitalMS_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AuthController(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            var res = await _unitOfWork.Auth.Login(loginRequestDto);
            if (res == null || !res.Success)
            {
                return BadRequest(res?.ErrorMessage ?? "Error Logging in");
            }

            var response = new Dictionary<string, object>
            {
                { "firstName", res.FirstName },
                { "lastName", res.LastName },
                { "email", res.Email },
                { "id", res.Id },
                { "role", res.Role },
                { "token", res.Token }
            };

            if (!string.IsNullOrEmpty(res.DoctorId))
            {
                response.Add("doctorId", res.DoctorId);
                if (!string.IsNullOrEmpty(res.Department))
                    response.Add("department", res.Department);
                if (!string.IsNullOrEmpty(res.ProfileImage))
                    response.Add("profileImage", res.ProfileImage);
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("RegisterPatient")]
        public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientRequestDto registerPatientRequestDto)
        {
            var appUserDomainModel = _mapper.Map<ApplicationUser>(registerPatientRequestDto);

            var registerResult = await _unitOfWork.Auth.RegisterPatient(registerPatientRequestDto);
            if (!registerResult)
            {
                return BadRequest("Error Registering Patient");
            }

            return Ok(registerResult);
        }

        [HttpPost]
        [Route("RegisterDrFull")]
        public async Task<IActionResult> RegisterDrFull([FromBody] RegisterDoctorFullRequestDto registerDoctorFullRequestDto)
        {
            var (success, appUserId) = await _unitOfWork.Auth.RegisterDoctor(registerDoctorFullRequestDto);
            if (success == false || string.IsNullOrEmpty(appUserId))
            {
                return BadRequest("Error Registering Doctor");
            }

            var doctorDomainModel = _mapper.Map<Doctor>(registerDoctorFullRequestDto);

            string bucketName = "hms-main-files";
            string region = "us-east-1";
            string s3Url = $"https://{bucketName}.s3.{region}.amazonaws.com/{registerDoctorFullRequestDto.ImageFileUrl}";

            doctorDomainModel.ApplicationUserId = appUserId;
            doctorDomainModel.ImageFilePath = s3Url;

            await _unitOfWork.Doctor.CreateAsync(doctorDomainModel);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [HttpPost]
        [Route("RegisterAdmin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminRequestDto registerAdminRequestDto)
        {
            var registerResult = await _unitOfWork.Auth.RegisterAdmin(registerAdminRequestDto);
            if (!registerResult)
            {
                return BadRequest("Error Registering Admin");
            }

            return Ok(registerResult);
        }

        [HttpGet]
        [Route("PingAuth")]
        public async Task<IActionResult> PingAuth()
        {
            var (success, firstName, lastName, roles, userID) = await _unitOfWork.Auth.PingAuth();
            if (!success)
            {
                return BadRequest(new { message = "User not found" });
            }

            return Ok(new { FirstName = firstName, LastName = lastName, Roles = roles, UserId = userID });
        }

        [HttpPost]
        [Route("ResetPasswordCustom")]
        public async Task<IActionResult> ResetPasswordCustom([FromBody] AuthRequestDto resetPasswordRequestDto)
        {
            var result = await _unitOfWork.Auth.ResetPasswordCustom(resetPasswordRequestDto);
            if (!result)
            {
                return BadRequest("Error Changing Password");
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetLoggedInDoctor")]
        public async Task<IActionResult> GetLoggedInDoctor()
        {
            var token = GetJwtTokenFromHeader();
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token not found in Authorization header.");
            }

            var userId = await _unitOfWork.Auth.GetUserIdFromToken(token);
            if (userId == null)
            {
                return BadRequest("DoctorId Not Found");
            }

            var doctorDomainModel = await _unitOfWork.Doctor.GetAsync(
                d => d.ApplicationUserId == userId,
                includeProperties: "Departament,ApplicationUser"
            );

            return Ok(_mapper.Map<LoggedInDoctorDto>(doctorDomainModel));
        }

        private string? GetJwtTokenFromHeader()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }
            return null;
        }


        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _unitOfWork.Auth.Logout();
            if (!result)
            {
                return BadRequest("Couldnt Logout");
            }

            return Ok();
        }

        [HttpPost]
        [Route("SendNotificationToAll")]
        public async Task<IActionResult> SendNotificationToAll([FromBody] NotificationRequestDto notificationRequestDto)
        {
            if (string.IsNullOrEmpty(notificationRequestDto.Message))
            {
                return BadRequest(new { message = "Message cannot be empty." });
            }

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", notificationRequestDto.Message);

            return Ok(new { message = "Notification sent successfully to all users." });
        }

        [HttpPost]
        [Route("SendNotificationToAllDoctors")]
        public async Task<IActionResult> SendNotificationToAllDoctors([FromBody] NotificationRequestDto notificationRequestDto)
        {
            if (string.IsNullOrEmpty(notificationRequestDto.Message))
            {
                return BadRequest(new { message = "Message cannot be empty." });
            }

            await _hubContext.Clients.Group("Doctors").SendAsync("ReceiveMessage", notificationRequestDto.Message);

            return Ok(new { message = "Notification sent successfully to doctors." });
        }

        [HttpPost]
        [Route("SendNotificationToAllAdmins")]
        public async Task<IActionResult> SendNotificationToAllAdmins([FromBody] NotificationRequestDto notificationRequestDto)
        {
            if (string.IsNullOrEmpty(notificationRequestDto.Message))
            {
                return BadRequest(new { message = "Message cannot be empty." });
            }

            await _hubContext.Clients.Group("Admins").SendAsync("ReceiveMessage", notificationRequestDto.Message);

            return Ok(new { message = "Notification sent successfully to admins." });
        }

        [HttpPost]
        [Route("SendNotificationByEmail")]
        public async Task<IActionResult> SendNotificationByEmail([FromBody] NotificationRequestDto notificationRequestDto)
        {
            if (string.IsNullOrEmpty(notificationRequestDto.Email) || string.IsNullOrEmpty(notificationRequestDto.Message))
            {
                return BadRequest(new { message = "Email and message are required." });
            }

            var connectionId = NotificationHub.GetConnectionId(notificationRequestDto.Email);

            if (string.IsNullOrEmpty(connectionId))
            {
                return NotFound(new { message = "User with this email is not connected." });
            }

            await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", notificationRequestDto.Message);

            return Ok(new { message = $"Notification sent to {notificationRequestDto.Email}" });
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _unitOfWork.Auth.GetAllAsync();

            if (users == null || !users.Any())
            {
                return NotFound(new { message = "No users found." });
            }

            return Ok(_mapper.Map<List<AllUsersDto>>(users));
        }

        [HttpGet]
        [Route("GetUsersByName/{name}")]
        public async Task<IActionResult> GetUsersByName([FromRoute] string name)
        {
            var users = await _unitOfWork.Auth.GetAllAsync(u => u.FirstName.ToLower().Contains(name.ToLower()));


            if (users == null || !users.Any())
            {
                return NotFound(new { message = "No users found." });
            }

            return Ok(_mapper.Map<List<AllUsersDto>>(users));
        }

        [HttpGet]
        [Route("GetTotalPatients")]
        public async Task<IActionResult> GetTotalPatients()
        {
            var totalPatients = await _unitOfWork.Auth.GetTotalPatients();
            if (totalPatients == 0)
            {
                return Ok(0);
            }

            return Ok(totalPatients);
        }

        [HttpGet]
        [Route("GetAllPatients")]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _unitOfWork.Auth.GetAllPatietns();
            if (patients == null || !patients.Any())
            {
                return Ok(new List<AllUsersDto>());
            }
            return Ok(patients);
        }

        [HttpPut]
        [Route("ChangePasswordCustom")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordRequestDto)
        {
            if (changePasswordRequestDto == null)
            {
                return BadRequest("Invalid request data.");
            }
            var result = await _unitOfWork.Auth.ChangePassword(changePasswordRequestDto);
            if (!result)
            {
                return BadRequest("Error changing password.");
            }
            return Ok();
        }
    }
}
