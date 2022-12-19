using AutoMapper;
using ACME.Identity.Models;
using ACME.Identity.Services.Interfaces;
using ACME.Library.Common.Bus;
using ACME.Library.RabbitMq.Messages.Notifications;
using ACME.Library.RabbitMq.Messages.Registration;

namespace ACME.Identity.Services
{
    public class QueueService : IQueueService
    {
        private readonly IMessageBus _bus;
        private readonly IMapper _mapper;

        public QueueService(IMessageBus bus, IMapper mapper)
        {
            _bus = bus;
            _mapper = mapper;
        }
        
        public async Task CreateNewUser(RegisterUserModel data)
        {
            // Event is RegistrationApprovedEvent due the fact that is triggered after creation of a registration
            await _bus.PublishAsync(_mapper.Map<RegistrationApprovedEvent>(data));
        }

        public async Task SendNotification(NotificationEvent data)
        {
            await _bus.PublishAsync(data);
        }
    }
}