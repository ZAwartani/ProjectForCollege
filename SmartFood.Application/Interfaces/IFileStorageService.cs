using Microsoft.AspNetCore.Http;

namespace SmartFood.Application.Interfaces
{
    public interface IFileStorageService
    {
        void CreateFolder(string relativeFolder);
        Task<string> SaveFileAsync(IFormFile File, string relativeFolder);
        Task<IReadOnlyList<string>> ListFilesAsync();
        Task<bool> DeleteFileAsync(string FileName);
    }
}
