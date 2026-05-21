namespace MegaProject.Web.Services;

public interface IFileService
{
    Task<List<string>> SaveFilesAsync(List<IFormFile> files);
    void DeleteFiles(IEnumerable<string> paths);
    void DeleteFile(string relativePath);
}