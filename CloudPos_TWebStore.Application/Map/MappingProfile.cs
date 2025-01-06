using AutoMapper;
using CloudPos_TWebStore.Domain.DataModels;
using CloudPos_WebStore.Application.DTOs;

namespace CloudPos_TWebStore.Application.Map
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CustomerDto, Customer>().ReverseMap();
        }
    }
}
