﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.Models.DTO
{
    public class UpdateDoctorRequestDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int YearsOfExperience { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public decimal ConsultationFee { get; set; }
        public bool isAvailable { get; set; }
        public int DepartamentId { get; set; }
        public IFormFile? Image { get; set; }
        public string ImageFileName { get; set; }
    }
}
