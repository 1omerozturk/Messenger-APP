using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MessengerApp.Core.DTOs.User;
using MessengerApp.Core.Services;
using MessengerApp.API.Extensions;

namespace MessengerApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly IFileService _fileService;

    public UserController(IUserService userService, IJwtService jwtService, IFileService fileService)
    {
        _userService = userService;
        _jwtService = jwtService;
        _fileService = fileService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var currentUserId = User.FindFirst("UserId")?.Value;
        if (currentUserId == null)
            return Unauthorized();
            
        var users = await _userService.GetAllUsersExceptCurrentAsync(currentUserId);
        return Ok(users);
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

    [Authorize]
    [HttpPost("refresh-token")]
    public async Task<ActionResult<object>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    {
        try
        {
            var user = await _userService.ValidateTokenAsync(refreshTokenDto.Token);
            if (user == null)
                return Unauthorized("Invalid token");

            var newToken = _jwtService.GenerateToken(user);
            return Ok(new { token = newToken });
        }
        catch (InvalidOperationException)
        {
            return Unauthorized("Invalid token");
        }
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var userId = User.FindFirst("UserId")?.Value;
        if (userId == null)
            return Unauthorized();

        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost("upload-profile-picture")]
    public async Task<ActionResult<string>> UploadProfilePicture(IFormFile file)
    {
        if (file == null || !file.IsValidProfilePicture())
            return BadRequest("Invalid file: Please upload a valid image file (JPEG, PNG, GIF) up to 5MB.");

        // For temporary uploads before the user is registered
        var tempUserId = Guid.NewGuid().ToString();
        try
        {
            var fileUrl = await _fileService.UploadProfilePictureAsync(file, tempUserId);
            return Ok(new { profilePictureUrl = fileUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error uploading file: {ex.Message}");
        }
    }

    [Authorize]
    [HttpPost("profile-picture")]
    public async Task<ActionResult<UserDto>> UpdateProfilePicture(IFormFile file)
    {
        if (file == null || !file.IsValidProfilePicture())
            return BadRequest("Invalid file: Please upload a valid image file (JPEG, PNG, GIF) up to 5MB.");
            
        var userId = User.FindFirst("UserId")?.Value;
        if (userId == null)
            return Unauthorized();

        try
        {
            // Get current user
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return NotFound();

            // Delete old profile picture if exists
            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                await _fileService.DeleteFileAsync(user.ProfilePicture);
            }

            // Upload new profile picture
            var fileUrl = await _fileService.UploadProfilePictureAsync(file, userId);

            // Update user profile
            var updateDto = new UpdateUserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePicture = fileUrl
            };

            var updatedUser = await _userService.UpdateAsync(userId, updateDto);
            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating profile picture: {ex.Message}");
        }
    }
}