using Microsoft.EntityFrameworkCore;
using ACME.API.Registration.Data;
using ACME.API.Registration.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ACME.Library.Domain.Enums.Registration;
using ACME.Library.Domain.Registration;

namespace ACME.API.Registration.Repositories
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly RegistrationDbContext _registrationDbContext;

        public RegistrationRepository(RegistrationDbContext registrationDbContext)
        {
            _registrationDbContext = registrationDbContext;
        }
        
        public async Task<RegistrationData> AddRegistration(RegistrationData registration)
        {
            await _registrationDbContext.Registrations.AddAsync(registration);
            await _registrationDbContext.SaveChangesAsync();
            return registration;
        }
           
        public async Task UpdateAsync(RegistrationData data)
        {
            if (data.RegistrationState != RegistrationStateType.None)
            {
                _registrationDbContext.Update(data);

                await _registrationDbContext.SaveChangesAsync();
            }
        }

        public async Task<RegistrationData> GetRegistrationByCorrelationId(Guid correlationId)
        {
            return await _registrationDbContext.Registrations.FirstOrDefaultAsync(p => p.CorrelationId == correlationId);
        }

        public async Task<RegistrationData> GetSingleAsync(Expression<Func<RegistrationData, bool>> predicate)
        {
            return await _registrationDbContext.Registrations.FirstOrDefaultAsync(predicate);
        }

        public async Task SaveChangesAsync()
        {
            await _registrationDbContext.SaveChangesAsync();
        }
        
    }
}