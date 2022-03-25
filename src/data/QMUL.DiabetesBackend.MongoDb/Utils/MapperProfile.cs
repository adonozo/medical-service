namespace QMUL.DiabetesBackend.MongoDb.Utils
{
    using AutoMapper;
    using Hl7.Fhir.Model;
    using Model.Extensions;
    using Models;

    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Patient, MongoPatient>()
                .ForMember(mongoPatient => mongoPatient.Email,
                    opt => opt.MapFrom(patient => patient.GetEmailExtension()))
                .ReverseMap()
                .ForSourceMember(mongoPatient => mongoPatient.Email, opt => opt.DoNotValidate());
        }
    }
}