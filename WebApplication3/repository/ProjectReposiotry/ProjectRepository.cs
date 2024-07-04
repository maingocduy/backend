using Dapper;
using Microsoft.EntityFrameworkCore;
using WebApplication3.DTOs;
using WebApplication3.DTOs.Account;
using WebApplication3.DTOs.Groups;
using WebApplication3.DTOs.ImageDto;
using WebApplication3.DTOs.Member;
using WebApplication3.DTOs.Project;
using WebApplication3.DTOs.Sponsor;
using WebApplication3.Entities;
using WebApplication3.Helper.Data;
using static System.Net.Mime.MediaTypeNames;

namespace WebApplication3.repository.ProjectReposiotry
{
    public interface IProjectRepository
    {
        Task AddProject(ProjectDTO project);
        Task DeleteProject(string name);
        Task<ProjectDTO> GetProject(string name);
        Task UpdateProject(ProjectDTO project);
        Task<PagedResult<ProjectDTO>> GetAllProject(int pageNumber = 1);
        Task<ProjectDTO> GetProjectID(string name);
        Task UpdateStatus(sbyte status, int id);
        Task<int> GetTotalSponsorCount();
        Task<int> GetTotalProjectCount();
        Task<int> SumContribution(int project_id);
        Task<decimal> GetTotalContributionAmount();
        Task<PagedResult<ProjectDTO>> GetAllProjectAprove(int pageNumber = 1);
        Task<PagedResult<ProjectDTO>> GetAllProjectNotExpired(int pageNumber = 1);
        Task<PagedResult<ProjectDTO>> GetAllProjectEndDate(int pageNumber = 1);
        Task<ProjectDTO> GetProjectByID(int Project_id);
        Task<List<ImageDtos>> GetImagesAsync(int project_id);
    }
    public class ProjectRepository : IProjectRepository
    {
        private AppDbContext _context;
        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddProject(ProjectDTO project)
        {
            using var connection = _context.CreateConnection();

            // Câu lệnh SQL để chèn một dự án mới
            var insertSql = @"
INSERT INTO Projects (Name, Description, StartDate, EndDate, Budget)
VALUES (@Name, @Description, @StartDate, @EndDate, @Budget);
";

            // Thực hiện chèn dữ liệu dự án mới
            await connection.ExecuteAsync(insertSql, new
            {
                Name = project.Name,
                Description = project.Description,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Budget = project.Budget
            });
        }


        public async Task DeleteProject(string name)
        {
            using var connection = _context.CreateConnection();

            // Đầu tiên, bạn cần xóa các mục phụ thuộc có thể tồn tại như chi tiết của dự án
            // ví dụ: xóa các mục trong MemberProjects mà có liên quan đến dự án
            var deleteMemberProjectsSql = @"
DELETE FROM MemberProjects
WHERE Project_id IN (
    SELECT Project_id FROM Projects WHERE Name = @ProjectName
);";

            // Thực hiện câu lệnh SQL xóa MemberProjects
            await connection.ExecuteAsync(deleteMemberProjectsSql, new { ProjectName = name });
            var deleteSponsorProjectsSql = @"
DELETE FROM projectsponsor
WHERE Project_id IN (
    SELECT Project_id FROM Projects WHERE Name = @ProjectName
);";

            // Thực hiện câu lệnh SQL xóa MemberProjects
            await connection.ExecuteAsync(deleteSponsorProjectsSql, new { ProjectName = name });


            var deleteProject_ImagesSql = @"
DELETE FROM Project_image
WHERE Project_id IN (
    SELECT Project_id FROM Projects WHERE Name = @ProjectName
);";

            // Thực hiện câu lệnh SQL xóa MemberProjects
            await connection.ExecuteAsync(deleteProject_ImagesSql, new { ProjectName = name });
            // Sau đó, xóa dự án
            var deleteProjectSql = @"
DELETE FROM Projects
WHERE Name = @ProjectName;";

            // Thực hiện câu lệnh SQL xóa Project
            await connection.ExecuteAsync(deleteProjectSql, new { ProjectName = name });
        }

        public async Task<PagedResult<ProjectDTO>> GetAllProject(int pageNumber = 1)
        {
            using var connection = _context.CreateConnection();
            int pageSize = 6;
            // Tính toán offset
            var offset = (pageNumber - 1) * pageSize;

            // Câu lệnh SQL mới
            var dtoSql = @"
        SELECT p.*
        FROM Projects AS p
        LIMIT @pageSize OFFSET @offset;
    ";

            // Thực hiện truy vấn
            var projects = await connection.QueryAsync<ProjectDTO>(
                dtoSql,
                new { pageSize, offset }
            );

            // Lấy tổng số dự án
            var countSql = "SELECT COUNT(*) FROM Projects";
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResult<ProjectDTO>
            {
                Data = projects.ToList(),
                TotalPages = totalPages
            };
        }

