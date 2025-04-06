using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.Models.DTO
{
    public class LoggedInDoctorDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImageFilePath { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DepartamentDto Departament { get; set; }
        public int ConsultationFee { get; set; }
    }
}
