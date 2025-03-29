using MessengerApp.Core.Entities.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MessengerApp.Core.Entities;

public class Message : BaseEntity
{
    [BsonElement("senderId")]
    public required string SenderId { get; set; }

    [BsonElement("receiverId")]
    public required string ReceiverId { get; set; }

    [BsonElement("content")]
    public required string Content { get; set; }

    [BsonElement("type")]
    public required string Type { get; set; }

    [BsonElement("attachmentUrl")]
    public string? AttachmentUrl { get; set; }

    [BsonElement("isRead")]
    public bool IsRead { get; set; }

    [BsonElement("readAt")]
    public DateTime? ReadAt { get; set; }
}

public enum MessageType
{
    Text,
    Image,
    File,
    Voice,
    Video
} 