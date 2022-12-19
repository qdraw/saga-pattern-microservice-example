namespace ACME.API.Notifications.Services.Interfaces;

public interface IMessageService
{
    Task SendToAll(string message);
}