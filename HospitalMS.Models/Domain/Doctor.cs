﻿using System;
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
        public string ImageIrl { get; set; }
        public int YearsOfExperience { get; set; }
        public int DepartamentId { get; set; }
        [ForeignKey("DepartamentId")]
        public Departament Departament { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public decimal ConsultationFee { get; set; }
        public DateTime JoinedDate { get; set; } = DateTime.Now;
        public bool isAvailable { get; set; }
    }
}
