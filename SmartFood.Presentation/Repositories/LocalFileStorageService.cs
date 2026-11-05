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

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            var uploadPath = _Configuration["File:Upload"];

            if (file == null || file.Length == 0)
                throw new Exception("File is empty");

            const long maxFileSize = 2 * 1024 * 1024;

            if (file.Length > maxFileSize)
                throw new Exception("File size must be 2 MB or less");

            var folder = Path.Combine(
                _Environment.WebRootPath,
                uploadPath
            );

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

            var path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);

            await file.CopyToAsync(stream);

            return fileName;
        }


        public Task<bool> DeleteFileAsync(string fileName)
        {
            var uploadPath = _Configuration["File:Upload"];

            var path = Path.Combine(
                _Environment.WebRootPath,
                uploadPath,
                fileName
            );

            if (File.Exists(path))
                File.Delete(path);

            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<string>> ListFilesAsync()
        {
            var folder = Path.Combine(_Environment.WebRootPath, "uploads");

            if (!Directory.Exists(folder))
                return Task.FromResult((IReadOnlyList<string>)new List<string>());

            var files = Directory.GetFiles(folder)
                .Select(Path.GetFileName)
                .ToList();

            return Task.FromResult((IReadOnlyList<string>)files);
        }
    }
}
