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

        public async Task<Booking?> UpdateAsync(int bookingId, Booking booking)
        {
            var bookingFromDb = await _db.Bookings.FindAsync(bookingId);
            if (bookingFromDb != null && bookingFromDb.DoctorId == booking.DoctorId)
            {
                bookingFromDb.DoctorId = booking.DoctorId;
                bookingFromDb.StartTime = booking.StartTime;
                bookingFromDb.EndTime = booking.EndTime;
                bookingFromDb.Status = booking.Status;
                bookingFromDb.Notes = booking.Notes;
                bookingFromDb.ContactPhoneNumber = booking.ContactPhoneNumber;
                bookingFromDb.Price = booking.Price;

                return bookingFromDb;
            }
            return null;
        }
    }
}
