using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MessengerApp.Core.DTOs.Message;
using MessengerApp.Core.Services;

namespace MessengerApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MessageDto>> GetById(string id)
    {
        var message = await _messageService.GetByIdAsync(id);
        if (message == null)
            return NotFound();

        return Ok(message);
    }

    [HttpGet("conversation/{receiverId}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetConversation(
        string receiverId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        var senderId = User.FindFirst("UserId")?.Value;
        if (senderId == null)
            return Unauthorized();

        var messages = await _messageService.GetConversationAsync(senderId, receiverId, skip, take);
        return Ok(messages);
    }

    [HttpGet("unread")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetUnreadMessages()
    {
        var userId = User.FindFirst("UserId")?.Value;
        if (userId == null)
            return Unauthorized();

        var messages = await _messageService.GetUnreadMessagesAsync(userId);
        return Ok(messages);
    }

    [HttpGet("unread/count")]
    public async Task<ActionResult<int>> GetUnreadMessageCount()
    {
        var userId = User.FindFirst("UserId")?.Value;
        if (userId == null)
            return Unauthorized();

        var count = await _messageService.GetUnreadMessageCountAsync(userId);
        return Ok(count);
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<IEnumerable<ConversationDto>>> GetConversations()
    {
        var userId = User.FindFirst("UserId")?.Value;
        if (userId == null)
            return Unauthorized();

        var conversations = await _messageService.GetConversationsAsync(userId);
        return Ok(conversations);
    }

    [HttpPost("read/{messageId}")]
    public async Task<IActionResult> MarkAsRead(string messageId)
    {
        var result = await _messageService.MarkAsReadAsync(messageId);
        if (!result)
            return NotFound();

        return Ok();
    }

    [HttpPost("read/all/{receiverId}")]
    public async Task<IActionResult> MarkAllAsRead(string receiverId)
    {
        var senderId = User.FindFirst("UserId")?.Value;
        if (senderId == null)
            return Unauthorized();

        var result = await _messageService.MarkAllAsReadAsync(senderId, receiverId);
        if (!result)
            return NotFound();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _messageService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
} 