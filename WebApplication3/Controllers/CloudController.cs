using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebApplication3.DTOs.ImageDto;
using WebApplication3.Helper;
using WebApplication3.Service.Cloudinary_image;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CloudController(ICloudinaryService cloudinaryService) : ControllerBase
    {
        [HttpPost("uploadsingle")]
        public async Task<IActionResult> UpLoadSingleImage([FromForm] imageUpload imageUploadModels)
        {
            if (imageUploadModels == null)
            {
                return BadRequest("No model");
            }

            if (imageUploadModels.file == null)
            {
                return BadRequest("No data");
            }

            var uploadResult = await cloudinaryService.UpLoadSingleImage(imageUploadModels);
            if (uploadResult.StatusCode != HttpStatusCode.OK)
            {
                return BadRequest();
            }

            return Ok(new { message = "Uploaded successfully!", result = uploadResult });
        }
        [HttpPost("uploadMulti")]
        public async Task<IActionResult> UpLoadMultipleImage([FromForm] List<imageUpload> imageUploadModels)
        {
            if (imageUploadModels == null)
            {
                return BadRequest("No model");
            }

            if (imageUploadModels.Count == 0)
            {
                return BadRequest("No data");
            }

            try
            {
                var uploadResults = await cloudinaryService.UpLoadMultipleImage(imageUploadModels);
                return Ok(new { message = "Uploaded successfully!", result = uploadResults });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading images: {ex.Message}");
            }
        }
        [HttpPost("uploadTinySingle")]
        public async Task<IActionResult> UpLoadTinySingleImage([FromForm] imageUpload imageUploadModels)
        {
            if (imageUploadModels == null)
            {
                return BadRequest("No model");
            }

            if (imageUploadModels.file == null)
            {
                return BadRequest("No data");
            }

            var uploadResult = await cloudinaryService.UpLoadSingleImage(imageUploadModels);
            if (uploadResult.StatusCode != HttpStatusCode.OK)
            {
                   return BadRequest();
            }

            return Ok(new { location =uploadResult.SecureUrl });
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody]string Public_id)
        {
            try
            {
                await cloudinaryService.DeleteImage(Public_id);
                return Ok();
            }
            catch
            {
                return BadRequest("Lỗi ");
            }
        }
        [HttpPost("UpImageProject")]
        public async Task<IActionResult> UpImageProject(CreateProjectImageRequest request)
        {
            try
            {
                await cloudinaryService.AddImageProject(request);
                return Ok(new { message = "Tạo thành công dự án mới !" });
            }
            catch
            {
                return BadRequest("Lỗi khi up ảnh");
            }
        }
        [HttpGet("GetDetail")]
        public async Task<ActionResult<ImageDtos>> GetImageDetail(string PublicId)
        {
            try
            {
                return Ok(await cloudinaryService.GetImageDetail(PublicId));
            }
            catch
            {
                return BadRequest("Lỗi khi up ảnh");
            }
        }

    }
}
