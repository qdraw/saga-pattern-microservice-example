using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ACME.API.Registration.Data;
using System;
using System.Threading.Tasks;
using ACME.Library.Common.Outbox;
using ACME.Library.Domain.Registration;
using ACME.Library.RabbitMq.Messages.Registration;
using ACME.Library.RabbitMq.Messages.Users;
using ACME.Library.RabbitMq.Strategies.Exceptions;
using ACME.Library.Saga;
using ACME.Library.Saga.Abstractions;

namespace ACME.API.Registration.Sagas
{
    public class RegistrationSaga : Saga<RegistrationData>, IMessageHandler<ApproveRegistrationCommand>, 
        IMessageHandler<CreateRegistrationCommand>,
        IMessageHandler<SubmitRegistrationCommand>, IMessageHandler<UserCreatedEvent>, IMessageHandler<UserFailedEvent>
    {
        
        private readonly ILogger<RegistrationSaga> _logger;
        private readonly IMapper _mapper;
        private readonly ITransactionalMessagePublisher _messagePublisher;
        private readonly RegistrationDbContext _context;

        public IState Created;
        public IState Approved;
        public IState Succeeded;
        public IState Failed;

        public RegistrationSaga(ILogger<RegistrationSaga> logger,
            IMapper mapper,
            ISagaStateRepository<RegistrationData> sagaStateRepo, 
            ITransactionalMessagePublisher messagePublisher, 
            RegistrationDbContext context)
            : base(sagaStateRepo)
        {
            _logger = logger;
            _mapper = mapper;
            _messagePublisher = messagePublisher;
            _context = context;
            
            ConfigureCorrelation<CreateRegistrationCommand>(
                e => e.CorrelationId);
            ConfigureCorrelation<ApproveRegistrationCommand>(
                e => e.CorrelationId);
            ConfigureCorrelation<SubmitRegistrationCommand>(
                e => e.CorrelationId);
            ConfigureCorrelation<UserCreatedEvent>(
                e => e.CorrelationId);
            ConfigureCorrelation<UserFailedEvent>(
                e => e.CorrelationId);
        }
        
        public async Task HandleAsync(CreateRegistrationCommand message)
        {
            _logger.LogInformation($"[{nameof(RegistrationSaga)}/HandleAsync] Handling {nameof(CreateRegistrationCommand)}");

            // Here is the registration created in the database
            await InstantiateAsync(() => Created, () => _mapper.Map<RegistrationData>(message));
            _messagePublisher.Publish();
        }

        public async Task HandleAsync(SubmitRegistrationCommand message)
        {
            _logger.LogInformation($"[{nameof(RegistrationSaga)}/HandleAsync] Handling {nameof(SubmitRegistrationCommand)}");
            await InstantiateAsync(() => Created, () => _mapper.Map<RegistrationData>(message));
           
            await _messagePublisher.CreateMessageAsync(_mapper.Map<RegistrationSubmittedEvent>(Data));
            _messagePublisher.Publish();
        }
        
        /// <summary>
        /// Commands are always successful
        /// </summary>
        /// <param name="message"></param>
        public async Task HandleAsync(ApproveRegistrationCommand message)
        {
            try
            {
                _logger.LogInformation($"[{nameof(RegistrationSaga)}/HandleAsync] Handling {nameof(ApproveRegistrationCommand)}");

                var transaction = await _context.Database.BeginTransactionAsync();
                // gives exception when state is not correct    
                AssertState(() => Created);
                // change after checking state
                await ChangeStateAsync(() => Approved);
            
                // create a new event to send to the company service
                // Events can fail
                await _messagePublisher.CreateMessageAsync(_mapper.Map<RegistrationApprovedEvent>(Data));
            
                await transaction.CommitAsync();

                _messagePublisher.Publish();
            }
            catch (Exception exception) when (exception is SqlException or DbUpdateConcurrencyException)
            {
                _logger.LogError($"Retry NackException {exception.Message}");
                throw new NackException(HandlingStrategy.NackDelayedRequeue, exception);
            }
            catch (Exception exception){
                _logger.LogInformation($"RegistrationFailedEvent {exception.Message}");
                var data = _mapper.Map<RegistrationFailedEvent>(message);
                data.Reason = exception.Message;
                await _messagePublisher.CreateMessageAsync(data);
                _messagePublisher.Publish();
            }
        }

        public async Task HandleAsync(UserCreatedEvent message)
        {
            _logger.LogInformation($"[{nameof(RegistrationSaga)}/HandleAsync] Handling {nameof(UserCreatedEvent)}");
            
            AssertState(() => Approved);
            await ChangeStateAsync(() => Succeeded);
            
            // For status display
            var data = _mapper.Map<RegistrationSucceedEvent>(Data);
            await _messagePublisher.CreateMessageAsync(data);
            _messagePublisher.Publish();
        }

        public async Task HandleAsync(UserFailedEvent message)
        {
            var data = _mapper.Map<RegistrationFailedEvent>(Data);
            data.Reason = message.Reason;
            await _messagePublisher.CreateMessageAsync(data);
            _messagePublisher.Publish();
        }
    }
}