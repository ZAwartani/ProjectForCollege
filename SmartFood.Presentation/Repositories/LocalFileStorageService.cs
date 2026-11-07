using SmartFood.Application.Interfaces;

namespace SmartFood.Presentation.Repositories
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _Environment;

        private readonly IConfiguration _Configuration;
        public LocalFileStorageService(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _Environment = environment;
            _Configuration = configuration;
        }

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

        public void CreateFolder(string relativeFolder)
        {
            var folder = GetStoragePath(relativeFolder);

            Directory.CreateDirectory(folder);
        }

        public async Task<string> SaveFileAsync(IFormFile file, string relativeFolder)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty");

            const long maxFileSize = 2 * 1024 * 1024;

            if (file.Length > maxFileSize)
                throw new Exception("File size must be 2 MB or less");

            var folder = GetStoragePath(relativeFolder);

            Directory.CreateDirectory(folder);

            var fileName =
                Guid.NewGuid() + Path.GetExtension(file.FileName);

            var path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);

            await file.CopyToAsync(stream);

            return Path.Combine(relativeFolder, fileName);
        }


        public Task<bool> DeleteFileAsync(string fileName)
        {
            var path = GetStoragePath(fileName);

            if (File.Exists(path))
                File.Delete(path);

            return Task.FromResult(true);
        }

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
    }
}
