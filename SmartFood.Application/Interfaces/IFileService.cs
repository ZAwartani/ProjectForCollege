using SmartFood.Application.DTOs;

namespace SmartFood.Application.Interfaces
{
    public interface IFileService
    {
        Task<bool> UploadFileDocumenationRestaurantAsync(UploadFile dto);
        Task<bool> UploadFileDocumenationOrganizationAsync(UploadFile dto);
        Task<IReadOnlyList<string>> ListAsync();
        Task<IReadOnlyList<FileClient>> GetByRestaurantIdAsync(int RestaurantId);
        Task<IReadOnlyList<FileClient>> GetByOrgnizationIdAsync(int OrgnaizationId);
        Task<bool> DeleteAsync(int FileId);


    }
}
