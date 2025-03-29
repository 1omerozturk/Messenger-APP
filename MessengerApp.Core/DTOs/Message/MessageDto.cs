using MessengerApp.Core.Entities;

namespace MessengerApp.Core.DTOs.Message;

public class MessageDto
{
    public required string Id { get; set; }
    public required string SenderId { get; set; }
    public required string ReceiverId { get; set; }
    public required string Content { get; set; }
    public required string Type { get; set; }
    public string? AttachmentUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateMessageDto
{
    public required string ReceiverId { get; set; }
    public required string Content { get; set; }
    public required string Type { get; set; }
    public string? AttachmentUrl { get; set; }
}

public class ConversationDto
{
    public required string UserId { get; set; }
    public required string Username { get; set; }
    public string? ProfilePicture { get; set; }
    public required string LastMessage { get; set; }
    public DateTime LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
} 