        public async Task<PagedResult<ProjectDTO>> GetAllProjectEndDate(int pageNumber = 1)
        {
            using var connection = _context.CreateConnection();
            int pageSize = 6;
            // Tính toán offset
            var offset = (pageNumber - 1) * pageSize;

            // Câu lệnh SQL mới
            var dtoSql = @"
SELECT p.*
FROM Projects AS p
 WHERE EndDate < CURDATE()
LIMIT @pageSize OFFSET @offset ;
";

            // Thực hiện truy vấn
            var projects = await connection.QueryAsync<ProjectDTO>(
                dtoSql,
                new { pageSize, offset }
            );

            // Lấy tổng số dự án
            var countSql = "SELECT COUNT(*) FROM Projects";
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResult<ProjectDTO>
            {
                Data = projects.ToList(),
                TotalPages = totalPages
            };
        }

        public async Task<int> SumContribution(int project_id)
        {
            var dtosqlproject = @"SELECT SUM(s.ContributionAmount) AS TotalContribution
FROM sponsor s
RIGHT JOIN projectsponsor sp ON s.sponsor_id = sp.sponsor_id
WHERE sp.project_id = @projectId;";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(dtosqlproject, new { projectId = project_id });
        }
        public async Task<PagedResult<ProjectDTO>> GetAllProjectNotExpired(int pageNumber = 1)
        {
            using var connection = _context.CreateConnection();
            int pageSize = 6;
            // Tính toán offset
            var offset = (pageNumber - 1) * pageSize;

            // Câu lệnh SQL mới
            var dtoSql = @"
        SELECT p.*
        FROM Projects AS p
        
        WHERE EndDate >= CURDATE()
        LIMIT @pageSize OFFSET @offset
    ";

            // Thực hiện truy vấn
            var projects = await connection.QueryAsync<ProjectDTO>(
                dtoSql,
                new { pageSize, offset }
            );

            // Lấy tổng số dự án
            var countSql = "SELECT COUNT(*) FROM Projects";
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResult<ProjectDTO>
            {
                Data = projects.ToList(),
                TotalPages = totalPages
            };


        }
        public async Task<List<ImageDtos>> GetImagesAsync(int project_id)
        {
            using var connection = _context.CreateConnection();
            var sql = @"SELECT * from project_image where Project_id = @project_id";
            var lst = await connection.QueryAsync<ImageDtos>(sql, new
            {
                project_id = project_id
            });
            return lst.ToList();
        }
        public async Task<PagedResult<ProjectDTO>> GetAllProjectAprove(int pageNumber = 1)
        {
            using var connection = _context.CreateConnection();
            int pageSize = 6;
            // Tính toán offset
            var offset = (pageNumber - 1) * pageSize;

            // Câu lệnh SQL mới
            var dtoSql = @"
SELECT p.*, m.*, g.*, s.*, i.*
FROM Projects AS p
LEFT JOIN MemberProjects AS mp ON p.Project_id = mp.Project_id
LEFT JOIN Members AS m ON mp.Member_id = m.Member_id
LEFT JOIN `Groups` AS g ON m.Group_id = g.Group_id
LEFT JOIN ProjectSponsor AS ps ON p.Project_id = ps.Project_id
LEFT JOIN sponsor AS s ON ps.Sponsor_id = s.Sponsor_id
LEFT JOIN Project_image AS i ON i.Project_id = p.Project_id WHERE p.status =1
LIMIT @pageSize OFFSET @offset;
";

            // Thực hiện truy vấn
            var projectsQuery = await connection.QueryAsync<ProjectDTO, MemberDTO, Group, SponsorDTO, ImageDtos, ProjectDTO>(
                dtoSql,
                (project, member, group, sponsor, image) =>
                {
                    project.Member ??= new List<MemberDTO>();
                    project.Sponsor ??= new List<SponsorDTO>();
                    project.images ??= new List<ImageDtos>();

                    if (member != null)
                    {
                        project.Member.Add(member);
                        member.groups = group;
                    }
                    if (sponsor != null)
                    {
                        project.Sponsor.Add(sponsor);
                    }
                    if (image != null)
                    {
                        project.images.Add(image);
                    }
                    return project;
                },
                new { pageSize, offset },
                splitOn: "Member_id,Group_id,Sponsor_id,image_id");

            var projects = projectsQuery.GroupBy(p => p.Project_id).Select(group =>
            {
                var groupedProject = group.First();

                if (groupedProject.Member != null && groupedProject.Member.Any())
                {
                    groupedProject.Member = group.Select(p => p.Member.FirstOrDefault()).ToList();
                }
                if (groupedProject.Sponsor != null && groupedProject.Sponsor.Any())
                {
                    groupedProject.Sponsor = group.Select(p => p.Sponsor.FirstOrDefault()).ToList();
                }
                if (groupedProject.images != null && groupedProject.images.Any())
                {
                    groupedProject.images = group.Select(p => p.images.FirstOrDefault()).ToList();
                }
                return groupedProject;
            }).ToList();

            // Lấy tổng số dự án
            var countSql = "SELECT COUNT(*) FROM Projects";
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResult<ProjectDTO>
            {
                Data = projects,
                TotalPages = totalPages
            };
        }

        public async Task<int> GetTotalProjectCount()
        {
            var dtosqlproject = @"SELECT COUNT(*) FROM Projects;";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(dtosqlproject);
        }

