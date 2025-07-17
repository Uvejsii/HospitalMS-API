using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.Models.Domain
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? ApplicationUser { get; set; }
        [NotMapped]
        public IFormFile Image { get; set; }
        public string ImageFileName { get; set; }
        public string ImageFileExtension { get; set; }
        public long ImageFileSizeInBytes { get; set; }
        public string ImageFilePath { get; set; }
        public int YearsOfExperience { get; set; }
        public int DepartamentId { get; set; }
        [ForeignKey("DepartamentId")]
        public Departament Departament { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public decimal ConsultationFee { get; set; }
        public DateTime JoinedDate { get; set; } = DateTime.Now;
        public string? Languages { get; set; }
        public bool isAvailable { get; set; }
        public ICollection<DoctorReview> Reviews { get; set; } = new List<DoctorReview>();
    }
}
