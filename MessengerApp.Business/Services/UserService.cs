using MessengerApp.Core.DTOs.User;
using MessengerApp.Core.Entities;
using MessengerApp.Core.Repositories;
using MessengerApp.Core.Services;
using MongoDB.Bson;
using System.Security.Cryptography;
using System.Text;

namespace MessengerApp.Business.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public UserService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<UserDto?> GetByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto> GetByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto createUserDto)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(createUserDto.Username);
        if (existingUser != null)
            throw new InvalidOperationException("Username already exists");

        existingUser = await _userRepository.GetByEmailAsync(createUserDto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email already exists");

        var user = new User
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Username = createUserDto.Username,
            Email = createUserDto.Email,
            PasswordHash = HashPassword(createUserDto.Password),
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            ProfilePicture = createUserDto.ProfilePicture
        };

        await _userRepository.CreateAsync(user);
        return MapToDto(user);
    }

    public async Task<UserDto> UpdateAsync(string id, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new InvalidOperationException("User not found");

        user.FirstName = updateUserDto.FirstName;
        user.LastName = updateUserDto.LastName;
        user.ProfilePicture = updateUserDto.ProfilePicture;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        return MapToDto(user);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return false;

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<UserDto> LoginAsync(UserLoginDto loginDto)
    {
        var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
        if (user == null)
            throw new InvalidOperationException("Invalid username or password");

        if (user.PasswordHash != HashPassword(loginDto.Password))
            throw new InvalidOperationException("Invalid username or password");

        return MapToDto(user);
    }

    public async Task<bool> AddContactAsync(UserContactDto contactDto)
    {
        var user = await _userRepository.GetByIdAsync(contactDto.UserId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        var contact = await _userRepository.GetByIdAsync(contactDto.ContactId);
        if (contact == null)
            throw new InvalidOperationException("Contact not found");

        return await _userRepository.AddContactAsync(contactDto.UserId, contactDto.ContactId);
    }

    public async Task<bool> RemoveContactAsync(UserContactDto contactDto)
    {
        return await _userRepository.RemoveContactAsync(contactDto.UserId, contactDto.ContactId);
    }

    public async Task<IEnumerable<UserDto>> GetUserContactsAsync(string userId)
    {
        var contacts = await _userRepository.GetUserContactsAsync(userId);
        return contacts.Select(MapToDto).Where(dto => dto != null).Select(dto => dto!);
    }

    public async Task<bool> BlockUserAsync(UserContactDto contactDto)
    {
        return await _userRepository.BlockUserAsync(contactDto.UserId, contactDto.ContactId);
    }

    public async Task<bool> UnblockUserAsync(UserContactDto contactDto)
    {
        return await _userRepository.UnblockUserAsync(contactDto.UserId, contactDto.ContactId);
    }

    public async Task<bool> UpdateOnlineStatusAsync(string userId, bool isOnline)
    {
        await _userRepository.UpdateOnlineStatusAsync(userId, isOnline);
        return true;
    }

    public async Task<bool> UpdateLastSeenAsync(string userId)
    {
        return await _userRepository.UpdateLastSeenAsync(userId);
    }

    public async Task<UserDto?> ValidateTokenAsync(string token)
    {
        try
        {
            var userId = _jwtService.GetUserIdFromToken(token);
            if (string.IsNullOrEmpty(userId))
                return null;

            return await GetByIdAsync(userId);
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersExceptCurrentAsync(string currentUserId)
    {
        var users = await _userRepository.GetAllAsync();
        return users
            .Where(u => u.Id != currentUserId && !u.IsDeleted)
            .Select(MapToDto)
            .Where(dto => dto != null)
            .Select(dto => dto!);
    }

    private static UserDto? MapToDto(User? user)
    {
        if (user == null)
            return null;

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfilePicture = user.ProfilePicture,
            IsOnline = user.IsOnline,
            LastSeen = user.LastSeen
        };
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
} 