using Microsoft.AspNetCore.Identity;
using ACME.Identity.Services.Interfaces;

namespace ACME.Identity.Services
{
    public class RoleService : IRoleService
    {
        private RoleManager<IdentityRole> _roleManager;
        public RoleService(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task Create(string name)
        {
            var roleExist = await _roleManager.RoleExistsAsync(name);
            if (!roleExist)
            {        
                await _roleManager.CreateAsync(new IdentityRole(name));
            }
        }

        public async Task Delete(string name)
        {
            await _roleManager.DeleteAsync(new IdentityRole(name));
        }

        public IEnumerable<IdentityRole> GetAll()
        {
            return _roleManager.Roles.ToList();
        }
    }
}
