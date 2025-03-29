using MessengerApp.Core.Entities;
using MessengerApp.Core.Settings;
using MongoDB.Driver;

namespace MessengerApp.Data.Context;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(MongoDbSettings settings)
    {
        _settings = settings;
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<User> Users => 
        _database.GetCollection<User>(_settings.UsersCollectionName);

    public IMongoCollection<Message> Messages => 
        _database.GetCollection<Message>(_settings.MessagesCollectionName);
} 