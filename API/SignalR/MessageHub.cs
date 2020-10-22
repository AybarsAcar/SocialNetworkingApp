using System;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
  public class MessageHub : Hub
  {
    private readonly IMapper _mapper;
    private readonly IHubContext<PresenceHub> _presenceHub;
    private readonly PresenceTracker _tracker;
    private readonly IUnitOfWork _unitOfWork;
    public MessageHub(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<PresenceHub> presenceHub, PresenceTracker tracker)
    {
      this._unitOfWork = unitOfWork;
      this._tracker = tracker;
      this._presenceHub = presenceHub;
      this._mapper = mapper;
    }

    public override async Task OnConnectedAsync()
    {
      // logged in user
      var user = Context.User.GetUsername();

      // get the other users username with a key of user
      var httpContext = Context.GetHttpContext();
      var otherUser = httpContext.Request.Query["user"].ToString();

      // create the group name
      var groupName = getGroupName(user, otherUser);

      await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

      var group = await AddToGroup(groupName);
      await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

      var messages = await _unitOfWork.MessageRepository.GetMessageThread(user, otherUser);

      if (_unitOfWork.HasChanges()) await _unitOfWork.Complete();

      await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }

    /* 
    SignalR already removes the logged our user from the group automatically
     */
    public override async Task OnDisconnectedAsync(Exception ex)
    {
      var group = await RemoveFromMessageGroup();
      await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);

      await base.OnDisconnectedAsync(ex);
    }

    /* 
    Send the message via the hub
     */
    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
      // get username
      var username = Context.User.GetUsername();

      if (username == createMessageDto.RecipientUsername.ToLower())
      {
        throw new HubException("You cannot send messages to yourself");
      }

      // get hold of both of our users
      var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
      var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

      if (recipient == null) throw new HubException("User not found");

      // create the new message
      var message = new Message
      {
        Sender = sender,
        Recipient = recipient,
        SenderUsername = sender.UserName,
        RecipientUsername = recipient.UserName,
        Content = createMessageDto.Content
      };

      // get the group name
      var groupName = getGroupName(sender.UserName, recipient.UserName);

      var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);

      if (group.Connections.Any(x => x.Username == recipient.UserName))
      {
        message.DateRead = DateTime.UtcNow;
      }
      else
      {
        var connections = await _tracker.GetConnectionsForUser(recipient.UserName);
        if (connections != null)
        {
          // so the user is online but not inside the particular message thread
          await _presenceHub.Clients
            .Clients(connections)
            .SendAsync("NewMessageReceived", new { username = sender.UserName, knownAs = sender.KnownAs });
        }
      }

      _unitOfWork.MessageRepository.AddMessage(message);

      if (await _unitOfWork.Complete())
      {
        // send the message as new message
        await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
      }
    }

    private async Task<Group> AddToGroup(string groupName)
    {
      var user = Context.User.GetUsername();
      var connId = Context.ConnectionId;

      var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
      var connection = new Connection(connId, user);

      if (group == null)
      {
        group = new Group(groupName);
        _unitOfWork.MessageRepository.AddGroup(group);
      }

      group.Connections.Add(connection);

      if (await _unitOfWork.Complete())
      {
        return group;
      }
      throw new HubException("Failed to join the group");
    }

    private async Task<Group> RemoveFromMessageGroup()
    {
      var group = await _unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
      var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);

      _unitOfWork.MessageRepository.RemoveConnection(connection);
      if (await _unitOfWork.Complete())
      {
        return group;
      }

      throw new HubException("Failed to remove from group");
    }

    private string getGroupName(string user1, string user2)
    {
      var stringCompare = string.CompareOrdinal(user1, user2) < 0;

      return stringCompare ? $"{user1}-{user2}" : $"{user2}-{user1}";
    }

  }
}