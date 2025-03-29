using Microsoft.AspNetCore.SignalR;
using MessengerApp.Core.DTOs.Message;
using MessengerApp.Core.Services;

namespace MessengerApp.API.Hubs;

public class ChatHub : Hub
{
    private readonly IMessageService _messageService;
    private readonly IUserService _userService;
    private static readonly Dictionary<string, string> _userConnections = new();

    public ChatHub(IMessageService messageService, IUserService userService)
    {
        _messageService = messageService;
        _userService = userService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("UserId")?.Value;
        if (userId != null)
        {
            _userConnections[userId] = Context.ConnectionId;
            await _userService.UpdateOnlineStatusAsync(userId, true);
            await Clients.All.SendAsync("UserStatusChanged", userId, true);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("UserId")?.Value;
        if (userId != null)
        {
            _userConnections.Remove(userId);
            await _userService.UpdateOnlineStatusAsync(userId, false);
            await Clients.All.SendAsync("UserStatusChanged", userId, false);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto message)
    {
        var senderId = Context.User?.FindFirst("UserId")?.Value;
        if (senderId == null)
            throw new InvalidOperationException("User not authenticated");

        var createdMessage = await _messageService.CreateAsync(senderId, message);
        
        if (_userConnections.TryGetValue(message.ReceiverId, out var receiverConnectionId))
        {
            await Clients.Client(receiverConnectionId).SendAsync("ReceiveMessage", createdMessage);
        }

        await Clients.Caller.SendAsync("MessageSent", createdMessage);
    }

    public async Task MarkMessageAsRead(string messageId)
    {
        var userId = Context.User?.FindFirst("UserId")?.Value;
        if (userId == null)
            throw new InvalidOperationException("User not authenticated");

        var message = await _messageService.GetByIdAsync(messageId);
        if (message == null)
            throw new InvalidOperationException("Message not found");

        if (message.ReceiverId != userId)
            throw new InvalidOperationException("Not authorized to mark this message as read");

        await _messageService.MarkAsReadAsync(messageId);

        if (_userConnections.TryGetValue(message.SenderId, out var senderConnectionId))
        {
            await Clients.Client(senderConnectionId).SendAsync("MessageRead", messageId);
        }
    }

    public async Task Typing(string receiverId)
    {
        var senderId = Context.User?.FindFirst("UserId")?.Value;
        if (senderId == null)
            throw new InvalidOperationException("User not authenticated");

        if (_userConnections.TryGetValue(receiverId, out var receiverConnectionId))
        {
            await Clients.Client(receiverConnectionId).SendAsync("UserTyping", senderId);
        }
    }

    public async Task StopTyping(string receiverId)
    {
        var senderId = Context.User?.FindFirst("UserId")?.Value;
        if (senderId == null)
            throw new InvalidOperationException("User not authenticated");

        if (_userConnections.TryGetValue(receiverId, out var receiverConnectionId))
        {
            await Clients.Client(receiverConnectionId).SendAsync("UserStoppedTyping", senderId);
        }
    }
} 