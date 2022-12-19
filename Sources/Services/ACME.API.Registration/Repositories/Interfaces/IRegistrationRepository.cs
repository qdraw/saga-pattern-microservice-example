using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ACME.Library.Domain.Registration;

namespace ACME.API.Registration.Repositories.Interfaces
{
    public interface IRegistrationRepository
    {
        Task<RegistrationData> AddRegistration(RegistrationData registration);
        Task<RegistrationData> GetRegistrationByCorrelationId(Guid correlationId);
        Task<RegistrationData> GetSingleAsync(Expression<Func<RegistrationData, bool>> predicate);
        Task UpdateAsync(RegistrationData data);
        Task SaveChangesAsync();
    }
}