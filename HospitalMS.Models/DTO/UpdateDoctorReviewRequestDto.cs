using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.Models.DTO
{
    public class UpdateDoctorReviewRequestDto
    {
        public int Stars { get; set; }
        public string Comment { get; set; }
        public int DoctorId { get; set; }
    }
}
