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
        [Route("GetAllDoctorReviews")]
        public async Task<IActionResult> GetAllDoctorReviews()
        {
            var reviewsModel = await _unitOfWork.DoctorReview.GetAllAsync(includeProperties: "Reviewer,Doctor");

            if (reviewsModel == null || !reviewsModel.Any())
            {
                return Ok(new List<DoctorReviewDto>());
            }

            var reviews = _mapper.Map<List<DoctorReviewDto>>(reviewsModel);

            if (!reviews.Any())
            {
                return Ok(new List<DoctorReviewDto>());
            }

            return Ok(reviews);
        }

        [HttpGet]
        [Route("GetDoctorReviewsByDrId")]
        public async Task<IActionResult> GetDoctorReviewsByDrId([FromQuery] int doctorId)
        {
            var reviewsModel = await _unitOfWork.DoctorReview.GetAllAsync(dr => dr.DoctorId == doctorId, includeProperties: "Reviewer");

            var reviews = _mapper.Map<List<DoctorReviewDto>>(reviewsModel);

            if (!reviews.Any())
            {
                return Ok(new List<DoctorReviewDto>());
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
        [Route("UpdateDoctorReview")]
        public async Task<IActionResult> UpdateDoctorReview([FromQuery] int id, [FromBody] UpdateDoctorReviewRequestDto updateDoctorReviewRequestDto)
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
        [Route("DeleteDoctorReview")]
        public async Task<IActionResult> DeleteDoctorReview([FromQuery] int reviewId, [FromQuery] int doctorId)
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
