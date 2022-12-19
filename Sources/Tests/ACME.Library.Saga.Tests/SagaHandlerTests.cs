using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ACME.Library.Common.Bus;
using ACME.Library.Domain.Interfaces.Saga;
using ACME.Library.Saga.Abstractions;
using ACME.Library.Saga.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using ACME.Library.Saga;
using Xunit;

namespace ACME.Library.Saga.Tests
{
    public class Tests
    {
        private SagaHandler _sagaHandler;
        private Mock<IServiceProvider> _serviceProviderMock;
        private Mock<ISagaStateRepository<DummyEntity>> _saga1Repo;
        private Mock<ISagaStateRepository<DummyEntity>> _saga2Repo;
        private Saga1 _saga1;
        private Saga2 _saga2;
        private Mock<IMessageBus> _messageConsumer;

        public Tests()
        {
            Setup();
        }
        
        private void Setup()
        {
            _saga1Repo = new Mock<ISagaStateRepository<DummyEntity>>();
            _saga2Repo = new Mock<ISagaStateRepository<DummyEntity>>();

            _messageConsumer = new Mock<IMessageBus>();

            _saga1 = new Saga1(_saga1Repo.Object);
            _saga2 = new Saga2(_saga2Repo.Object);

            var sagaConfigs = new []
            {
                new SagaConfiguration(typeof(Saga1)),
                new SagaConfiguration(typeof(Saga2))
            };

            _serviceProviderMock = BuildServiceProviderMock(_saga1, _saga2);
            
            _sagaHandler = new SagaHandler(new NullLogger<SagaHandler>(), _serviceProviderMock.Object, _messageConsumer.Object, sagaConfigs);
        }

        [Fact]
        public async Task SagaHandler_StartHandler_ConsumersRegistered()
        {
            // Act
            await _sagaHandler.StartAsync(CancellationToken.None);
            
            // Assert
            _messageConsumer.Verify(x => x.ConsumeAsync(It.IsAny<Func<DummyEvent1, Task>>()), Times.Exactly(2));
            _messageConsumer.Verify(x => x.ConsumeAsync(It.IsAny<Func<DummyEvent2, Task>>()), Times.Exactly(1));
            _messageConsumer.Verify(x => x.ConsumeAsync(It.IsAny<Func<DummyEvent3, Task>>()), Times.Exactly(1));
        }


        private Mock<IServiceProvider> BuildServiceProviderMock(params object[] services)
        {
            var serviceProvider = new Mock<IServiceProvider>();

            foreach (var service in services)
            {
                serviceProvider
                    .Setup(x => x.GetService(service.GetType()))
                    .Returns(service);
            }

            var serviceScope = new Mock<IServiceScope>();
            serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);

            var serviceScopeFactory = new Mock<IServiceScopeFactory>();
            serviceScopeFactory
                .Setup(x => x.CreateScope())
                .Returns(serviceScope.Object);

            serviceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(serviceScopeFactory.Object);

            return serviceProvider;
        }
    }

    internal class DummyEntity : ISagaData
    {
        public Guid CorrelationId { get; set; }
        public string State { get; set; }
    }

    internal class DummyEvent1
    {
        public Guid Id { get; set; }
    }

    internal class DummyEvent2
    {
        public Guid Guid { get; set; }
    }

    internal class DummyEvent3
    {
    }

    internal class Saga1 : Saga<DummyEntity>, IMessageHandler<DummyEvent1>, IMessageHandler<DummyEvent2>, IMessageHandler<DummyEvent3>
    {
        public bool IsHandled { get; private set; }

        public Saga1(ISagaStateRepository<DummyEntity> sagaStateRepo)
            : base(sagaStateRepo)
        {
            ConfigureCorrelation<DummyEvent1>(x => x.Id);
            ConfigureCorrelation<DummyEvent2>(x => x.Guid);
        }

        public Task HandleAsync(DummyEvent1 message)
        {
            IsHandled = true;
            return Task.CompletedTask;
        }

        public Task HandleAsync(DummyEvent2 message)
        {
            IsHandled = true;
            return Task.CompletedTask;
        }

        public Task HandleAsync(DummyEvent3 message)
        {
            IsHandled = true;
            return Task.CompletedTask;
        }
    }

    internal class Saga2 : Saga<DummyEntity>, IMessageHandler<DummyEvent1>
    {
        public bool IsHandled { get; private set; }

        public Saga2(ISagaStateRepository<DummyEntity> sagaStateRepo)
            : base(sagaStateRepo)
        {
            ConfigureCorrelation<DummyEvent1>(x => x.Id);
        }

        public Task HandleAsync(DummyEvent1 message)
        {
            IsHandled = true;
            return Task.CompletedTask;
        }
    }
}