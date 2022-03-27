namespace QMUL.DiabetesBackend.MongoDb.Mapper
{
    using AutoMapper;
    using Model;
    using Models;

    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<HealthEvent, MongoEvent>()
                .ReverseMap();
        }
    }
}