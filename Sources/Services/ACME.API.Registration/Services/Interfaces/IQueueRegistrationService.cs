#nullable enable
using System.Threading.Tasks;
using ACME.Library.Domain.Registration;
using ACME.Library.RabbitMq.Messages.Registration;

namespace ACME.API.Registration.Services.Interfaces
{
    public interface IQueueRegistrationService
    {
        Task SubmitRegistration(RegistrationData data);
        Task<bool> ApproveRegistration(ApproveRegistrationCommand model);
        Task CreateRegistration(RegistrationData data);
    }
}