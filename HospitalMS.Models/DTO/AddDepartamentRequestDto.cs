using HospitalMS.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.Models.DTO
{
    public class AddDepartamentRequestDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Location { get; set; }
    }
}
