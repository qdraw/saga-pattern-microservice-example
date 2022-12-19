using ACME.API.Notifications.Data;
using ACME.API.Notifications.Models;
using ACME.API.Notifications.Services.Interfaces;
using ACME.Library.Common.Outbox;
using ACME.Library.RabbitMq.Messages.Registration;
using ACME.Library.RabbitMq.Messages.Users;
using ACME.Library.Saga;
using ACME.Library.Saga.Abstractions;

namespace ACME.API.Notifications.Sagas
{
    public class NotificationSaga : Saga<NotificationModel>,
        IMessageHandler<ApproveRegistrationCommand>,
         IMessageHandler<SubmitRegistrationCommand>,
        IMessageHandler<UserCreatedEvent>,
        IMessageHandler<UserFailedEvent>,
        IMessageHandler<RegistrationApprovedEvent>,
        IMessageHandler<RegistrationSucceedEvent>,
        IMessageHandler<RegistrationFailedEvent>
    {

        private readonly ILogger<NotificationSaga> _logger;
        private readonly ITransactionalMessagePublisher _messagePublisher;
        private readonly NotificationsDbContext _context;

        public IState Created;
        public IState Approved;
        public IState Succeeded;
        public IState Failed;
        private readonly IMessageService _messageService;

        public NotificationSaga(ILogger<NotificationSaga> logger,
            ISagaStateRepository<NotificationModel> sagaStateRepo,
            ITransactionalMessagePublisher messagePublisher,
            NotificationsDbContext context, IMessageService messageService)
            : base(sagaStateRepo)
        {
            _logger = logger;
            _messagePublisher = messagePublisher;
            _context = context;
            _messageService = messageService;

            ConfigureCorrelation<ApproveRegistrationCommand>(
                e => e.CorrelationId);
            ConfigureCorrelation<SubmitRegistrationCommand>(
                e => e.CorrelationId);
            ConfigureCorrelation<UserCreatedEvent>(
                e => e.CorrelationId);
            ConfigureCorrelation<UserFailedEvent>(
                e => e.CorrelationId);
            ConfigureCorrelation<RegistrationApprovedEvent>(
                e => e.CorrelationId);
            ConfigureCorrelation<RegistrationSucceedEvent>(
                e => e.CorrelationId);
            ConfigureCorrelation<RegistrationFailedEvent>(
                e => e.CorrelationId);
            
        }

        public async Task HandleAsync(ApproveRegistrationCommand message)
        {
            await _messageService.SendToAll( "ApproveRegistrationCommand for " + message.CorrelationId);
        }

        public async Task HandleAsync(SubmitRegistrationCommand message)
        {
            await _messageService.SendToAll( "SubmitRegistrationCommand for " + message.CorrelationId + " with email " + message.Email);
        }

        public async Task HandleAsync(UserCreatedEvent message)
        {
            await _messageService.SendToAll( "UserCreatedEvent for " + message.CorrelationId + " with email " + message.Email);
        }

        public async Task HandleAsync(UserFailedEvent message)
        {
            await _messageService.SendToAll( "UserFailedEvent for " + message.CorrelationId + " with email " + message.Email);
        }

        public async Task HandleAsync(RegistrationApprovedEvent message)
        {
            await _messageService.SendToAll( "RegistrationApprovedEvent for " + message.CorrelationId + " with email " + message.Email);
        }

        public async Task HandleAsync(RegistrationSucceedEvent message)
        {
            await _messageService.SendToAll( "RegistrationSucceedEvent for " + message.CorrelationId + " with email " + message.Email);
        }

        public async Task HandleAsync(RegistrationFailedEvent message)
        {
            await _messageService.SendToAll( "RegistrationFailedEvent for " + message.CorrelationId + " with email " + message.Email);
        }
    }
}