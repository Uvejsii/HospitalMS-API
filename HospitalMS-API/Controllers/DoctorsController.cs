using AutoMapper;
using Azure.Core;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HospitalMS_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DoctorsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetAllDoctors")]
        public async Task<IActionResult> GetAll([FromQuery]int pageNumber, int pageSize, bool status)
        {
            var doctorsDomainModel = await _unitOfWork.Doctor.GetAllAsync(d => d.isActive == status,
                includeProperties: "Departament,Reviews.Reviewer", pageNumber, pageSize);
            var doctorDtos = _mapper.Map<List<DoctorDto>>(doctorsDomainModel);

            foreach (var dto in doctorDtos)
            {
                var domainDoctor = doctorsDomainModel.FirstOrDefault(d => d.Id == dto.Id);
                if (domainDoctor != null && domainDoctor.Reviews != null && domainDoctor.Reviews.Any())
                {
                    dto.TotalReviewsCount = domainDoctor.Reviews.Count;
                    dto.ReviewStarAverage = Math.Round(domainDoctor.Reviews.Average(r => r.Stars), 2);
                }
                else
                {
                    dto.TotalReviewsCount = 0;
                    dto.ReviewStarAverage = 0;
                }
            }

            return Ok(doctorDtos);
        }

        [HttpGet]
        [Route("GetDoctorById")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var doctorDomainModel = await _unitOfWork.Doctor.GetAsync(d => d.Id == id, includeProperties: "Departament,Reviews.Reviewer");

            if (doctorDomainModel == null)
            {
                return NotFound();
            }

            var doctorDto = _mapper.Map<DoctorDto>(doctorDomainModel);

            if (doctorDomainModel.Reviews != null && doctorDomainModel.Reviews.Any())
            {
                doctorDto.TotalReviewsCount = doctorDomainModel.Reviews.Count;
                doctorDto.ReviewStarAverage = Math.Round(doctorDomainModel.Reviews.Average(r => r.Stars), 2);
            }
            else
            {
                doctorDto.TotalReviewsCount = 0;
                doctorDto.ReviewStarAverage = 0;
            }

            return Ok(doctorDto);
        }

        [HttpPut]
        [Route("EditDoctor")]
        public async Task<IActionResult> Edit([FromQuery] int id, [FromBody] UpdateDoctorRequestDto updateDoctorRequestDto)
        {

            if (updateDoctorRequestDto == null)
            {
                return BadRequest();
            }

            var updatedDoctor = await _unitOfWork.Doctor.UpdateAsync(id, updateDoctorRequestDto);
            if (updatedDoctor == null)
            {
                return NotFound();
            }

            _mapper.Map<Doctor>(updatedDoctor);

            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [HttpPut]
        [Route("UpdateDoctorStatus")]
        public async Task<IActionResult> UpdateDoctorStatus([FromQuery] int id, bool status)
        {
            var doctorDomainModel = await _unitOfWork.Doctor.UpdateDocStatus(id, status);

            if (doctorDomainModel == false)
            {
                return NotFound();
            }

            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [HttpGet]
        [Route("GetTotalDoctors")]
        public async Task<IActionResult> GetTotalDoctors()
        {
            var totalDoctors = await _unitOfWork.Doctor.GetAllAsync(d => d.isActive) ?? Enumerable.Empty<Doctor>();
            return Ok(totalDoctors.Count());
        }

        [HttpGet]
        [Route("GetTotalAvailableDoctors")]
        public async Task<IActionResult> GetTotalAvailableDoctors()
        {
            var allDoctors = await _unitOfWork.Doctor.GetAllAsync() ?? Enumerable.Empty<Doctor>();
            var availableDoctors = allDoctors.Where(d => d.isAvailable == true && d.isActive);
            return Ok(availableDoctors.Count());
        }

        [HttpGet]
        [Route("GetTotalUnavailableDoctors")]
        public async Task<IActionResult> GetTotalUnavailableDoctors()
        {
            var allDoctors = await _unitOfWork.Doctor.GetAllAsync() ?? Enumerable.Empty<Doctor>();
            var unavailableDoctors = allDoctors.Where(d => d.isAvailable == false && d.isActive);
            return Ok(unavailableDoctors.Count());
        }
    }
}
