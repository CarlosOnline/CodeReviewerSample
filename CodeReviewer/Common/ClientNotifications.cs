using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading.Tasks;
using System.Web.Routing;
using System.Web.Script.Serialization;
using Microsoft.AspNet.SignalR;

#if OldConnection
public class ClientConnection : PersistentConnection
{
    public static ClientConnection Broadcaster { get; set; }

    protected override Task OnConnected(IRequest request, string connectionId)
    {
        Broadcaster = this;

        return Connection.Broadcast("Connection " + connectionId + " connected");
    }

    protected override Task OnReceived(IRequest request, string connectionId, string data)
    {
        Broadcaster = this;

        // Broadcast data to all clients
        return Connection.Broadcast(data);
    }

    public static void Broadcast(object data)
    {
#if DEBUG
        var js = new JavaScriptSerializer();
        System.Diagnostics.Debug.WriteLine("Broadcasting:");
        System.Diagnostics.Debug.WriteLine("Broadcasting {0}", js.Serialize(data));
#endif        

        if (Broadcaster != null)
            Broadcaster.Connection.Broadcast(data);
    }
}
#endif

public class ChangeListHub : Hub
{
    public void Join(string changeList)
    {
        Groups.Add(Context.ConnectionId, changeList);
    }

    public static void Broadcast(object data, string changeList)
    {
#if DEBUG
        var js = new JavaScriptSerializer();
        System.Diagnostics.Debug.WriteLine("Broadcasting:");
        System.Diagnostics.Debug.WriteLine("Broadcasting {0}", js.Serialize(data));
#endif

        var context = GlobalHost.ConnectionManager.GetHubContext<ChangeListHub>();
        context.Clients.Group(changeList).received(data);
    }

    public void BroadcastData(object data, string changeList)
    {
#if DEBUG
        var js = new JavaScriptSerializer();
        System.Diagnostics.Debug.WriteLine("Broadcasting:");
        System.Diagnostics.Debug.WriteLine("Broadcasting {0}", js.Serialize(data));
#endif

        var clients = Clients.Group(changeList);
        clients.received(data);
    }
}