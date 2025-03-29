using MessengerApp.Core.Entities.Base;
using MongoDB.Bson.Serialization.Attributes;

namespace MessengerApp.Core.Entities;

public class Message : BaseEntity
{
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public string Content { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public MessageType Type { get; set; }
    public string? AttachmentUrl { get; set; }
}

public enum MessageType
{
    Text,
    Image,
    File,
    Voice,
    Video
} 