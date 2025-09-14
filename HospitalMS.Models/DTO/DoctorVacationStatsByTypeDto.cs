using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.Models.DTO
{
    public class DoctorVacationStatsByTypeDto
    {
        public List<DoctorVacationTypeStatsDto> VacationTypeStats { get; set; }
    }
}
