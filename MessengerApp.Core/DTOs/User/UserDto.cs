namespace MessengerApp.Core.DTOs.User;

public class UserDto
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? ProfilePicture { get; set; }
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
}

public class CreateUserDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? ProfilePicture { get; set; }
}

public class UpdateUserDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? ProfilePicture { get; set; }
}

public class UserLoginDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class UserContactDto
{
    public required string UserId { get; set; }
    public required string ContactId { get; set; }
} 