namespace ACME.API.Notifications.Services.Interfaces;

/// <summary>
/// Keep this interface direct to SignalR Specs
/// </summary>
public interface ISignalRService
{
    Task SendAsync(string method, object? arg1, CancellationToken cancellationToken = default);
}