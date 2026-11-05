using SmartFood.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFood.Application.Interfaces
{
    public interface IFileService
    {
        Task<UploadFile> UploadFileAsync(UploadFile dto);
        Task<IReadOnlyList<string>> ListAsync();
        Task<IReadOnlyList<FileClient>> GetByRestaurantIdAsync(int RestaurantId);
        Task<IReadOnlyList<FileClient>> GetByOrgnizationIdAsync(int OrgnaizationId);
        Task<bool> DeleteAsync(int FileId);

    }
}
