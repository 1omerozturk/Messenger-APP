using MessengerApp.Core.Entities;

namespace MessengerApp.Core.Repositories;

public interface IMessageRepository : IBaseRepository<Message>
{
    Task<IEnumerable<Message>> GetConversationAsync(string senderId, string receiverId, int skip = 0, int take = 50);
    Task<IEnumerable<Message>> GetUnreadMessagesAsync(string userId);
    Task<bool> MarkAsReadAsync(string messageId);
    Task<bool> MarkAllAsReadAsync(string senderId, string receiverId);
    Task<int> GetUnreadMessageCountAsync(string userId);
    Task<IEnumerable<Message>> GetLastMessagesAsync(string userId, int count = 20);
} 