using AutoMapper;
using HospitalMS.Models.Domain;
using HospitalMS.Models.DTO;

namespace HospitalMS_API.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Doctor, DoctorDto>().ReverseMap();
            CreateMap<AddDoctorRequestDto, Doctor>().ReverseMap();
            CreateMap<UpdateDoctorRequestDto, Doctor>().ReverseMap();
            CreateMap<RegisterDoctorFullRequestDto, Doctor>().ReverseMap();

            CreateMap<Departament, DepartamentDto>().ReverseMap();
            CreateMap<AddDepartamentRequestDto, Departament>().ReverseMap();
            CreateMap<UpdateDepartamentRequestDto, Departament>().ReverseMap();

            CreateMap<RegisterPatientRequestDto, ApplicationUser>().ReverseMap();

            CreateMap<LoggedInDoctorDto, Doctor>().ReverseMap();

            CreateMap<AllUsersDto, ApplicationUser>().ReverseMap();

            CreateMap<ChatMessage, ChatMessageResponseDto>().ReverseMap();
            CreateMap<SendMessageRequestDto, ChatMessage>()
                .ForMember(dest => dest.SentAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsRead, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<DoctorReview, DoctorReviewDto>().ReverseMap();
            CreateMap<AddDoctorReviewRequestDto, DoctorReview>().ReverseMap();
            CreateMap<UpdateDoctorReviewRequestDto, DoctorReview>().ReverseMap();

            CreateMap<DoctorReviewerDto, ApplicationUser>().ReverseMap();
        }
    }
}
