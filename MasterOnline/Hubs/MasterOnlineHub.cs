using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading;
using System.Threading.Tasks;

namespace MasterOnline.Hubs
{
    [Microsoft.AspNet.SignalR.Hubs.HubName("MasterOnlineHub")]
    public class MasterOnlineHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
        public async Task Announcement(string Pengumuman)
        {
            Clients.All.broadcastmessage(Pengumuman);
        }

        public async Task Notifikasi(string groupName, string message)
        {
            Clients.Group(groupName).monotification(message);
        }
        public Task JoinGroup(string groupName)
        {
            return Groups.Add(Context.ConnectionId, groupName);
        }

        public Task LeaveGroup(string connection_id, string groupName)
        {
            return Groups.Remove(connection_id, groupName);
        }
    }
}