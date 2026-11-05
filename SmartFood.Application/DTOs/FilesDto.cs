using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFood.Application.DTOs
{
    public record UploadFile(IFormFile File , int Id);
    public record FileClient(int Id, string AddressNameFile);
}
