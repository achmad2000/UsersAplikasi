//using Microsoft.AspNetCore.SignalR;
//using SharedServices;

//namespace Pengguna.Hubs
//{
//    public class UserHub : Hub
//    {
//        private readonly SignalRBridge _bridge;

//        public UserHub()
//        {
//            // ganti URL ke endpoint AdminHub di project admin
//            _bridge = new SignalRBridge("https://localhost:5202/Hubs/Admin");
//        }

//        public async Task SendToAdmin(string adminId, string message)
//        {
//            await Clients.User(adminId).SendAsync("ReceiveFromUser", message);

//            // forward ke admin server
//            await _bridge.SendAsync("ReceiveFromUser", message);
//        }

//        public async Task NotifyAdmins(string message)
//        {
//            await Clients.All.SendAsync("UserBroadcast", message);
//            await _bridge.SendAsync("AdminBroadcast", message);
//        }
//    }
//}
