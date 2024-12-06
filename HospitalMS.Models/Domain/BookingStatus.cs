using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.Models.Domain
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Ongoing,
        Finished,
        Cancelled
    }
}
