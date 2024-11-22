using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.Models.DTO
{
    public class AddDoctorRequestDto
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public int YearsOfExperience { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public decimal ConsultationFee { get; set; }
        [Required]
        public bool isAvailable { get; set; }
        [Required]
        public int DepartamentId { get; set; }
        [Required]
        public IFormFile Image { get; set; }
        [Required]
        public string ImageFileName { get; set; }
    }
}
