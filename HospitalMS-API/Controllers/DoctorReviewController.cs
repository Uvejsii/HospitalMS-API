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
    public class DoctorReviewController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public DoctorReviewController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetDoctorReviewsByDrId/{doctorId:int}")]
        public async Task<IActionResult> GetAllDoctorReviews([FromRoute] int doctorId)
        {
            var reviews = await _unitOfWork.DoctorReview.GetAllAsync(dr => dr.DoctorId == doctorId, includeProperties: "Reviewer");
            if (!reviews.Any())
            {
                return Ok(new List<DoctorReview>());
            }

            return Ok(reviews);
        }

        [HttpPost]
        [Route("CreateDoctorReview")]
        public async Task<IActionResult> CreateDoctorReview([FromBody] AddDoctorReviewRequestDto addDoctorReviewRequestDto)
        {
            var doctorReview = _mapper.Map<DoctorReview>(addDoctorReviewRequestDto);

            if (doctorReview == null)
            {
                return BadRequest("Invalid review data.");
            }

            if (doctorReview.Stars < 1 || doctorReview.Stars > 5)
            {
                return BadRequest("Stars must be between 1 and 5.");
            }

            await _unitOfWork.DoctorReview.CreateAsync(doctorReview);
            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [HttpPut]
        [Route("UpdateDoctorReview/{id:int}")]
        public async Task<IActionResult> UpdateDoctorReview([FromRoute] int id, [FromBody] UpdateDoctorReviewRequestDto updateDoctorReviewRequestDto)
        {
            if (updateDoctorReviewRequestDto == null)
            {
                return BadRequest("Invalid review data.");
            }
            if (updateDoctorReviewRequestDto.Stars < 1 || updateDoctorReviewRequestDto.Stars > 5)
            {
                return BadRequest("Stars must be between 1 and 5.");
            }
            var updatedReview = await _unitOfWork.DoctorReview.UpdateAsync(id, updateDoctorReviewRequestDto);
            if (updatedReview == null)
            {
                return NotFound("Doctor review not found.");
            }
            await _unitOfWork.SaveAsync();
            return Ok();
        }

        [HttpDelete]
        [Route("DeleteDoctorReview/{reviewId:int}/{doctorId:int}")]
        public async Task<IActionResult> DeleteDoctorReview([FromRoute] int reviewId, [FromRoute] int doctorId)
        {
            var review = await _unitOfWork.DoctorReview.GetAsync(r => r.Id == reviewId && r.DoctorId == doctorId);
            if (review == null)
            {
                return NotFound("Doctor review not found.");
            }

            bool isDeleted = await _unitOfWork.DoctorReview.Delete(review.Id);
            if (!isDeleted)
            {
                return BadRequest("Failed to delete the review.");
            }

            await _unitOfWork.SaveAsync();
            return Ok("Doctor review deleted successfully.");
        }
    }
}
