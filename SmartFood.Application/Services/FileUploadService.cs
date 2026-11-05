using SmartFood.Application.Concrete.Factories;
using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;
using SmartFood.Application.Interfaces.Factory;
using SmartFood.Application.Interfaces.Specification;
using SmartFood.Core.Entity;

namespace SmartFood.Application.Services
{
    public class FileUploadService : IFileService
    {
        private readonly IFileStorageService _FileStorage;

        private readonly IGenericRepository<VerificationDocument, int> _RepoFile;

        private readonly IGenericRepository<Organization, int> _RepoOrganization;

        private readonly IGenericRepository<Restaurant, int> _RepoRestaurant;

        private readonly ISpecificationBuilderFactory _Factory;

        private readonly ICurrentUser _CurrentUser;

        public FileUploadService(IFileStorageService fileStorage,
            IGenericRepository<VerificationDocument, int> repoFile,
            IGenericRepository<Organization, int> repoOrganization,
            IGenericRepository<Restaurant, int> repoRestaurant,
            SpeificationBuilderFactory factory, ICurrentUser currentUser)
        {
            _FileStorage = fileStorage;
            _RepoFile = repoFile;
            _RepoOrganization = repoOrganization;
            _RepoRestaurant = repoRestaurant;
            _Factory = factory;
            _CurrentUser = currentUser;
        }

        private int userId => _CurrentUser.Id;

        public async Task<bool> DeleteAsync(int FileId)
        {
            if (!_CurrentUser.IsAuthenticated)
            {
                throw new UnauthorizedAccessException(
                    "You must log in before deleting a file."
                );
            }

            var document = await _RepoFile.GetByIdAsync(FileId);

            if (document == null)
            {
                return false;
            }

            bool ownsDocument =
            (_CurrentUser.IsInRole("Restaurant") &&
             document.RestaurantId == _CurrentUser.Id)
            ||
            (_CurrentUser.IsInRole("Organization") &&
             document.OrgnizationId == _CurrentUser.Id);

            if (!ownsDocument)
            {
                throw new UnauthorizedAccessException(
                    "You can only delete your own files."
                );
            }

            var fileDeleted = await _FileStorage.DeleteFileAsync(
                document.FilePath
                );

            if (!fileDeleted)
            {
                return false;
            }

            await _RepoFile.DeleteAsync(document.Id);

            return true;


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

        public async Task<bool> UploadFileDocumenationOrganizationAsync(UploadFile dto)
        {
            // Save the actual file on disk
            var fileName = await _FileStorage.SaveFileAsync(dto.File);

            // Create the database entity
            var document = new VerificationDocument(
                fileName,
                userId,
                null
            );

            // Save the document information in the database
            await _RepoFile.AddAsync(document);

            var organization = _Factory.CreateBuilder<Organization>()
                .AddCriteria(x => x.Id == userId);

            var result = await _RepoOrganization.GetAsync(organization);

            if (result == null)
            {
                return false;
            }

            result.VerificationId = document.Id;

            await _RepoOrganization.UpdateAsync(result);


            return true;
        }

        public async Task<bool> UploadFileDocumenationRestaurantAsync(UploadFile dto)
        {
            // Save the actual file on disk
            var fileName = await _FileStorage.SaveFileAsync(dto.File);

            // Create the database entity
            var document = new VerificationDocument(
                fileName,
                null,
                userId
            );

            // Save the document information in the database
            await _RepoFile.AddAsync(document);

            var restaurant = _Factory.CreateBuilder<Restaurant>()
                .AddCriteria(x => x.Id == userId);

            var result = await _RepoRestaurant.GetAsync(restaurant);

            if (result == null)
            {
                return false;
            }

            result.VerificationId = document.Id;

            await _RepoRestaurant.UpdateAsync(result);

            return true;
        }
    }
}
