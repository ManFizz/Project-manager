namespace MegaProject.Services;

using System.IO;

public class FileService : IFileService
{
    private static readonly string[] AllowedExtensions =
        { ".png", ".jpg", ".jpeg", ".pdf", ".webp", ".mov", ".webm", ".mp4" };

    private const long MaxFileSize = 15 * 1024 * 1024;

    public async Task<List<string>> SaveFilesAsync(List<IFormFile> files)
    {
        var result = new List<string>();

        if (files.Count == 0)
            return result;

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        foreach (var file in files)
        {
            if (file.Length <= 0 || file.Length > MaxFileSize)
                continue;

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!AllowedExtensions.Contains(extension))
                continue;

            var uniqueName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadsFolder, uniqueName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            result.Add($"/uploads/{uniqueName}");
        }

        return result;
    }
}