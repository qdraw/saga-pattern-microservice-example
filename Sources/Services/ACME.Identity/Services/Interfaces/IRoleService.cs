using Microsoft.AspNetCore.Identity;

namespace ACME.Identity.Services.Interfaces
{
    public interface IRoleService
    {
       Task Create(string name);
       Task Delete(string name);
       IEnumerable<IdentityRole> GetAll();
    }
}
