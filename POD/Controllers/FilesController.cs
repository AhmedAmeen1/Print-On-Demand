using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;

namespace POD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly Cloudinary _cloudinary;

        public FilesController(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public class UploadImageRequest
        {
            public IFormFile File { get; set; }
        }

        [HttpPost("UploadImage")]
        public async Task<IActionResult> UploadImage([FromForm] UploadImageRequest request)
        {
            var file = request.File;

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Optional: validate file type and size here

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = "product-images" // Optional Cloudinary folder
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Ok(new { imageUrl = uploadResult.SecureUrl.ToString() });
            }
            else
            {
                return StatusCode(500, "Image upload failed.");
            }
        }
    }
}
