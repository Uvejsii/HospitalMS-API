using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.Models.DTO
{
    public class DoctorVacationStatsDto
    {
        public int TotalRequestedDays { get; set; }
        public int PendingRequestedDays { get; set; }
        public int ApprovedRequestedDays { get; set; }
        public int RejectedRequestedDays { get; set; }
        public List<DoctorVacationRequestInfoDto> Pending { get; set; }
        public List<DoctorVacationRequestInfoDto> Approved { get; set; }
        public List<DoctorVacationRequestInfoDto> Rejected { get; set; }
    }
}
