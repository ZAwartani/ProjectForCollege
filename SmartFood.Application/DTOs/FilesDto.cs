using Microsoft.AspNetCore.Http;

namespace SmartFood.Application.DTOs
{
    public record UploadFile(IFormFile File);
    public record FileClient(int Id, string AddressNameFile);


}
