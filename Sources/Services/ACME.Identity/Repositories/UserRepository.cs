using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ACME.Identity.Data;
using ACME.Identity.Models;
using ACME.Identity.Repositories.Interfaces;

namespace ACME.Identity.Repositories
{
    public class UserRepository : ApplicationUser, IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly DbSet<ApplicationUser> users;
        private readonly IMapper _mapper;

        public UserRepository(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext)); ;
            users = dbContext.Users;
            _mapper = mapper;
        }

        public async Task<RegistrationUsers?> GetByCorrelationId(Guid correlationId)
        {
            var usersList = await users.Where(x => x.CorrelationId == correlationId).ToListAsync();
            var usr = usersList.FirstOrDefault();
            if(usr == null)
            {
                return null;
            }

            return _mapper.Map<RegistrationUsers>(usersList);
        }

        public IQueryable<ApplicationUser> GetAll()
        {
            var query = users.Include(x => x.CompanyRoles);
            return query;
        }
    }
}