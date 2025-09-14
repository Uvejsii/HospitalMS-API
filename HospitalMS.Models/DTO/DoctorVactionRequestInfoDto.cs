using HospitalMS.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.Models.DTO
{
    public class DoctorVacationRequestInfoDto
    {
        public int Id { get; set; }
        public DoctorVacationType VacationType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RequestedDays { get; set; }
    }
}
