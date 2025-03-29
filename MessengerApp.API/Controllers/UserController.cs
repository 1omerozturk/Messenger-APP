using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MessengerApp.Core.DTOs.User;
using MessengerApp.Core.Services;

namespace MessengerApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;

    public UserController(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(CreateUserDto createUserDto)
    {
        try
        {
            var user = await _userService.CreateAsync(createUserDto);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<object>> Login(UserLoginDto loginDto)
    {
        try
        {
            var user = await _userService.LoginAsync(loginDto);
            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                user,
                token
            });
        }
        catch (InvalidOperationException)
        {
            return Unauthorized("Invalid username or password");
        }
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(string id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [Authorize]
    [HttpGet("username/{username}")]
    public async Task<ActionResult<UserDto>> GetByUsername(string username)
    {
        var user = await _userService.GetByUsernameAsync(username);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Update(string id, UpdateUserDto updateUserDto)
    {
        try
        {
            var user = await _userService.UpdateAsync(id, updateUserDto);
            return Ok(user);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _userService.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [Authorize]
    [HttpPost("contacts")]
    public async Task<IActionResult> AddContact(UserContactDto contactDto)
    {
        try
        {
            var result = await _userService.AddContactAsync(contactDto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpDelete("contacts")]
    public async Task<IActionResult> RemoveContact(UserContactDto contactDto)
    {
        var result = await _userService.RemoveContactAsync(contactDto);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [Authorize]
    [HttpGet("contacts/{userId}")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetContacts(string userId)
    {
        var contacts = await _userService.GetUserContactsAsync(userId);
        return Ok(contacts);
    }

    [Authorize]
    [HttpPost("block")]
    public async Task<IActionResult> BlockUser(UserContactDto contactDto)
    {
        var result = await _userService.BlockUserAsync(contactDto);
        if (!result)
            return NotFound();

        return Ok();
    }

    [Authorize]
    [HttpPost("unblock")]
    public async Task<IActionResult> UnblockUser(UserContactDto contactDto)
    {
        var result = await _userService.UnblockUserAsync(contactDto);
        if (!result)
            return NotFound();

        return Ok();
    }
}