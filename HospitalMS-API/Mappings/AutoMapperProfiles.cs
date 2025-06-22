using AutoMapper;
using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;

namespace HospitalMS_API.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // Doctor Mappings
            CreateMap<Doctor, DoctorDto>().ReverseMap();
            CreateMap<AddDoctorRequestDto, Doctor>().ReverseMap();
            CreateMap<UpdateDoctorRequestDto, Doctor>().ReverseMap();
            CreateMap<RegisterDoctorFullRequestDto, Doctor>().ReverseMap();
            CreateMap<LoggedInDoctorDto, Doctor>().ReverseMap();

            // Department Mappings
            CreateMap<Departament, DepartamentDto>().ReverseMap();
            CreateMap<AddDepartamentRequestDto, Departament>().ReverseMap();
            CreateMap<UpdateDepartamentRequestDto, Departament>().ReverseMap();

            // Patient Mappings
            CreateMap<RegisterPatientRequestDto, ApplicationUser>().ReverseMap();
            CreateMap<AllUsersDto, ApplicationUser>().ReverseMap();

            // Chat Mappings
            CreateMap<ChatMessage, ChatMessageResponseDto>().ReverseMap();
            CreateMap<SendMessageRequestDto, ChatMessage>()
                .ForMember(dest => dest.SentAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsRead, opt => opt.Ignore())
                .ReverseMap();

            // Doctor Review Mappings
            CreateMap<DoctorReview, DoctorReviewDto>().ReverseMap();
            CreateMap<AddDoctorReviewRequestDto, DoctorReview>().ReverseMap();
            CreateMap<UpdateDoctorReviewRequestDto, DoctorReview>().ReverseMap();
            CreateMap<DoctorReviewerDto, ApplicationUser>().ReverseMap();

            // Bookings Mappings
            CreateMap<Booking, AddBookingRequestDto>().ReverseMap();
            CreateMap<BookingDto, Booking>().ReverseMap();
        }
    }
}
