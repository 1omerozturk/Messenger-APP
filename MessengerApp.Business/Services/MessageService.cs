using MessengerApp.Core.DTOs.Message;
using MessengerApp.Core.Entities;
using MessengerApp.Core.Repositories;
using MessengerApp.Core.Services;

namespace MessengerApp.Business.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;

    public MessageService(IMessageRepository messageRepository, IUserRepository userRepository)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }

    public async Task<MessageDto> GetByIdAsync(string id)
    {
        var message = await _messageRepository.GetByIdAsync(id);
        return MapToDto(message);
    }

    public async Task<IEnumerable<MessageDto>> GetConversationAsync(string senderId, string receiverId, int skip = 0, int take = 50)
    {
        var messages = await _messageRepository.GetConversationAsync(senderId, receiverId, skip, take);
        return messages.Select(MapToDto);
    }

    public async Task<MessageDto> CreateAsync(string senderId, CreateMessageDto createMessageDto)
    {
        var sender = await _userRepository.GetByIdAsync(senderId);
        if (sender == null)
            throw new InvalidOperationException("Sender not found");

        var receiver = await _userRepository.GetByIdAsync(createMessageDto.ReceiverId);
        if (receiver == null)
            throw new InvalidOperationException("Receiver not found");

        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = createMessageDto.ReceiverId,
            Content = createMessageDto.Content,
            Type = createMessageDto.Type,
            AttachmentUrl = createMessageDto.AttachmentUrl
        };

        await _messageRepository.AddAsync(message);
        return MapToDto(message);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _messageRepository.SoftDeleteAsync(id);
    }

    public async Task<IEnumerable<MessageDto>> GetUnreadMessagesAsync(string userId)
    {
        var messages = await _messageRepository.GetUnreadMessagesAsync(userId);
        return messages.Select(MapToDto);
    }

    public async Task<bool> MarkAsReadAsync(string messageId)
    {
        return await _messageRepository.MarkAsReadAsync(messageId);
    }

    public async Task<bool> MarkAllAsReadAsync(string senderId, string receiverId)
    {
        return await _messageRepository.MarkAllAsReadAsync(senderId, receiverId);
    }

    public async Task<int> GetUnreadMessageCountAsync(string userId)
    {
        return await _messageRepository.GetUnreadMessageCountAsync(userId);
    }

    public async Task<IEnumerable<ConversationDto>> GetConversationsAsync(string userId)
    {
        var lastMessages = await _messageRepository.GetLastMessagesAsync(userId);
        var conversations = new List<ConversationDto>();

        foreach (var message in lastMessages)
        {
            var otherUserId = message.SenderId == userId ? message.ReceiverId : message.SenderId;
            var otherUser = await _userRepository.GetByIdAsync(otherUserId);

            if (otherUser != null)
            {
                var unreadCount = await _messageRepository.GetUnreadMessageCountAsync(userId);
                conversations.Add(new ConversationDto
                {
                    UserId = otherUser.Id,
                    Username = otherUser.Username,
                    ProfilePicture = otherUser.ProfilePicture,
                    LastMessage = message.Content,
                    LastMessageTime = message.CreatedAt,
                    UnreadCount = unreadCount,
                    IsOnline = otherUser.IsOnline,
                    LastSeen = otherUser.LastSeen
                });
            }
        }

        return conversations.OrderByDescending(x => x.LastMessageTime);
    }

    private static MessageDto MapToDto(Message message)
    {
        if (message == null)
            return null;

        return new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId,
            Content = message.Content,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt,
            Type = message.Type,
            AttachmentUrl = message.AttachmentUrl,
            CreatedAt = message.CreatedAt
        };
    }
} 