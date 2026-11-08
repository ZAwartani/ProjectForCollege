
using SmartFood.Application.Interfaces;

namespace SmartFood.Presentation.Repositories
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _Environment;
        private readonly IConfiguration _Configuration;

        public LocalFileStorageService(
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _Environment = environment;
            _Configuration = configuration;
        }

        // ================= STORAGE PATH =================

        private string GetStoragePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath) ||
                Path.IsPathRooted(relativePath))
            {
                throw new ArgumentException("A relative path is required.");
            }

            var webRoot = _Environment.WebRootPath
                ?? Path.Combine(_Environment.ContentRootPath, "wwwroot");

            var root = Path.GetFullPath(webRoot)
                .TrimEnd(Path.DirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            var fullPath = Path.GetFullPath(
                Path.Combine(root, relativePath)
            );

            if (!fullPath.StartsWith(
                root,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "The path must stay inside wwwroot."
                );
            }

            return fullPath;
        }

        // ================= CREATE FOLDER =================

        public void CreateFolder(string relativeFolder)
        {
            var folder = GetStoragePath(relativeFolder);

            Directory.CreateDirectory(folder);
        }

        // ================= SAVE FILE =================

        public async Task<string> SaveFileAsync(
            IFormFile file,
            string relativeFolder)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty.");

            const long maxFileSize = 2 * 1024 * 1024;

            if (file.Length > maxFileSize)
                throw new Exception("File size must be 2 MB or less.");

            var folder = GetStoragePath(relativeFolder);

            Directory.CreateDirectory(folder);

            var fileName =
                Guid.NewGuid() + Path.GetExtension(file.FileName);

            var path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);

            await file.CopyToAsync(stream);

            return Path.Combine(relativeFolder, fileName);
        }

        // ================= DELETE FILE =================

        public Task<bool> DeleteFileAsync(string fileName)
        {
            var path = GetStoragePath(fileName);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return Task.FromResult(true);
        }

        // ================= LIST FILES =================

        public Task<IReadOnlyList<string>> ListFilesAsync()
        {
            var webRoot = _Environment.WebRootPath
                ?? Path.Combine(_Environment.ContentRootPath, "wwwroot");

            if (!Directory.Exists(webRoot))
            {
                return Task.FromResult<IReadOnlyList<string>>(
                    new List<string>()
                );
            }

            var files = Directory
                .GetFiles(webRoot, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(webRoot, path))
                .ToList();

            return Task.FromResult<IReadOnlyList<string>>(files);
        }

        // ================= DELETE FOLDER =================

        private Task<bool> DeleteFolderAsync(string relativeFolder)
        {
            var folder = GetStoragePath(relativeFolder);

            if (!Directory.Exists(folder))
                return Task.FromResult(false);

            Directory.Delete(folder, recursive: true);

            return Task.FromResult(true);
        }

        // ================= DELETE ORGANIZATION FOLDER =================

        public Task DeleteFolderOrg(string FolderName)
        {
            var folderPath = Path.Combine(
                "Organizations",
                FolderName,
                "VerificationDoc"
            );

            return DeleteFolderAsync(folderPath);
        }

        // ================= DELETE RESTAURANT FOLDER =================

        public Task DeleteFolderRes(string FolderName)
        {
            var folderPath = Path.Combine(
                "Restaurants",
                FolderName,
                "VerificationDoc"
            );

            return DeleteFolderAsync(folderPath);
        }
    }
}
