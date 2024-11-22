using HospitalMS.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.Models.DTO
{
    public class DoctorDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImageFileName { get; set; }
        public string ImageFilePath { get; set; }
        public int YearsOfExperience { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public decimal ConsultationFee { get; set; }
        public bool isAvailable { get; set; }
        public DepartamentDto Departament { get; set; }
    }
}
