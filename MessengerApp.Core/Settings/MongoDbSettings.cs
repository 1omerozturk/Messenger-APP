namespace MessengerApp.Core.Settings;

public class MongoDbSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string UsersCollectionName { get; set; } = "Users";
    public string MessagesCollectionName { get; set; } = "Messages";
} 