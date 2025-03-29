using MessengerApp.Core.Entities;

namespace MessengerApp.Core.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User> GetByUsernameAsync(string username);
    Task<User> GetByEmailAsync(string email);
    Task<bool> AddContactAsync(string userId, string contactId);
    Task<bool> RemoveContactAsync(string userId, string contactId);
    Task<bool> BlockUserAsync(string userId, string blockedUserId);
    Task<bool> UnblockUserAsync(string userId, string blockedUserId);
    Task<IEnumerable<User>> GetUserContactsAsync(string userId);
    Task<bool> UpdateOnlineStatusAsync(string userId, bool isOnline);
    Task<bool> UpdateLastSeenAsync(string userId);
} 