namespace MessengerApp.Core.Settings;

public class MongoDbSettings
{
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
    public required string UsersCollectionName { get; set; } = "Users";
    public required string MessagesCollectionName { get; set; } = "Messages";
}