using ACME.Library.Domain.Interfaces.Saga;
using ACME.Library.Saga.Abstractions;
using ACME.Library.Saga.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ACME.Library.Saga
{
    public class Saga<TSagaData> : ISaga
        where TSagaData : class, ISagaData, new()
    {
        private readonly ISagaStateRepository<TSagaData> _sagaStateRepo;
        private readonly IList<CorrelationConfiguration> _correlations = new List<CorrelationConfiguration>();

        protected TSagaData Data { get; private set; }
        protected Guid CorrelationId { get; private set; }

        public Saga(ISagaStateRepository<TSagaData> sagaStateRepo)
        {
            _sagaStateRepo = sagaStateRepo;
        }

        public void Initialize<TMessage>(TMessage message)
            where TMessage: class
        {
            Correlate(message);
            LoadData();
        }

        protected void ConfigureCorrelation<TMessage>(Func<TMessage, Guid> messageValueExtractorFunction)
        {
            var config = new CorrelationConfiguration<TMessage>(messageValueExtractorFunction);
            _correlations.Add(config);
        }

        protected void AssertState(params Expression<Func<IState>>[] stateExpressions)
        {
            var allowedStates = stateExpressions.Select(GetName).ToList();

            AssertDataExists();
            
            if (!allowedStates.Contains(Data.State))
            {
                throw new InvalidStateException(allowedStates, Data.State);
            }
        }

        protected void AssertDataNotExists()
        {
            if (Data != null)
            {
                throw new EntityAlreadyExistsException(CorrelationId);
            }
        }

        protected void AssertDataExists()
        {
            if (Data == null)
            {
                throw new EntityNotExistsException(CorrelationId);
            }
        }

        protected Task InstantiateAsync(Expression<Func<IState>> stateExpression)
        {
            return InstantiateAsync(stateExpression, () => new TSagaData());
        }

        protected async Task InstantiateAsync(Expression<Func<IState>> stateExpression, Func<TSagaData> factory)
        {
            AssertDataNotExists();

            var state = GetName(stateExpression);

            var instance = factory();
            instance.CorrelationId = CorrelationId;
            instance.State = state;

            await _sagaStateRepo.CreateAsync(instance);
            await _sagaStateRepo.SaveChangesAsync();

            Data = instance;
        }

        protected async Task UpdateAsync(Expression<Func<IState>> stateExpression)
        {
            AssertDataExists();

            var state = GetName(stateExpression);

            Data.State = state;

            await _sagaStateRepo.UpdateAsync(Data);
            await _sagaStateRepo.SaveChangesAsync();
        }

        protected async Task ChangeStateAsync(Expression<Func<IState>> stateExpression)
        {
            AssertDataExists();

            var state = GetName(stateExpression);

            Data.State = state;

            await _sagaStateRepo.UpdateAsync(Data);
            await _sagaStateRepo.SaveChangesAsync();
        }

        protected async Task UpdateDataAsync()
        {
            AssertDataExists();

            await _sagaStateRepo.UpdateAsync(Data);
            await _sagaStateRepo.SaveChangesAsync();
        }

        private void Correlate<TMessage>(TMessage message)
            where TMessage : class
        {
            var configType = typeof(CorrelationConfiguration<>).MakeGenericType(message.GetType());
            var correlation = _correlations.FirstOrDefault(c => configType.IsInstanceOfType(c));

            if (correlation == null)
            {
                throw new InvalidConfigurationException($"Correlation configuration for message of type '{message.GetType().Name}' is not found");
            }

            var getCorrelationIdMethod = configType.GetMethod(nameof(CorrelationConfiguration<TMessage>.GetCorrelationId));
            CorrelationId = (Guid)getCorrelationIdMethod.Invoke(correlation, new object[] { message });
        }

        private void LoadData()
        {
            Data = _sagaStateRepo.GetByCorrelationId(CorrelationId);
        }

        private string GetName<TField>(Expression<Func<TField>> field)
        {
            return (field.Body as MemberExpression ?? ((UnaryExpression)field.Body).Operand as MemberExpression)?.Member.Name;
        }
    }
}
