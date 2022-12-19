using ACME.Library.Domain.Interfaces.Saga;

namespace ACME.API.Notifications.Models;

public class NotificationModel : ISagaData
{
    public int Id { get; set; }
    
    public string Message { get; set; }
    
    public DateTime TimeStamp { get; set; }
    public Guid CorrelationId { get; set; }
    public string State { get; set; }
}