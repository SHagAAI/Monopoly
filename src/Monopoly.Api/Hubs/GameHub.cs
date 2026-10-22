using Microsoft.AspNetCore.SignalR;

namespace Monopoly.Api.Hubs
{
    public class GameHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}
