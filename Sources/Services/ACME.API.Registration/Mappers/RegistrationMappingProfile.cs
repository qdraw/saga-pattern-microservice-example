using ACME.Library.Domain.Registration;
using ACME.Library.RabbitMq.Messages.Registration;
using AutoMapper;

namespace ACME.API.Registration.Mappers
{
    public class RegistrationMappingProfile : Profile
    {
        public RegistrationMappingProfile()
        {
            CreateMap<CreateRegistrationCommand, RegistrationData>().ReverseMap();
            CreateMap<SubmitRegistrationCommand, RegistrationData>().ReverseMap();
            CreateMap<ApproveRegistrationCommand, RegistrationFailedEvent>().ReverseMap();
            CreateMap<RegistrationData, RegistrationApprovedEvent>().ReverseMap();
            CreateMap<RegistrationSucceedEvent, RegistrationData>().ReverseMap();
        }
    }
}