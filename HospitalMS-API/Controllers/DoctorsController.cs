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

        [HttpPost]
        [Route("CreateDoctor")]
        public async Task<IActionResult> Create([FromForm] AddDoctorRequestDto addDoctorRequestDto)
        {
            if (addDoctorRequestDto.Image == null)
            {
                return BadRequest(new { message = "Image file is required." });
            }

            var doctorDomaniModel = _mapper.Map<Doctor>(addDoctorRequestDto);
            var fileExtension = Path.GetExtension(addDoctorRequestDto.Image.FileName);
            if (string.IsNullOrEmpty(fileExtension))
            {
                return BadRequest(new { message = "File extension is missing or invalid." });
            }

            ValidateFileUpload(addDoctorRequestDto);
            
            doctorDomaniModel.Image = addDoctorRequestDto.Image;
            doctorDomaniModel.ImageFileExtension = fileExtension;
            doctorDomaniModel.ImageFileSizeInBytes = addDoctorRequestDto.Image.Length;
            doctorDomaniModel.ImageFileName = addDoctorRequestDto.ImageFileName;

            await _unitOfWork.Doctor.CreateAsync(doctorDomaniModel);
            await _unitOfWork.Doctor.UploadDrImage(doctorDomaniModel);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        private void ValidateFileUpload(AddDoctorRequestDto requestDto)
        {
            var allowedExtensions = new string[] { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(Path.GetExtension(requestDto.Image.FileName)))
            {
                ModelState.AddModelError("file", "Unsupported file extension");
            }

            if (requestDto.Image.Length > 10485760)
            {
                ModelState.AddModelError("file", "File size more than 10MB, please upload a smaller size file");
            }
        }

        [HttpGet]
        [Route("GetAllDoctors")]
        public async Task<IActionResult> GetAll()
        {
            var doctorsDomainModel = await _unitOfWork.Doctor.GetAllAsync(includeProperties: "Departament");

            return Ok(_mapper.Map<List<DoctorDto>>(doctorsDomainModel));
        }

        [HttpGet]
        [Route("GetDoctorById/{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var doctorDomainModel = await _unitOfWork.Doctor.GetAsync(d => d.Id == id, includeProperties: "Departament");

            if (doctorDomainModel == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<DoctorDto>(doctorDomainModel));
        }

        [HttpPut]
        [Route("EditDoctor/{id:int}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] UpdateDoctorRequestDto updateDoctorRequestDto)
        {
            var doctorDomainModel = _mapper.Map<Doctor>(updateDoctorRequestDto);

            doctorDomainModel = await _unitOfWork.Doctor.UpdateAsync(id, doctorDomainModel);

            if (doctorDomainModel == null)
            {
                return NotFound();
            }

            _mapper.Map<DoctorDto>(doctorDomainModel);

            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("DeleteDoctor/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var doctorDomainModel = await _unitOfWork.Doctor.DeleteDoc(id);

            if (doctorDomainModel == false)
            {
                return NotFound();
            }

            await _unitOfWork.SaveAsync();

            return Ok();
        }
    }
}
