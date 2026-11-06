using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;
using System.Security.Claims;

namespace SmartFood.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileService _FileService;

        public FileUploadController(IFileService fileService)
        {
            _FileService = fileService;
        }


        [HttpPost("organization/upload")]
        [Authorize(Roles = "Organization")]
        public async Task<IActionResult> UploadOrganization(
    [FromForm] UploadFile dto)
        {
            var result =
                await _FileService.UploadFileDocumenationOrganizationAsync(dto);

            if (!result)
            {
                return BadRequest("Document upload failed.");
            }

            return Ok("Document uploaded successfully.");
        }


        [HttpPost("restaurant/upload")]
        [Authorize(Roles = "Restaurant")]
        public async Task<IActionResult> UploadRestaurant(
       [FromForm] UploadFile dto)
        {
            var result =
                await _FileService.UploadFileDocumenationRestaurantAsync(dto);

            if (!result)
            {
                return BadRequest("Document upload failed.");
            }

            return Ok("Document uploaded successfully.");
        }

        [HttpDelete("{fileId}")]
        [Authorize(Roles = "Restaurant,Organization")]
        public async Task<IActionResult> Delete(int fileId)
        {
            var result = await _FileService.DeleteAsync(fileId);

            if (!result)
            {
                return NotFound("Document not found.");
            }

            return NoContent();
        }


        [HttpGet("organization/files")]
        [Authorize(Roles = "Organization")]
        public async Task<IActionResult> GetOrganizationFiles()
        {
            int organizationId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await _FileService
                .GetByOrgnizationIdAsync(organizationId);

            return Ok(result);
        }


        [HttpGet("restaurant/files")]
        [Authorize(Roles = "Restaurant")]
        public async Task<IActionResult> GetRestaurantFiles()
        {
            int restaurantId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var result = await _FileService
                .GetByRestaurantIdAsync(restaurantId);

            return Ok(result);
        }
    }
}