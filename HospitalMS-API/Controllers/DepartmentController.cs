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
    public class DepartmentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DepartmentController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("CreateDepartment")]
        public async Task<IActionResult> Create([FromBody] AddDepartamentRequestDto addDepartamentRequestDto)
        {
            var departmentDomainModel = _mapper.Map<Departament>(addDepartamentRequestDto);

            await _unitOfWork.Departament.CreateAsync(departmentDomainModel);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [HttpGet]
        [Route("GetAllDepartments")]
        public async Task<IActionResult> GetAll([FromQuery] bool status)
        {
            var departmentsDomainModel = await _unitOfWork.Departament.GetAllAsync(d => d.IsActive == status); // removed: includeProp

            return Ok(_mapper.Map<List<DepartamentDto>>(departmentsDomainModel));
        }

        [HttpGet]
        [Route("GetDepartmentById/{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var departmentDomainModel = await _unitOfWork.Departament.GetAsync(d => d.Id == id, includeProperties: "Doctors");

            if (departmentDomainModel == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<DepartamentDto>(departmentDomainModel));
        }

        [HttpPut]
        [Route("EditDepartment")]
        public async Task<IActionResult> Edit([FromQuery] int id, [FromBody] UpdateDepartamentRequestDto updateDepartamentRequestDto)
        {
            var departmentDomainModel = _mapper.Map<Departament>(updateDepartamentRequestDto);

            departmentDomainModel = await _unitOfWork.Departament.UpdateAsync(id, departmentDomainModel);

            if (departmentDomainModel == null)
            {
                return NotFound();
            }

            _mapper.Map<DepartamentDto>(departmentDomainModel);

            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [HttpPut]
        [Route("UpdateDepartmentStatus")]
        public async Task<IActionResult> UpdateDepartmentStatus([FromQuery] int id, bool status)
        {
            var departmentDomainModel = await _unitOfWork.Departament.UpdateDepartmentStatus(id, status);

            if (departmentDomainModel == false)
            {
                return NotFound();
            }

            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [HttpGet]
        [Route("GetTotalDepartments")]
        public async Task<IActionResult> GetTotalDepartments()
        {
            var totalDepartments = await _unitOfWork.Departament.GetAllAsync() ?? Enumerable.Empty<Departament>();
            return Ok(totalDepartments.Count());
        }

        [HttpGet]
        [Route("GetDoctorCountPerDepartment")]
        public async Task<IActionResult> GetCountPerDepartment()
        {
            var departments = await _unitOfWork.Departament.GetAllAsync(includeProperties: "Doctors");
            if (departments == null || !departments.Any())
            {
                return Ok(new List<Departament>());
            }
            var departmentCounts = departments.Select(d => new
            {
                DepartmentName = d.Name,
                DoctorCount = d.Doctors?.Count ?? 0
            });
            return Ok(departmentCounts);
        }
    }
}
