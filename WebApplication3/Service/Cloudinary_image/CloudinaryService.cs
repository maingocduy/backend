using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebApplication3.DTOs.ImageDto;
using WebApplication3.Helper;
using WebApplication3.repository.CloudRepository;

namespace WebApplication3.Service.Cloudinary_image
{
    public interface ICloudinaryService
    {
        public Task<ImageUploadResult> UpLoadSingleImage([FromForm] imageUpload imageUploadModels);
        public Task<List<ImageUploadResult>> UpLoadMultipleImage([FromForm] List<imageUpload> imageUploadModels);
        public Task<List<ImageUploadResult>> UploadImages([FromForm] List<IFormFile> imageUploadDatas);
        public Task DeleteImage(string publicId);

        public Task AddImageProject(CreateProjectImageRequest request);
        Task<string> GetPublicIdByProjectName(string projectName);
        Task<ImageDtos> GetImageDetail(string publicId);
    }
    public class CloudinaryService(IClouRepository clouRepository) : ICloudinaryService
    {
        public async Task<List<ImageUploadResult>> UploadImages([FromForm] List<IFormFile> imageUploadDatas)
        {
            Cloudinary cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
            cloudinary.Api.Secure = true;

            List<ImageUploadResult> uploadResults = new List<ImageUploadResult>();
            foreach (var imageUploadData in imageUploadDatas)
            {
                var fileName = imageUploadData.FileName + Guid.NewGuid().ToString();
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(fileName, imageUploadData.OpenReadStream()),
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };
                var uploadResult = await cloudinary.UploadAsync(uploadParams);
                uploadResults.Add(uploadResult);
            }

            return uploadResults;
        }

        public async Task<List<ImageUploadResult>> UpLoadMultipleImage(List<imageUpload> imageUploadModels)
        {
            var cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
            cloudinary.Api.Secure = true;

            List<ImageUploadResult> uploadResults = new List<ImageUploadResult>();

            foreach (var imageUploadModel in imageUploadModels)
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(imageUploadModel.FileName, imageUploadModel.file.OpenReadStream()),
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };
                var uploadResult = await cloudinary.UploadAsync(uploadParams);
                uploadResults.Add(uploadResult);
            }
            return uploadResults;
        }


        public async Task<ImageUploadResult> UpLoadSingleImage([FromForm] imageUpload imageUploadModels)
        {
            var cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));
            cloudinary.Api.Secure = true;
            var fileName = imageUploadModels.FileName + Guid.NewGuid().ToString(); // Thêm một phần tử độc đáo vào tên file
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(fileName, imageUploadModels.file.OpenReadStream()),
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true
            };
            var uploadResult = await cloudinary.UploadAsync(uploadParams);
            return uploadResult;
        }
        
        public async Task DeleteImage(string publicId)
        {
            var cloudinary = new Cloudinary(Environment.GetEnvironmentVariable("CLOUDINARY_URL"));

            try
            {
                var result = await cloudinary.DeleteResourcesAsync(publicId);

                // Xử lý kết quả nếu cần
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    // Xóa thành công
                    throw new Exception("lỗi khi xóa ảnh");
                   
                }
            }
            catch (Exception ex)
            {
                // Xử lý exception nếu có lỗi xảy ra
                throw new BadHttpRequestException("lỗi khi chạy code");
            }
        }

    /*    public async Task AddImageProject([FromForm] imageUpload imageUploadModels, string Image_content, string projectName)
        {
            var UpImage = await UpLoadSingleImage(imageUploadModels);
            var ImageDto = new ImageDtos()
            {
                Image_id = UpImage.PublicId,
                image_url = UpImage.SecureUrl.ToString(),
                image_content = Image_content
            };
            try { 
                await clouRepository.InsertImageToProject(projectName, ImageDto);

            }
            catch (Exception ex) {
                throw ex;
            }
        }*/
        public async Task AddImageProject(CreateProjectImageRequest request)
        {
          
            var ImageDto = new ImageDtos()
            {
                Image_id = request.publicID,
                image_url = request.url
            };
            try
            {
                await clouRepository.InsertImageToProject(request.ProjectName, ImageDto);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<string> GetPublicIdByProjectName(string projectName)
        {
            return await clouRepository.GetPublicIdImageByProject(projectName);
        }
        public async Task<ImageDtos> GetImageDetail(string publicId)
        {
            try
            {
                return await clouRepository.GetImageDetail(publicId);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
