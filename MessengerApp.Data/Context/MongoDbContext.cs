using Microsoft.Extensions.Options;
using MessengerApp.Core.Entities;
using MessengerApp.Core.Settings;
using MongoDB.Driver;

namespace MessengerApp.Data.Context;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    public readonly MongoDbSettings _settings;

    public string UsersCollectionName => _settings.UsersCollectionName;
    public string MessagesCollectionName => _settings.MessagesCollectionName;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
    }

    public IMongoCollection<User> Users => 
        _database.GetCollection<User>(_settings.UsersCollectionName);

    public IMongoCollection<Message> Messages => 
        _database.GetCollection<Message>(_settings.MessagesCollectionName);

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }
} 