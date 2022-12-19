using ACME.Identity.Models;
using ACME.Library.RabbitMq.Messages.Notifications;

namespace ACME.Identity.Services.Interfaces
{
    public interface IQueueService
    {
        Task CreateNewUser(RegisterUserModel data);
        Task SendNotification(NotificationEvent data);
    }
}