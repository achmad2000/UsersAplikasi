using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
public class JobOrderHub : Hub
{
    public async Task JoinTechnicianGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Technicians");
    }
    public async Task JoinCustomerGroup(string customerName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Customer_{customerName.ToLower()}");
    }
}