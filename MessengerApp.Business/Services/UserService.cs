using MessengerApp.Core.DTOs.User;
using MessengerApp.Core.Entities;
using MessengerApp.Core.Repositories;
using MessengerApp.Core.Services;
using System.Security.Cryptography;
using System.Text;

namespace MessengerApp.Business.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> GetByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return MapToDto(user);
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
            Username = createUserDto.Username,
            Email = createUserDto.Email,
            Password = HashPassword(createUserDto.Password),
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName
        };

        await _userRepository.AddAsync(user);
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

        await _userRepository.UpdateAsync(user);
        return MapToDto(user);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _userRepository.SoftDeleteAsync(id);
    }

    public async Task<UserDto> LoginAsync(UserLoginDto loginDto)
    {
        var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
        if (user == null)
            throw new InvalidOperationException("Invalid username or password");

        if (user.Password != HashPassword(loginDto.Password))
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
        return contacts.Select(MapToDto);
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
        return await _userRepository.UpdateOnlineStatusAsync(userId, isOnline);
    }

    public async Task<bool> UpdateLastSeenAsync(string userId)
    {
        return await _userRepository.UpdateLastSeenAsync(userId);
    }

    private static UserDto MapToDto(User user)
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