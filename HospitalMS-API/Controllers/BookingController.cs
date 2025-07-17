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
            var start = addBookingRequestDto.StartTime.TimeOfDay;
            var end = addBookingRequestDto.EndTime.TimeOfDay;

            var duration = addBookingRequestDto.EndTime - addBookingRequestDto.StartTime;
            if (duration != TimeSpan.FromMinutes(30))
            {
                return BadRequest("Booking must be for a 30-minute slot.");
            }

            if (addBookingRequestDto.StartTime.Minute != 0 && addBookingRequestDto.StartTime.Minute != 30)
            {
                return BadRequest("Bookings can only start on the hour or half-hour (e.g., 9:00, 9:30).");
            }

            if (addBookingRequestDto.EndTime.Hour < addBookingRequestDto.StartTime.Hour)
            {
                return BadRequest("End time must be after start time.");
            }

            if (addBookingRequestDto.EndTime.Minute != 0 && addBookingRequestDto.EndTime.Minute != 30)
            {
                return BadRequest("Bookings can only end on the hour or half-hour (e.g., 9:30, 10:00).");
            }

            var workStart = new TimeSpan(9, 0, 0);
            var workEnd = new TimeSpan(17, 0, 0);

            if (start < workStart || end > workEnd)
            {
                return BadRequest("Booking must be within working hours (9:00 AM to 5:00 PM).");
            }

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

        [HttpGet]
        [Route("GetBookingsByDoctorId/{doctorId:int}")]
        public async Task<IActionResult> GetBookingByDoctorId([FromRoute] int doctorId)
        {
            var bookingsModel = await _unitOfWork.Booking.GetAllAsync(b => b.DoctorId == doctorId, includeProperties: "User,Doctor.Reviews");
            List<BookingDto> bookings = _mapper.Map<List<BookingDto>>(bookingsModel);
            if (bookings == null || !bookings.Any())
            {
                return Ok(new List<BookingDto>());
            }
            return Ok(bookings);
        }

        [HttpGet]
        [Route("GetBookingsByUserId/{userId}")]
        public async Task<IActionResult> GetBookingByUserId([FromRoute] string userId)
        {
            var bookingsModel = await _unitOfWork.Booking.GetAllAsync(b => b.UserId == userId, includeProperties: "User,Doctor.Reviews");
            List<BookingDto> bookings = _mapper.Map<List<BookingDto>>(bookingsModel);
            if (bookings == null || !bookings.Any())
            {
                return Ok(new List<BookingDto>());
            }
            return Ok(bookings);
        }

        [HttpGet]
        [Route("GetFreeBookingSlotsByDay")]
        public async Task<IActionResult> GetFreeBookingSlotsByDay([FromQuery] int doctorId, [FromQuery] int day)
        {
            var currentDate = DateTime.Now;
            var date = new DateTime(currentDate.Year, currentDate.Month, day);

            var bookings = await _unitOfWork.Booking.GetAllAsync(
                b => b.DoctorId == doctorId &&
                     b.StartTime.Date == date.Date
            );

            var slots = new List<(TimeSpan Start, TimeSpan End)>();
            var workStart = new TimeSpan(9, 0, 0);
            var workEnd = new TimeSpan(17, 0, 0);
            var breakStart = new TimeSpan(12, 0, 0);
            var breakEnd = new TimeSpan(13, 0, 0);

            for (var time = workStart; time < workEnd; time = time.Add(TimeSpan.FromMinutes(30)))
            {
                var slotEnd = time.Add(TimeSpan.FromMinutes(30));
                if (slotEnd <= breakStart || time >= breakEnd)
                {
                    slots.Add((time, slotEnd));
                }
            }

            TimeSpan TruncateToMinute(TimeSpan t) => new TimeSpan(t.Hours, t.Minutes, 0);

            var freeSlots = slots.Where(slot =>
                !bookings.Any(b =>
                    TruncateToMinute(b.StartTime.TimeOfDay) < slot.End &&
                    TruncateToMinute(b.EndTime.TimeOfDay) > slot.Start
                )
            )
            .Select(slot => new
            {
                Start = date.Date.Add(slot.Start),
                End = date.Date.Add(slot.End)
            })
            .ToList();

            return Ok(freeSlots);
        }
    }
}
