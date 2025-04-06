using MessengerApp.Core.Entities.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MessengerApp.Core.Entities;

public class User : BaseEntity
{
    [BsonElement("username")]
    public required string Username { get; set; }

    [BsonElement("email")]
    public required string Email { get; set; }

    [BsonElement("passwordHash")]
    public required string PasswordHash { get; set; }

    [BsonElement("firstName")]
    public required string FirstName { get; set; }

    [BsonElement("lastName")]
    public required string LastName { get; set; }

    [BsonElement("profilePicture")]
    public string? ProfilePicture { get; set; }

    [BsonElement("isOnline")]
    public bool IsOnline { get; set; }

    [BsonElement("lastSeen")]
    public DateTime? LastSeen { get; set; }

    public List<string> Contacts { get; set; } = new();
    public List<string> BlockedUsers { get; set; } = new();
}