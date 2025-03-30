using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using MessengerApp.Core.Services;
using System.IO;

namespace MessengerApp.Business.Services;

public class FileService : IFileService
{
    private readonly IHostEnvironment _environment;
    private const string UPLOADS_FOLDER = "uploads";
    private const string PROFILE_PICTURES_FOLDER = "profile_pictures";

    public FileService(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> UploadProfilePictureAsync(IFormFile file, string userId)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Invalid file", nameof(file));

        // Create directories if they don't exist
        var uploadsPath = Path.Combine(_environment.ContentRootPath, UPLOADS_FOLDER);
        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        var profilePicturesPath = Path.Combine(uploadsPath, PROFILE_PICTURES_FOLDER);
        if (!Directory.Exists(profilePicturesPath))
            Directory.CreateDirectory(profilePicturesPath);

        // Generate unique filename
        var fileExtension = Path.GetExtension(file.FileName);
        var fileName = $"{userId}_{DateTime.UtcNow.Ticks}{fileExtension}";
        var filePath = Path.Combine(profilePicturesPath, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative URL
        return $"/{UPLOADS_FOLDER}/{PROFILE_PICTURES_FOLDER}/{fileName}";
    }

    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl))
            return Task.FromResult(false);

        try
        {
            // Get absolute path from relative URL
            var relativePath = fileUrl.TrimStart('/');
            var fullPath = Path.Combine(_environment.ContentRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
} 