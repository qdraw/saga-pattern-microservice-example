using ACME.API.Notifications.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ACME.API.Notifications.Services;

public class SignalRService : Hub<ISignalRService>
{
    // nothing here
}