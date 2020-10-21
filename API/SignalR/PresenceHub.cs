using System;
using System.Threading.Tasks;
using API.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
  [Authorize]
  public class PresenceHub : Hub
  {
    private readonly PresenceTracker _tracker;
    public PresenceHub(PresenceTracker tracker)
    {
      this._tracker = tracker;
    }

    public override async Task OnConnectedAsync()
    {
      var currentUser = Context.User.GetUsername();
      var connId = Context.ConnectionId;

      var isOnline = await _tracker.UserConnected(currentUser, connId);

      if (isOnline)
      {
        await Clients.Others.SendAsync("UserIsOnline", currentUser);
      }

      // return the list of currently online users
      var currentUsers = await _tracker.GetOnlineUsers();
      await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
    }

    public override async Task OnDisconnectedAsync(Exception ex)
    {
      var currentUser = Context.User.GetUsername();
      var connId = Context.ConnectionId;

      var isOffline = await _tracker.UserDisconnected(currentUser, connId);

      if (isOffline)
      {
        await Clients.Others.SendAsync("UserIsOffline", currentUser);
      }

      // if an exception pass it up to the base class
      await base.OnDisconnectedAsync(ex);
    }
  }
}