        public async Task<int> GetTotalSponsorCount()
        {
            var dtosqlsponsor = @"SELECT COUNT(*) FROM sponsor;";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(dtosqlsponsor);
        }

        public async Task<decimal> GetTotalContributionAmount()
        {
            var dtosqlcontribution = @"SELECT SUM(ContributionAmount) FROM sponsor;";
            using var connection = _context.CreateConnection();
            var result = await connection.ExecuteScalarAsync<decimal?>(dtosqlcontribution);
            return result ?? 0;
        }

        public async Task<ProjectDTO> GetProject(string name)
        {
            using var connection = _context.CreateConnection();

            // Câu lệnh SQL để lấy dự án theo tên và lọc theo Group_id nếu có
            var dtoSql = @"
SELECT p.*,i.*
FROM Projects AS p
LEFT JOIN Project_image AS i ON i.Project_id = p.Project_id
WHERE 
    p.Name = @ProjectName";

            // Nếu có GroupId, thêm điều kiện lọc cho Members
            // Thực hiện truy vấn
            var projectQuery = await connection.QueryAsync<ProjectDTO, ImageDtos, ProjectDTO>(
        dtoSql,
        (project, image) =>
        {
            // Kiểm tra và khởi tạo danh sách thành viên, nhóm, nhà tài trợ và ảnh nếu cần

            project.images ??= new List<ImageDtos>();

            // Thêm thành viên, nhóm, nhà tài trợ và ảnh nếu không null

            if (image != null)
            {
                project.images.Add(image);
            }

            return project;
        },
        new { ProjectName = name }, // Đối tượng ẩn danh với tên dự án
        splitOn: "Member_id,Group_id,Sponsor_id,image_id");

            // Lấy dự án đầu tiên hoặc trả về null nếu không có kết quả
            var project = projectQuery.GroupBy(p => p.Project_id).Select(group =>
            {
                var groupedProject = group.First();

                if (groupedProject.images != null && groupedProject.images.Any())
                {
                    groupedProject.images = group.Select(p => p.images.FirstOrDefault()).ToList();
                }
                return groupedProject;
            }).FirstOrDefault();

            // Trả về dự án hoặc null nếu không tìm thấy
            return project;
        }

        public async Task<ProjectDTO> GetProjectByID(int Project_id)
        {
            using var connection = _context.CreateConnection();

            // Câu lệnh SQL để lấy dự án theo tên và lọc theo Group_id nếu có
            var dtoSql = @"
SELECT p.*,i.*
FROM Projects AS p
LEFT JOIN Project_image AS i ON i.Project_id = p.Project_id
WHERE 
    p.Project_id = @Project_id";

            // Nếu có GroupId, thêm điều kiện lọc cho Members
            // Thực hiện truy vấn
            var projectQuery = await connection.QueryAsync<ProjectDTO, ImageDtos, ProjectDTO>(
        dtoSql,
        (project, image) =>
        {
            // Kiểm tra và khởi tạo danh sách thành viên, nhóm, nhà tài trợ và ảnh nếu cần

            project.images ??= new List<ImageDtos>();

            // Thêm thành viên, nhóm, nhà tài trợ và ảnh nếu không null

            if (image != null)
            {
                project.images.Add(image);
            }

            return project;
        },
        new { Project_id = Project_id }, // Đối tượng ẩn danh với tên dự án
        splitOn: "Member_id,Group_id,Sponsor_id,image_id");

            // Lấy dự án đầu tiên hoặc trả về null nếu không có kết quả
            var project = projectQuery.GroupBy(p => p.Project_id).Select(group =>
            {
                var groupedProject = group.First();

                if (groupedProject.images != null && groupedProject.images.Any())
                {
                    groupedProject.images = group.Select(p => p.images.FirstOrDefault()).ToList();
                }
                return groupedProject;
            }).FirstOrDefault();

            // Trả về dự án hoặc null nếu không tìm thấy
            return project;
        }


        public async Task<ProjectDTO> GetProjectID(string name)
        {
            using var connection = _context.CreateConnection();

            // Câu lệnh SQL truy vấn ID của dự án
            var sql = @"
        SELECT Project_id FROM Projects
        WHERE Name = @name;
    ";

            // Thực hiện truy vấn và trả về ID của dự án
            return await connection.QueryFirstOrDefaultAsync<ProjectDTO>(sql, new { name });
        }

        public async Task UpdateProject(ProjectDTO project)
        {
            using var connection = _context.CreateConnection();

            // Câu lệnh SQL cập nhật dự án
            var sql = @"
                UPDATE Projects
                SET Name = @Name, Budget = @Budget, Description = @Description, 
                    StartDate = @StartDate, EndDate = @EndDate
                WHERE Project_id = @Project_id;";

            // Thực hiện cập nhật
            await connection.ExecuteAsync(sql, project);

        }

        public async Task UpdateStatus(sbyte status, int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "UPDATE Projects SET status = @status Where Project_id = @id";
            await connection.ExecuteAsync(sql, new
            {
                status = status,
                id = id
            });
        }
    }
}
