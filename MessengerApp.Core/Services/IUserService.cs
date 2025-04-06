using MessengerApp.Core.DTOs.User;

namespace MessengerApp.Core.Services;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(string id);
    Task<UserDto> GetByUsernameAsync(string username);
    Task<UserDto> CreateAsync(CreateUserDto createUserDto);
    Task<UserDto> UpdateAsync(string id, UpdateUserDto updateUserDto);
    Task<bool> DeleteAsync(string id);
    Task<UserDto> LoginAsync(UserLoginDto loginDto);
    Task<bool> AddContactAsync(UserContactDto contactDto);
    Task<bool> RemoveContactAsync(UserContactDto contactDto);
    Task<IEnumerable<UserDto>> GetUserContactsAsync(string userId);
    Task<bool> BlockUserAsync(UserContactDto contactDto);
    Task<bool> UnblockUserAsync(UserContactDto contactDto);
    Task<bool> UpdateOnlineStatusAsync(string userId, bool isOnline);
    Task<bool> UpdateLastSeenAsync(string userId);
    Task<UserDto?> ValidateTokenAsync(string token);
    Task<IEnumerable<UserDto>> GetAllUsersExceptCurrentAsync(string currentUserId);
}