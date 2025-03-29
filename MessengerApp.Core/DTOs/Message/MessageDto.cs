using MessengerApp.Core.Entities;

namespace MessengerApp.Core.DTOs.Message;

public class MessageDto
{
    public string Id { get; set; }
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public string Content { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public MessageType Type { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateMessageDto
{
    public string ReceiverId { get; set; }
    public string Content { get; set; }
    public MessageType Type { get; set; }
    public string? AttachmentUrl { get; set; }
}

public class ConversationDto
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string? ProfilePicture { get; set; }
    public string LastMessage { get; set; }
    public DateTime LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
} 