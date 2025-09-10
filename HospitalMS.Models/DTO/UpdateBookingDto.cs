using HospitalMS.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.Models.DTO
{
    public class UpdateBookingDto
    {
        public int Id { get; set; }
        public BookingStatus Status { get; set; }
        public string Prescription { get; set; }
    }
}
