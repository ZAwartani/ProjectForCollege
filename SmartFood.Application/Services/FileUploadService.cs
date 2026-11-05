using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;
using SmartFood.Application.Interfaces.Factory;
using SmartFood.Application.Interfaces.Specification;
using SmartFood.Core.Entity;
namespace SmartFood.Application.Services
{
    public class FileUploadService : IFileService
    {
        private readonly IGenericRepository<VerificationDocument, int> _RepoFile;
        private readonly IGenericRepository<Restaurant, int> _RepoRestaurant;
        private readonly IGenericRepository<Organization, int> _RepoOrganization;
        private readonly IFileStorageService _FileStorage;
        private readonly ISpecificationBuilderFactory _Factory;
        private readonly ICurrentUser _CurrentUser;

        public FileUploadService(
              IGenericRepository<VerificationDocument, int> repoFile
            , IGenericRepository<Restaurant, int> repoRestaurant
            , IGenericRepository<Organization, int> repoOrganization
            , IFileStorageService fileStorage
            , ISpecificationBuilderFactory factory)
        {
            _RepoFile = repoFile;
            _RepoRestaurant = repoRestaurant;
            _RepoOrganization = repoOrganization;
            _FileStorage = fileStorage;
            _Factory = factory;
        }

        private int UserId => _CurrentUser.Id;

        public Task<bool> DeleteAsync(int FileId)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<FileClient>> GetByOrgnizationIdAsync(int OrgnaizationId)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<FileClient>> GetByRestaurantIdAsync(int RestaurantId)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<string>> ListAsync()
        {
            throw new NotImplementedException();
        }

        public Task<UploadFile> UploadFileAsync(UploadFile dto)
        {
            throw new NotImplementedException();
        }
    }
}
