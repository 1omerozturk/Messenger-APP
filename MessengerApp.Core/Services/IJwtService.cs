using MessengerApp.Core.DTOs.User;

namespace MessengerApp.Core.Services;

public interface IJwtService
{
    string GenerateToken(UserDto user);
}