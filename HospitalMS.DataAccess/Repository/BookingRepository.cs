using HospitalMS.DataAccess.Data;
using HospitalMS.DataAccess.Repository.IRepository;
using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private ApplicationDbContext _db;
        public BookingRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Booking?> UpdateAsync(Booking booking)
        {
            var bookingFromDb = await _db.Bookings.FindAsync(booking.Id);
            if (bookingFromDb != null)
            {
                bookingFromDb.Status = booking.Status;
                bookingFromDb.Prescription = booking.Prescription;

                return bookingFromDb;
            }
            return null;
        }
    }
}
