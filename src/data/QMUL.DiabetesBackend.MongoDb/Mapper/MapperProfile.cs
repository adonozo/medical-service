namespace QMUL.DiabetesBackend.MongoDb.Mapper;

using AutoMapper;
using Model;
using Models;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<ResourceReference, MongoEvent.MongoResourceReference>()
            .ReverseMap();

        CreateMap<HealthEvent, MongoEvent>()
            .ReverseMap();
    }
}