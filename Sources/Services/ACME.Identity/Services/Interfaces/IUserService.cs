using ACME.Identity.Models;
using ACME.Library.Domain.Core;

namespace ACME.Identity.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> GetAsync(Guid id);
        Task<User> GetAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task Update(User user);
        Task Delete(Guid id);
        Task<AccountResultModel> Login(LoginModel credential, HttpContext httpContext);
        Task<AccountResultModel> Register(RegisterUserModel model);
    }
}
