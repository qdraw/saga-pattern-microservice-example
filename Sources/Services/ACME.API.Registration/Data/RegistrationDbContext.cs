using ACME.Library.Domain.Registration;
using ACME.Library.Outbox.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACME.API.Registration.Data
{
    public class RegistrationDbContext : DbContext
    {
        public RegistrationDbContext()
        {
        }

        public RegistrationDbContext(DbContextOptions<RegistrationDbContext> options) : base(options)
        {
        }

        public DbSet<RegistrationData> Registrations { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Do nothing because of that in debug mode this only triggered
#if (DEBUG) 
            optionsBuilder.EnableSensitiveDataLogging();
#endif
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
} 
