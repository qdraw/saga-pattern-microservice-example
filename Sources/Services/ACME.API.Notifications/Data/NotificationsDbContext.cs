using ACME.API.Notifications.Models;
using ACME.Library.Outbox.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACME.API.Notifications.Data
{
    public class NotificationsDbContext : DbContext
    {
        public NotificationsDbContext()
        {
        }

        public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options)
        {
        }

        public DbSet<NotificationModel> Notifications { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
    }
}
