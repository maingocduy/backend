using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using WebApplication3.DTOs.Groups;
using WebApplication3.DTOs.ImageDto;
using WebApplication3.DTOs.Member;
using WebApplication3.DTOs.Project;
using WebApplication3.DTOs.Sponsor;
using WebApplication3.Helper.Data;

namespace WebApplication3.repository.CloudRepository
{
    public interface IClouRepository
    {
        Task<List<ImageDtos>> GetALLImageDtosAsync(string PubliicId);
        Task InsertImageToProject(string ProjectName, ImageDtos image);
        Task<String> GetPublicIdImageByProject(string ProjectName);
        Task<ImageDtos> GetImageDetail(string publicID);
    }
    public class CloudRepository : IClouRepository
    {
        private AppDbContext _context;
        public CloudRepository(AppDbContext _context)
        {
            this._context = _context;
        }
        public async Task<List<ImageDtos>> GetALLImageDtosAsync(string ProjectName)
        {
            using var connection = _context.CreateConnection();

            // Câu lệnh SQL để lấy dự án theo tên
            var dtoSql = @"
    SELECT * from Project_image where Project_id = ( SELECT Project_id FROM Projects WHERE Name = @ProjectName);";// @ProjectName là tham số truyền vào

            // Thực hiện truy vấn
            var projectQuery = await connection.QueryAsync<ImageDtos>(
                dtoSql,
                (new { ProjectName })
            );
            return projectQuery.ToList();
        }
        public async Task InsertImageToProject(string ProjectName, ImageDtos image)
        {
            using var connection = _context.CreateConnection();

            // Câu lệnh SQL để lấy dự án theo tên
            var SelectPRojectSql = "SELECT Project_id FROM Projects WHERE Name = @ProjectName";

            // Thực hiện truy vấn để lấy ID của dự án
            var projectQuery = await connection.ExecuteScalarAsync<int>(SelectPRojectSql, new
            {
                ProjectName = ProjectName
            });

            // Nếu không tìm thấy ID của dự án, bạn có thể muốn xử lý trường hợp này ở đây

            // Câu lệnh SQL để chèn dữ liệu vào bảng Project_image
            var dtoSql = @"
        INSERT INTO Project_image (image_id, image_url, image_content, Project_id)
        VALUES (@image_id, @image_url, @image_content, @Project_id);
    ";

            // Thực hiện chèn dữ liệu
            await connection.ExecuteAsync(dtoSql, new
            {
                image_id = image.Image_id,
                image_url = image.image_url,
                image_content = image.image_content,
                Project_id = projectQuery
            });
        }


        public async Task <String> GetPublicIdImageByProject(string ProjectName)
        {
            using var connection = _context.CreateConnection();

            // Câu lệnh SQL để lấy dự án theo tên
            var SelectPRojectSql = "SELECT image_id from Project_image where Project_id = (select Project_id from Projects where Name = @projectName)";

            // Thực hiện truy vấn để lấy ID của dự án
            var projectQuery = await connection.ExecuteScalarAsync<string>(SelectPRojectSql, new
            {
                projectName = ProjectName
            });

            // Nếu không tìm thấy ID của dự án, bạn có thể muốn xử lý trường hợp này ở đây

            // Câu lệnh SQL để chèn dữ liệu vào bảng Project_image
            return projectQuery;

        }
        public async Task<ImageDtos> GetImageDetail(string publicID)
        {
            using var connection = _context.CreateConnection();

            // Câu lệnh SQL để lấy dự án theo tên
            var SelectImageSql = "SELECT * from Project_image where image_id = @id";

            // Thực hiện truy vấn để lấy ID của dự án
     return await connection.QueryFirstOrDefaultAsync<ImageDtos>(SelectImageSql, new
            {
                id = publicID
            });

        }
    }
}

