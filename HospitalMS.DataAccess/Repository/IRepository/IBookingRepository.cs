using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository.IRepository
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<Booking?> UpdateAsync(Booking booking);
    }
}
