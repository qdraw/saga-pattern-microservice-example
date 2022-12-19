using System.Linq.Expressions;
using ACME.API.Notifications.Models;
using ACME.Library.Domain.Core;

namespace ACME.API.Notifications.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<NotificationModel> GetSingleAsync(Expression<Func<NotificationModel, bool>> predicate);

    }
}
