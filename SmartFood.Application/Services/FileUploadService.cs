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
            ISpecificationBuilderFactory factory, ICurrentUser currentUser)
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

            // Clear the owner's reference to this document
            if (_CurrentUser.IsInRole("Restaurant"))
            {
                var restaurant = await _RepoRestaurant.GetByIdAsync(userId);

                if (restaurant != null &&
                    restaurant.VerificationId == document.Id)
                {
                    restaurant.VerificationId = null;

                    await _RepoRestaurant.UpdateAsync(restaurant);
                }
            }
            else if (_CurrentUser.IsInRole("Organization"))
            {
                var organization = await _RepoOrganization.GetByIdAsync(userId);

                if (organization != null &&
                    organization.VerificationId == document.Id)
                {
                    organization.VerificationId = null;

                    await _RepoOrganization.UpdateAsync(organization);
                }
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

        public async Task<IReadOnlyList<FileClient>>
    GetByOrgnizationIdAsync(int OrgnaizationId)
        {
            var spec = _Factory.CreateBuilder<VerificationDocument>()
                .AddCriteria(d => d.OrgnizationId == OrgnaizationId);

            var documents = await _RepoFile.ListAsync(spec);

            return documents.Select(d => new FileClient(
                d.Id,
                d.FilePath
            )).ToList();
        }

        public async Task<IReadOnlyList<FileClient>>
    GetByRestaurantIdAsync(int RestaurantId)
        {
            var spec = _Factory.CreateBuilder<VerificationDocument>()
                .AddCriteria(d => d.RestaurantId == RestaurantId);

            var documents = await _RepoFile.ListAsync(spec);

            return documents.Select(d => new FileClient(
                d.Id,
                d.FilePath
            )).ToList();
        }

        public Task<IReadOnlyList<string>> ListAsync()
        {
            return _FileStorage.ListFilesAsync();
        }

        public async Task<bool> UploadFileDocumenationOrganizationAsync(
     UploadFile dto)
        {
            // Find the current organization first
            var organization = _Factory.CreateBuilder<Organization>()
                .AddCriteria(x => x.Id == userId);

            var result = await _RepoOrganization.GetAsync(organization);

            if (result == null)
            {
                return false;
            }

            var verificationFolder = Path.Combine(
    "Organizations",
    result.OrganizationName,
    "VerificationDoc"
);

            _FileStorage.CreateFolder(verificationFolder);

            var fileName = await _FileStorage.SaveFileAsync(
                dto.File,
                verificationFolder
            );
            // Save the document information in the database
            var document = new VerificationDocument(
                userId,
                null,
                fileName
            );

            await _RepoFile.AddAsync(document);

            // Link the organization to its selected document
            result.VerificationId = document.Id;

            await _RepoOrganization.UpdateAsync(result);

            return true;
        }

        public async Task<bool> UploadFileDocumenationRestaurantAsync(
            UploadFile dto)
        {
            // Find the current restaurant first
            var restaurant = _Factory.CreateBuilder<Restaurant>()
                .AddCriteria(x => x.Id == userId);

            var result = await _RepoRestaurant.GetAsync(restaurant);

            if (result == null)
            {
                return false;
            }

            var restaurantFolder = Path.Combine("Restaurants",
                result.RestaurantName
             );

            var productsFolder = Path.Combine(
                restaurantFolder,
                "Products"
            );

            var verificationFolder = Path.Combine(
                restaurantFolder,
                "VerificationDoc"
            );

            _FileStorage.CreateFolder(productsFolder);
            _FileStorage.CreateFolder(verificationFolder);

            var fileName = await _FileStorage.SaveFileAsync(
                dto.File,
                verificationFolder
            );

            // Save the document information in the database
            var document = new VerificationDocument(
                null,
                userId,
                fileName
            );

            await _RepoFile.AddAsync(document);

            // Link the restaurant to its selected document
            result.VerificationId = document.Id;

            await _RepoRestaurant.UpdateAsync(result);

            return true;
        }
    }
}
