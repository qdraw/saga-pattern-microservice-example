using System.Linq.Expressions;
using ACME.API.Notifications.Data;
using ACME.API.Notifications.Models;
using ACME.API.Notifications.Repositories.Interfaces;
using ACME.Library.Saga.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ACME.API.Notifications.Repositories
{
    public class NotificationRepository : INotificationRepository, ISagaStateRepository<NotificationModel>
    {
        private readonly NotificationsDbContext context;
        private readonly DbSet<NotificationModel> notifications;


        public NotificationRepository(NotificationsDbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            notifications = context.Notifications;
        }

        public async Task<NotificationModel> GetSingleAsync(Expression<Func<NotificationModel, bool>> predicate)
        {
            var notification = await notifications.FirstOrDefaultAsync(predicate);
            if (notification == null) return null;
            context.Attach(notification).State = EntityState.Detached;
            return notification;
        }

        public async Task<List<NotificationModel>> GetByAsync(List<Expression<Func<NotificationModel, bool>>> predicates)
        {
            var query = notifications.AsQueryable();

            foreach (var predicate in predicates)
                query = query.Where(predicate);

            return await query.OrderByDescending(x => x.TimeStamp).ToListAsync();
        }
        

        public async Task UpdateAsync<T>(T item)
        {
            context.Attach(item).State = EntityState.Detached;

            context.Update(item);

            context.Attach(item).State = EntityState.Modified;
            await context.SaveChangesAsync();
            context.Attach(item).State = EntityState.Detached;
        }

        public void Delete<T>(T item)
        {
            var type = item.GetType();
            if (type.IsGenericType)
            {
                context.RemoveRange(item);
            }
            else
            {
                context.Remove(item);
            }
            context.SaveChanges();
        }

        public NotificationModel? GetByCorrelationId(Guid correlationId)
        {
            return context.Notifications.FirstOrDefault(p => p.CorrelationId == correlationId);
        }

        public async Task CreateAsync(NotificationModel item)
        {
            await context.AddAsync(item);       
            await context.SaveChangesAsync();
        }

        public Task UpdateAsync(NotificationModel data)
        {
            throw new NotImplementedException();
        }

        public async Task SaveChangesAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
