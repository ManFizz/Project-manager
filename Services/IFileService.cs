namespace MegaProject.Services;

public interface IFileService
{
    Task<List<string>> SaveFilesAsync(List<IFormFile> files);
}