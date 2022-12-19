#nullable enable
using System;
using System.Threading.Tasks;
using ACME.Library.Domain.Interfaces.Saga;

namespace ACME.Library.Saga.Abstractions
{
    public interface ISagaStateRepository<TSagaData>
        where TSagaData : ISagaData
    {
        TSagaData? GetByCorrelationId(Guid correlationId);
        Task CreateAsync(TSagaData data);
        Task UpdateAsync(TSagaData data);
        Task SaveChangesAsync();
    }
}