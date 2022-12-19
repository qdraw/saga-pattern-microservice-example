using ACME.API.Notifications.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ACME.API.Notifications.Services;

public class MessageService : IMessageService
{
    private readonly IHubContext<SignalRService, ISignalRService> _hubContext;
    private readonly ILogger<MessageService> _logger;

    public MessageService( IHubContext<SignalRService, ISignalRService> hubContext, ILogger<MessageService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
            
    public async Task SendToAll(string message)
    {
        await _hubContext.Clients.All.SendAsync("Send", message);
    }

}