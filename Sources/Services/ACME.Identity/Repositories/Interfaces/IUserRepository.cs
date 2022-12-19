using ACME.Identity.Models;

namespace ACME.Identity.Repositories.Interfaces
{
    public interface IUserRepository
    {
        IQueryable<ApplicationUser> GetAll();
        Task<RegistrationUsers?> GetByCorrelationId(Guid correlationId);
    }
}