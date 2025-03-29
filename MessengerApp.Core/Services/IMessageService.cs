using MessengerApp.Core.DTOs.Message;

namespace MessengerApp.Core.Services;

public interface IMessageService
{
    Task<MessageDto> GetByIdAsync(string id);
    Task<IEnumerable<MessageDto>> GetConversationAsync(string senderId, string receiverId, int skip = 0, int take = 50);
    Task<MessageDto> CreateAsync(string senderId, CreateMessageDto createMessageDto);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<MessageDto>> GetUnreadMessagesAsync(string userId);
    Task<bool> MarkAsReadAsync(string messageId);
    Task<bool> MarkAllAsReadAsync(string senderId, string receiverId);
    Task<int> GetUnreadMessageCountAsync(string userId);
    Task<IEnumerable<ConversationDto>> GetConversationsAsync(string userId);
    Task<bool> DeleteConversationAsync(string userId, string otherUserId);
} 