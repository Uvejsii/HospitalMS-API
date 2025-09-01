using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalMS.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IDoctorRepository Doctor { get; }
        IDepartamentRepository Departament { get; }
        IAuthRepository Auth { get; }
        IChatRepository Chat { get; }
        IDoctorReviewRepository DoctorReview { get; }
        IBookingRepository Booking { get; }
        IDoctorVacationRepository DoctorVacation { get; }

        Task SaveAsync();
    }
}
