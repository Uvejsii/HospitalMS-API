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
    public class BookingController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookingController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetAllBookings")]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookingsModel = await _unitOfWork.Booking.GetAllAsync(includeProperties: "User,Doctor.Reviews");

            List<BookingDto> bookings = _mapper.Map<List<BookingDto>>(bookingsModel);

            if (bookings == null || !bookings.Any())
            {
                return Ok(new List<BookingDto>());
            }

            return Ok(bookings);
        }

        [HttpPost]
        [Route("CreateBooking")]
        public async Task<IActionResult> CreateBooking([FromBody] AddBookingRequestDto addBookingRequestDto)
        {
            Booking booking = _mapper.Map<Booking>(addBookingRequestDto);

            if (booking == null)
            {
                return BadRequest("Invalid booking data.");
            }
            await _unitOfWork.Booking.CreateAsync(booking);
            await _unitOfWork.SaveAsync();
            return Ok();
        }

        [HttpPut]
        [Route("UpdateBookingStatus/{bookingId:int}/{bookingStatus:int}")]
        public async Task<IActionResult> UpdateBookingStatus([FromRoute] int bookingId, [FromRoute] int bookingStatus)
        {
            Booking booking = await _unitOfWork.Booking.GetAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return BadRequest("Booking not found");
            }

            booking.Status = (BookingStatus)bookingStatus;

            await _unitOfWork.SaveAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("DeleteBooking/{bookingId:int}")]
        public async Task<IActionResult> DeleteBooking([FromRoute] int bookingId)
        {
            await _unitOfWork.Booking.Delete(bookingId);
            await _unitOfWork.SaveAsync();
            return Ok();
        }   
    }
}
