using MessengerApp.Core.Entities;
using MessengerApp.Core.Repositories;
using MessengerApp.Core.Settings;
using MessengerApp.Data.Context;
using MongoDB.Driver;

namespace MessengerApp.Data.Repositories;

public class MessageRepository : BaseRepository<Message>, IMessageRepository
{
    public MessageRepository(MongoDbContext context) : base(context, context._settings.MessagesCollectionName)
    {
    }

    public async Task<IEnumerable<Message>> GetConversationAsync(string senderId, string receiverId, int skip = 0, int take = 50)
    {
        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.Or(
                Builders<Message>.Filter.And(
                    Builders<Message>.Filter.Eq(x => x.SenderId, senderId),
                    Builders<Message>.Filter.Eq(x => x.ReceiverId, receiverId)
                ),
                Builders<Message>.Filter.And(
                    Builders<Message>.Filter.Eq(x => x.SenderId, receiverId),
                    Builders<Message>.Filter.Eq(x => x.ReceiverId, senderId)
                )
            ),
            Builders<Message>.Filter.Eq(x => x.IsDeleted, false)
        );

        return await _collection.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Limit(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<Message>> GetUnreadMessagesAsync(string userId)
    {
        return await _collection.Find(x => x.ReceiverId == userId && !x.IsRead && !x.IsDeleted)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> MarkAsReadAsync(string messageId)
    {
        var update = Builders<Message>.Update
            .Set(x => x.IsRead, true)
            .Set(x => x.ReadAt, DateTime.UtcNow)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(x => x.Id == messageId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> MarkAllAsReadAsync(string senderId, string receiverId)
    {
        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.Eq(x => x.SenderId, senderId),
            Builders<Message>.Filter.Eq(x => x.ReceiverId, receiverId),
            Builders<Message>.Filter.Eq(x => x.IsRead, false),
            Builders<Message>.Filter.Eq(x => x.IsDeleted, false)
        );

        var update = Builders<Message>.Update
            .Set(x => x.IsRead, true)
            .Set(x => x.ReadAt, DateTime.UtcNow)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        var result = await _collection.UpdateManyAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public async Task<int> GetUnreadMessageCountAsync(string userId)
    {
        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.Eq(x => x.ReceiverId, userId),
            Builders<Message>.Filter.Eq(x => x.IsRead, false),
            Builders<Message>.Filter.Eq(x => x.IsDeleted, false)
        );

        return (int)await _collection.CountDocumentsAsync(filter);
    }

    public async Task<IEnumerable<Message>> GetLastMessagesAsync(string userId, int count = 20)
    {
        var filter = Builders<Message>.Filter.Or(
            Builders<Message>.Filter.Eq(x => x.SenderId, userId),
            Builders<Message>.Filter.Eq(x => x.ReceiverId, userId)
        );

        return await _collection.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Limit(count)
            .ToListAsync();
    }
} 