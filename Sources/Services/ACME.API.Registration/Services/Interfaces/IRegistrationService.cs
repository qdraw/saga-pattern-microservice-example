#nullable enable
using System;
using System.Threading.Tasks;
using ACME.Library.Domain.Registration;

namespace ACME.API.Registration.Services.Interfaces
{
    public interface IRegistrationService
    {
        Task<RegistrationData> AddRegistration(RegistrationData data);
        Task UpdateRegistration(RegistrationData data);
        Task<RegistrationData> GetRegistration(Guid correlationId);
    }
}