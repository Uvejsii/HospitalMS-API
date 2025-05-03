using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.Models.DTO
{
    public class NotificationRequestDto
    {
        [Required]
        public string Message { get; set; }
        public string? Email { get; set; }
    }
}
