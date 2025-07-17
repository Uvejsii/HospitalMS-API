using HospitalMS.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.Models.DTO
{
    public class AddBookingRequestDto
    {
        public string Notes { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public decimal Price { get; set; }
        public string ContactPhoneNumber { get; set; }
        public int DoctorId { get; set; }
        public string UserId { get; set; }
        public BookingStatus Status { get; set; }
    }
}
