using System.Linq.Expressions;
using MessengerApp.Core.Entities.Base;
using MessengerApp.Core.Repositories;
using MessengerApp.Data.Context;
using MongoDB.Driver;

namespace MessengerApp.Data.Repositories;

public abstract class BaseRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> Collection;

    protected BaseRepository(MongoDbContext context, string collectionName)
    {
        Collection = context.GetCollection<T>(collectionName);
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        return await Collection.Find(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await Collection.Find(x => !x.IsDeleted).ToListAsync();
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        await Collection.InsertOneAsync(entity);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await Collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
    }

    public virtual async Task DeleteAsync(string id)
    {
        var update = Builders<T>.Update
            .Set(x => x.IsDeleted, true)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);
        await Collection.UpdateOneAsync(x => x.Id == id, update);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await Collection.Find(predicate)
            .ToListAsync();
    }
} 