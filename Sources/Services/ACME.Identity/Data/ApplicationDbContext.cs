
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ACME.Identity.Models;
using ACME.Library.Outbox.EntityFramework.Entities;

namespace ACME.Identity.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Do nothing because of that in debug mode this only triggered
#if (DEBUG)
        optionsBuilder.EnableSensitiveDataLogging();
#endif
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<CompanyUserRoleEntity>()
                .HasKey(p => new { p.CompanyCode, p.UserId, p.CompanyRole });
        builder.Entity<CompanyUserRoleEntity>()
                .HasOne(x => x.User)
                .WithMany(x => x.CompanyRoles).HasForeignKey(f => f.UserId);

        base.OnModelCreating(builder);
    }
}