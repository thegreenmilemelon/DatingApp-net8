using System;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class MessageHub(IMessageRepository messageRepository, IUserRepository userRepository, IMapper mapper, IHubContext<PresenceHub> presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext?.Request.Query["user"];

        if (Context.User == null || string.IsNullOrEmpty(otherUser)) throw new Exception("Cannot join group");
        var groupName = GetGroupName(Context.User.GetUsername(), otherUser);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        //so our clien can check who is in  group at any specific time
        //so when we do mark a message as read we can check to make sure that the other user is in that group before doing so
        //then we're going to return that information to the client
        var group = await AddToGroup(groupName);
        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await messageRepository.GetMessageThread(Context.User.GetUsername(), otherUser!);

        // await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var group = await RemoveFromMessageGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
        //hence we can update the list of online users like who is in the group and not
        //also the client will be able to maintain the lst of online users
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var username = Context.User?.GetUsername() ?? throw new Exception("could not get user");
        if (username == createMessageDto.RecipientUsername.ToLower())
            throw new HubException("You cannot message yourself");

        var sender = await userRepository.GetUserByUsernameAsync(username);
        var recipient = await userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
        if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null)
            throw new HubException("Cannot send message at this time");
        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await messageRepository.GetMessageGroup(groupName);

        if (group != null && group.Connections.Any(x => x.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow; //message as read vayo
        }
        else
        {
            var connection = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
            if (connection != null && connection?.Count != null)
            {
                await presenceHub.Clients.Clients(connection).SendAsync("NewMessageReceived", new { username = sender.UserName, knownAs = sender.KnownAs });
            }
        }

        messageRepository.AddMessage(message);
        if (await messageRepository.SaveAllAsync())
        {
            //var groupName = GetGroupName(sender.UserName, recipient.UserName);
            await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
        }
    }

    private async Task<Group> AddToGroup(string groupName)
    {
        var username = Context.User?.GetUsername() ?? throw new Exception("could not get username");
        var group = await messageRepository.GetMessageGroup(groupName);
        var connection = new Connection { ConnectionId = Context.ConnectionId, Username = username };
        if (group == null)
        {
            group = new Group { Name = groupName };
            messageRepository.AddGroup(group);
        }
        group.Connections.Add(connection);
        if (await messageRepository.SaveAllAsync())
        {
            return group;
        }
        throw new HubException("Failed to join group");
    }

    private async Task<Group> RemoveFromMessageGroup()
    {
        var group = await messageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        //var connection = await messageRepository.GetConnection(Context.ConnectionId);
        if (connection != null)
        {
            messageRepository.RemoveConnection(connection);
            if (await messageRepository.SaveAllAsync()) return group!;
        }
        throw new HubException("Failed to remove from group");
    }

    private string GetGroupName(string caller, string? other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
}