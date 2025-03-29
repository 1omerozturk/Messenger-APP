using MessengerApp.Core.Entities;
using MessengerApp.Core.Repositories;
using MessengerApp.Core.Settings;
using MessengerApp.Data.Context;
using MongoDB.Driver;

namespace MessengerApp.Data.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(MongoDbContext context) : base(context, context.UsersCollectionName)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await Collection.Find(u => u.Username == username).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await Collection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<bool> AddContactAsync(string userId, string contactId)
    {
        var update = Builders<User>.Update.AddToSet(u => u.Contacts, contactId);
        var result = await Collection.UpdateOneAsync(u => u.Id == userId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> RemoveContactAsync(string userId, string contactId)
    {
        var update = Builders<User>.Update.Pull(u => u.Contacts, contactId);
        var result = await Collection.UpdateOneAsync(u => u.Id == userId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> BlockUserAsync(string userId, string blockedUserId)
    {
        var update = Builders<User>.Update.AddToSet(u => u.BlockedUsers, blockedUserId);
        var result = await Collection.UpdateOneAsync(u => u.Id == userId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UnblockUserAsync(string userId, string blockedUserId)
    {
        var update = Builders<User>.Update.Pull(u => u.BlockedUsers, blockedUserId);
        var result = await Collection.UpdateOneAsync(u => u.Id == userId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<IEnumerable<User>> GetUserContactsAsync(string userId)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return Enumerable.Empty<User>();
        
        return await Collection.Find(u => user.Contacts.Contains(u.Id)).ToListAsync();
    }

    public async Task<IEnumerable<User>> GetOnlineUsersAsync()
    {
        return await Collection.Find(u => u.IsOnline).ToListAsync();
    }

    public async Task UpdateOnlineStatusAsync(string userId, bool isOnline)
    {
        var update = Builders<User>.Update
            .Set(u => u.IsOnline, isOnline)
            .Set(u => u.LastSeen, DateTime.UtcNow);

        await Collection.UpdateOneAsync(u => u.Id == userId, update);
    }

    public async Task<bool> UpdateLastSeenAsync(string userId)
    {
        var update = Builders<User>.Update.Set(u => u.LastSeen, DateTime.UtcNow);
        var result = await Collection.UpdateOneAsync(u => u.Id == userId, update);
        return result.ModifiedCount > 0;
    }
} 