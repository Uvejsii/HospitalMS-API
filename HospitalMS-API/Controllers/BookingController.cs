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
            var bookingsModel = await _unitOfWork.Booking.GetAllAsync(includeProperties: "User,Doctor.Reviews,Doctor.Departament");

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
        [Route("UpdateBookingStatus")]
        public async Task<IActionResult> UpdateBookingStatus([FromBody] UpdateBookingDto updateBookingDto)
        {
            var bookingDomainModel = _mapper.Map<Booking>(updateBookingDto);

            bookingDomainModel = await _unitOfWork.Booking.UpdateAsync(bookingDomainModel);

            if (bookingDomainModel == null)
            {
                return BadRequest("Booking not found");
            }

            _mapper.Map<BookingDto>(bookingDomainModel);

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
        [Route("GetBookingsByDoctorId")]
        public async Task<IActionResult> GetBookingByDoctorId([FromQuery] int doctorId)
        {
            var bookingsModel = await _unitOfWork.Booking.GetAllAsync(b => b.DoctorId == doctorId, includeProperties: "User");
            List<DoctorBookingDto> bookings = _mapper.Map<List<DoctorBookingDto>>(bookingsModel);
            if (bookings == null || !bookings.Any())
            {
                return Ok(new List<DoctorBookingDto>());
            }
            return Ok(bookings);
        }

        [HttpGet]
        [Route("GetBookingsByUserId")]
        public async Task<IActionResult> GetBookingByUserId([FromQuery] string userId)
        {
            var bookingsModel = await _unitOfWork.Booking.GetAllAsync(b => b.UserId == userId, includeProperties: "User,Doctor");
            List<BookingDto> bookings = _mapper.Map<List<BookingDto>>(bookingsModel);
            if (bookings == null || !bookings.Any())
            {
                return Ok(new List<BookingDto>());
            }
            return Ok(bookings);
        }

        [HttpGet]
        [Route("GetPatientsByDoctorId")]
        public async Task<IActionResult> GetPatientsByDoctorId([FromQuery] int doctorId)
        {
            var bookingsDomainModel = await _unitOfWork.Booking.GetAllAsync(p => p.DoctorId == doctorId, includeProperties: "User,Doctor");
            if (bookingsDomainModel == null || !bookingsDomainModel.Any())
            {
                return Ok(new List<object>());
            }

            var patientStats = bookingsDomainModel
                .GroupBy(b => b.UserId)
                .Select(g => new
                {
                    User = _mapper.Map<DoctorReviewerDto>(g.First().User),
                    TotalBookings = g.Count(),
                    TotalSpent = g.Where(b => b.Status == BookingStatus.Finished).Sum(b => b.Price)
                })
                .ToList();

            return Ok(patientStats);
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

        [HttpGet]
        [Route("GetTotalBookings")]
        public async Task<IActionResult> GetTotalBookings()
        {
            var bookingModel = await _unitOfWork.Booking.GetAllAsync();

            if (bookingModel == null || !bookingModel.Any())
            {
                return Ok(new
                {
                    overallTotal = 0,
                    pendingTotal = 0,
                    confirmedTotal = 0,
                    ongoingTotal = 0,
                    finishedTotal = 0,
                    cancelledTotal = 0
                });
            }

            var bookingCounts = bookingModel.GroupBy(b => b.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var response = new
            {
                overallTotal = bookingModel.Count,
                pendingTotal = bookingCounts.ContainsKey(BookingStatus.Pending) ? bookingCounts[BookingStatus.Pending] : 0,
                confirmedTotal = bookingCounts.ContainsKey(BookingStatus.Confirmed) ? bookingCounts[BookingStatus.Confirmed] : 0,
                ongoingTotal = bookingCounts.ContainsKey(BookingStatus.Ongoing) ? bookingCounts[BookingStatus.Ongoing] : 0,
                finishedTotal = bookingCounts.ContainsKey(BookingStatus.Finished) ? bookingCounts[BookingStatus.Finished] : 0,
                cancelledTotal = bookingCounts.ContainsKey(BookingStatus.Cancelled) ? bookingCounts[BookingStatus.Cancelled] : 0
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("GetMonthlyBookingTotals")]
        public async Task<IActionResult> GetMonthlyBookingTotals()
        {
            var bookings = await _unitOfWork.Booking.GetAllAsync(b => b.Status == BookingStatus.Finished);

            if (bookings == null || !bookings.Any())
            {
                return Ok(new List<object>());
            }

            var monthlyTotals = bookings
                .GroupBy(b => new { b.StartTime.Year, b.StartTime.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalPrice = g.Sum(b => b.Price)
                })
                .ToList();

            return Ok(monthlyTotals);
        }

        [HttpGet]
        [Route("GetTodayBookingsByDoctorId")]
        public async Task<IActionResult> GetTodayBookingsByDoctorId([FromQuery] int doctorId)
        {
            var today = DateTime.Today;

            var bookings = await _unitOfWork.Booking.GetAllAsync(
                b => b.DoctorId == doctorId &&
                     b.Status != BookingStatus.Finished &&
                     b.Status != BookingStatus.Cancelled &&
                     b.StartTime.Date == today,
                includeProperties: "User"
            );

            if (bookings == null || !bookings.Any())
            {
                return Ok(new List<object>());
            }

            var result = bookings.Select(b => new
            {
                b.StartTime,
                b.EndTime,
                User = _mapper.Map<DoctorReviewerDto>(b.User)
            }).ToList();

            return Ok(result);
        }

        [HttpGet]
        [Route("GetTotalPatientsAndEarningsByDoctorId")]
        public async Task<IActionResult> GetTotalPatientsAndEarningsByDoctorId([FromQuery] int doctorId)
        {
            var bookings = await _unitOfWork.Booking
                .GetAllAsync(b => b.DoctorId == doctorId && b.Status == BookingStatus.Finished, includeProperties: "User");

            if (bookings == null || !bookings.Any())
            {
                return Ok(new
                {
                    totalPatients = 0,
                    totalEarnings = 0m
                });
            }

            var totalPatients = bookings.Select(b => b.UserId).Distinct().Count();
            var totalEarnings = bookings.Sum(b => b.Price);
            return Ok(new
            {
                totalPatients,
                totalEarnings
            });
        }
    }
}
