﻿using AutoMapper;
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
        [Route("RegisterDoctor")]
        public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorRequestDto registerDoctorRequestDto)
        {
            var registerResult = await _unitOfWork.Auth.RegisterDoctor(registerDoctorRequestDto);
            if (!registerResult)
            {
                return BadRequest("Error Registering Doctor");
            }

            return Ok(registerResult);
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

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            var (success, roles) = await _unitOfWork.Auth.Login(loginRequestDto);
            if (!success)
            {
                return BadRequest("Error Logging in");
            }

            return Ok(new { Message = "Login successful", Roles = roles });
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
            var userId = await _unitOfWork.Auth.GetUserId();
            if (userId == null)
            {
                return BadRequest("DoctorId Not Found");
            }

            var doctorDomainModel = await _unitOfWork.Doctor.GetAsync(d => d.ApplicationUserId == userId, includeProperties: "Departament,ApplicationUser");

            return Ok(_mapper.Map<LoggedInDoctorDto>(doctorDomainModel));
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
    }
}
