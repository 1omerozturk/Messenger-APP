using MessengerApp.Core.Entities;
using MessengerApp.Core.Repositories;
using MessengerApp.Core.Settings;
using MessengerApp.Data.Context;
using MongoDB.Driver;

namespace MessengerApp.Data.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(MongoDbContext context) : base(context, context._settings.UsersCollectionName)
    {
    }

    public async Task<User> GetByUsernameAsync(string username)
    {
        return await _collection.Find(x => x.Username == username && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        return await _collection.Find(x => x.Email == email && !x.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> AddContactAsync(string userId, string contactId)
    {
        var update = Builders<User>.Update
            .AddToSet(x => x.Contacts, contactId)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(x => x.Id == userId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> RemoveContactAsync(string userId, string contactId)
    {
        var update = Builders<User>.Update
            .Pull(x => x.Contacts, contactId)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(x => x.Id == userId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> BlockUserAsync(string userId, string blockedUserId)
    {
        var update = Builders<User>.Update
            .AddToSet(x => x.BlockedUsers, blockedUserId)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(x => x.Id == userId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UnblockUserAsync(string userId, string blockedUserId)
    {
        var update = Builders<User>.Update
            .Pull(x => x.BlockedUsers, blockedUserId)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(x => x.Id == userId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<IEnumerable<User>> GetUserContactsAsync(string userId)
    {
        var user = await GetByIdAsync(userId);
        if (user == null || !user.Contacts.Any())
            return Enumerable.Empty<User>();

        return await _collection.Find(x => user.Contacts.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> UpdateOnlineStatusAsync(string userId, bool isOnline)
    {
        var update = Builders<User>.Update
            .Set(x => x.IsOnline, isOnline)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        if (!isOnline)
        {
            update = update.Set(x => x.LastSeen, DateTime.UtcNow);
        }

        var result = await _collection.UpdateOneAsync(x => x.Id == userId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> UpdateLastSeenAsync(string userId)
    {
        var update = Builders<User>.Update
            .Set(x => x.LastSeen, DateTime.UtcNow)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(x => x.Id == userId, update);
        return result.ModifiedCount > 0;
    }
} 