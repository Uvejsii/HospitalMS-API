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

            CreateMap<Departament, DepartamentDto>().ReverseMap();
        }
    }
}
