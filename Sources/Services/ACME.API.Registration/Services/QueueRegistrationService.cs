using ACME.API.Registration.Services.Interfaces;
using System.Threading.Tasks;
using ACME.Library.Common.Bus;
using ACME.Library.Domain.Registration;
using ACME.Library.RabbitMq.Messages.Registration;
using AutoMapper;
using System;

namespace ACME.API.Registration.Services
{
    public class QueueRegistrationService : IQueueRegistrationService
    {
        private readonly IMessageBus _bus;
        private readonly IMapper _mapper;

        public QueueRegistrationService(IMessageBus bus, IMapper mapper)
        {
            _bus = bus;
            _mapper = mapper;
        }

        public async Task CreateRegistration(RegistrationData data)
        {
            var createData = _mapper.Map<CreateRegistrationCommand>(data);
            CreateRegistrationCommand registration = createData;
            await _bus.PublishAsync(registration);
        }
        
        public async Task SubmitRegistration(RegistrationData data)
        {
            SubmitRegistrationCommand registration = (SubmitRegistrationCommand)data;
            await _bus.PublishAsync(registration);
        }

        public async Task<bool> ApproveRegistration(ApproveRegistrationCommand model)
        {
            await _bus.PublishAsync(model);
            return true;
        }
    }
}