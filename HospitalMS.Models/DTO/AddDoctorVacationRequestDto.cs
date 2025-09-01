using HospitalMS.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.Models.DTO
{
    public class AddDoctorVacationRequestDto
    {
        [Required]
        public int DoctorId { get; set; }
        [Required]
        public DoctorVacationType VacationType { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
    }
}
