using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace SharedServices
{
    public class SignalRBridge
    {
        private readonly HubConnection _connection;
        private readonly string _target;

        public SignalRBridge(string targetUrl)
        {
            _target = targetUrl;
            _connection = new HubConnectionBuilder()
                .WithUrl(targetUrl)
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task StartAsync()
        {
            if (_connection.State == HubConnectionState.Disconnected)
                await _connection.StartAsync();
        }

        public async Task SendAsync(string method, string message)
        {
            await StartAsync();
            await _connection.InvokeAsync(method, message);
        }
    }
}
