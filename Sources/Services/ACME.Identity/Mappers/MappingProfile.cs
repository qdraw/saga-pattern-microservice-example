using AutoMapper;
using ACME.Identity.Models;
using ACME.Library.Common.Extensions;
using ACME.Library.Domain.Core;
using ACME.Library.RabbitMq.Messages.Registration;
using ACME.Library.RabbitMq.Messages.Users;

namespace ACME.Identity.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EmailAddress));

            CreateMap<ApplicationUser, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => StringExtensions.ToGuidFromString(src.Id)))
                .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.Email));

            CreateMap<RegisterUserModel, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.EmailAddress))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.EmailAddress))
                .ReverseMap();

            CreateMap<RegistrationUsers, UserCreatedEvent>().ReverseMap();

            CreateMap<RegistrationApprovedEvent, UserFailedEvent>().ReverseMap();
            CreateMap<RegistrationApprovedEvent, UserCreatedEvent>().ReverseMap();
            
            CreateMap<RegistrationApprovedEvent, RegistrationUsers>()
                .ForMember(dest => dest.EmailAddress, opt => 
                    opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CorrelationId, opt => 
                    opt.MapFrom(src => src.CorrelationId))
                .ReverseMap();
        }
    }
}
