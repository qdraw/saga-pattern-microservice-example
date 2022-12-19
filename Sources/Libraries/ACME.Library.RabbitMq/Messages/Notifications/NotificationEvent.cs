using ACME.Library.RabbitMq.Attributes;
using Notification = ACME.Library.Common.Models.Message.Notification;

namespace ACME.Library.RabbitMq.Messages.Notifications
{
    [BusTypeName(nameof(NotificationEvent))]
    public class NotificationEvent
    {   
       public Notification Notification { get; set; }
    } 
} 