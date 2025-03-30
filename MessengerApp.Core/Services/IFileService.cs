using Microsoft.AspNetCore.Http;

namespace MessengerApp.Core.Services;

public interface IFileService
{
    Task<string> UploadProfilePictureAsync(IFormFile file, string userId);
    Task<bool> DeleteFileAsync(string fileUrl);
} 