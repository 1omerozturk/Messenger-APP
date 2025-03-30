using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace MessengerApp.API.Extensions;

public static class FormFileExtensions
{
    private static readonly Regex ImageRegex = new Regex(@"^image\/(webp|jpeg|png|gif|jpg)$", RegexOptions.Compiled);
    private const int MaxFileSize = 5 * 1024 * 1024; // 5MB

    public static bool IsValidProfilePicture(this IFormFile file)
    {
        if (file == null || file.Length <= 0 || file.Length > MaxFileSize)
            return false;

        return file.ContentType != null && ImageRegex.IsMatch(file.ContentType);
    }
} 