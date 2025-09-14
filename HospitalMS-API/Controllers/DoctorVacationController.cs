using AutoMapper;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DoctorVacationController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public DoctorVacationController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("RequestVacation")]
        public async Task<IActionResult> RequestDoctorVacation([FromBody] AddDoctorVacationRequestDto addDoctorVacationRequestDto)
        {
            var doctorVacationModel = _mapper.Map<DoctorVacation>(addDoctorVacationRequestDto);
            var result = await _unitOfWork.DoctorVacation.RequestDoctorVacation(doctorVacationModel);
            if (result.Success)
            {
                await _unitOfWork.SaveAsync();
                return Ok();
            }
            else
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }
        }

        [HttpPut]
        [Route("UpdateVacation")]
        public async Task<IActionResult> UpdateVacation([FromQuery] int vacationId, bool status)
        {
            var result = await _unitOfWork.DoctorVacation.UpdateDoctorVacationStatus(vacationId, status);
            if (!result)
            {
                return NotFound(new { Message = "Vacation request not found." });
            }

            await _unitOfWork.SaveAsync();
            return Ok();
        }

        [HttpGet]
        [Route("GetAllVacationRequests")]
        public async Task<IActionResult> GetAllDoctorVacations()
        {
            var vacations = await _unitOfWork.DoctorVacation.GetAllAsync();
            if (vacations == null || !vacations.Any())
            {
                return Ok(new List<DoctorVacation>());
            }
            return Ok(vacations);
        }

        [HttpGet]
        [Route("GetAllApprovedVacationRequests")]
        public async Task<IActionResult> GetAllApprovedVacationRequests()
        {
            var vacations = await _unitOfWork.DoctorVacation.GetAllAsync(v => v.IsApproved == true, includeProperties: "Doctor.Departament");
            if (vacations == null || !vacations.Any())
            {
                return Ok(new List<DoctorVacation>());
            }
            return Ok(vacations);
        }

        [HttpGet]
        [Route("GetDoctorVacationsByDrId")]
        public async Task<IActionResult> GetDoctorVacations([FromQuery] int doctorId)
        {
            var vacations = await _unitOfWork.DoctorVacation.GetAllAsync(v => v.DoctorId == doctorId);
            if (vacations == null || !vacations.Any())
            {
                return Ok(new List<DoctorVacation>());
            }

            return Ok(vacations);
        }

        [HttpGet]
        [Route("GetTotalVacationRequestsByDrId")]
        public async Task<IActionResult> GetDoctorVacationRequestsByDrId([FromQuery] int doctorId)
        {
            var stats = await _unitOfWork.DoctorVacation.GetDoctorVacationStatsByDoctorId(doctorId);
            
            return Ok(stats);
        }

        [HttpGet]
        [Route("CheckIfVacationExists")]
        public async Task<IActionResult> CheckIfVacationExists([FromQuery] int doctorId, DateTime startDate, DateTime endDate)
        {
            var exists = await _unitOfWork.DoctorVacation.CheckIfDoctorVacationExists(doctorId, startDate, endDate);
            return Ok(exists);
        }
    }
}
