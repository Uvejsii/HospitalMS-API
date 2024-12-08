using AutoMapper;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalMS_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuthController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
            var (success, firstName, lastName, roles) = await _unitOfWork.Auth.PingAuth();
            if (!success)
            {
                return BadRequest("User not found");
            }

            return Ok(new { FirstName = firstName, LastName = lastName, Roles = roles });
        }
    }
}
