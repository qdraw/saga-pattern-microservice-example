using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ACME.Identity.Data;
using ACME.Identity.Models;
using ACME.Identity.Services.Interfaces;
using ACME.Library.Common.Outbox;
using ACME.Library.RabbitMq.Messages.Registration;
using ACME.Library.RabbitMq.Messages.Users;
using ACME.Library.RabbitMq.Strategies.Exceptions;
using ACME.Library.Saga;
using ACME.Library.Saga.Abstractions;

namespace ACME.Identity.Sagas
{
    public class UserSaga : Saga<RegistrationUsers>, 
        IMessageHandler<RegistrationApprovedEvent>,
        IMessageHandler<RegistrationSubmittedEvent>
    {
        private readonly ILogger<UserSaga> _logger;
        private readonly IMapper _mapper;
        private readonly ITransactionalMessagePublisher _messagePublisher;
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly IQueueService _queueService;

        public IState Created;
        public IState Succeeded;
        public IState Failed;

        public UserSaga(ILogger<UserSaga> logger,
            IMapper mapper,
            ISagaStateRepository<RegistrationUsers> sagaStateRepo,
            IQueueService queueService,
            ITransactionalMessagePublisher messagePublisher,
            ApplicationDbContext context)
            : base(sagaStateRepo)
        {
            _logger = logger;
            _mapper = mapper;
            _messagePublisher = messagePublisher;
            _queueService = queueService;
            _context = context;
            ConfigureCorrelation<RegistrationApprovedEvent>(
                e => e.CorrelationId);
            ConfigureCorrelation<RegistrationSubmittedEvent>(
                e => e.CorrelationId);
        }

        public async Task HandleAsync(RegistrationApprovedEvent message)
        {
            try
            {
                _logger.LogInformation($"[{nameof(UserSaga)}/HandleAsync] Handling {nameof(RegistrationApprovedEvent)}");

                var transaction = await _context.Database.BeginTransactionAsync();

                var user = _mapper.Map<RegistrationUsers>(message);
                user.CorrelationId  = message.CorrelationId;
                
                await InstantiateAsync(() => Created, () => user);

                await _messagePublisher.CreateMessageAsync(_mapper.Map<UserCreatedEvent>(Data));

                _messagePublisher.Publish();

                await transaction.CommitAsync();
            }
            catch (Exception exception) when (exception is SqlException or DbUpdateConcurrencyException)
            {
                _logger.LogError($"Retry NackException {exception.Message}");
                throw new NackException(HandlingStrategy.NackDelayedRequeue, exception);
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"UserFailedEvent {exception.Message}");
                var data = _mapper.Map<UserFailedEvent>(message);
                data.Reason = exception.Message;
                await _messagePublisher.CreateMessageAsync(data);
                _messagePublisher.Publish();
            }
        }

        public async Task HandleAsync(RegistrationSubmittedEvent message)
        {
            // try
            // {
            //     var users = await _userService.GetUsersForRegistrationApproval();
            //     var recipients = new List<Recipient>();
            //     foreach (var user in users)
            //     {
            //         //TODO change to NotificationChannels preferences of user when preferences are built
            //         //TODO change to Language when language is correctly stored in IDS on USER profile
            //         recipients.Add(new Recipient() { Name = user.FirstName, Email = user.EmailAddress , NotificationChannels = new List<NotificationType> { NotificationType.Mail, NotificationType.MessageCenter }, Locale = LocaleType.nl });
            //     }
            //
            //     if (recipients.Any())
            //     {
            //         var notification = new NotificationEvent()
            //         {
            //             Notification = new Notification()
            //             {
            //                 Key = NotificationKeyType.RegistrationSubmitted,
            //                 Recipients = recipients,
            //                 ReplaceTags = new Dictionary<string, string>() { { "$REGISTRATIONID$", message.CorrelationId.ToString() } }
            //             }
            //         };
            //         await _queueService.SendNotification(notification);
            //     }
            //     else
            //     {
            //         _logger.LogError($"RegistrationSubmitted no recipients");
            //     }
            // }
            // catch (Exception exception)
            // {
            //     _logger.LogError($"RegistrationSubmittedFailedEvent {exception.Message}");
            //
            // }
        }
    }
}