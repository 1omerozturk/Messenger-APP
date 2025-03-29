using MessengerApp.Core.Entities.Base;
using MongoDB.Bson.Serialization.Attributes;

namespace MessengerApp.Core.Entities;

public class User : BaseEntity
{
    public string Username { get; set; }
    public string Email { get; set; }
    
    [BsonElement("PasswordHash")]
    public string Password { get; set; }
    
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ProfilePicture { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
    
    public List<string> Contacts { get; set; } = new();
    public List<string> BlockedUsers { get; set; } = new();
} 