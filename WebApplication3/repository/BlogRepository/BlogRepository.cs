using CloudinaryDotNet;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WebApplication3.DTOs;
using WebApplication3.DTOs.Account;
using WebApplication3.DTOs.Blog;
using WebApplication3.DTOs.Groups;
using WebApplication3.DTOs.Member;
using WebApplication3.DTOs.Project;
using WebApplication3.DTOs.Sponsor;
using WebApplication3.Entities;
using WebApplication3.Helper.Data;
using static System.Net.WebRequestMethods;

namespace WebApplication3.repository.BlogRepository
{
    public interface IBlogRepository
    {
        Task<PagedResult<BlogDTO>> GetAllBlogs(int pageSize, int pageNumber, string? keyword = null, bool? approved = null);
        Task<BlogDTO> GetBlog(int id);
        Task<BlogDTO> GetBlogsByTitle(string title);
        Task UpdateBlog(int blog_id, BlogDTO blog);
        Task DeleteBlog(BlogDTO blog);
        Task<PagedResult<BlogDTO>> GetAllBlogsTrue(int pageNumber);
        Task UpdateStatus(bool Approved, int id);
        Task AddBlog(int id, BlogDTO blog);

        Task<PagedResult<BlogDTO>> GetAllBlogsByAccId(int acc_id, int pageNumber);
    }
    public class BlogRepository : IBlogRepository
    {
        private AppDbContext _context;
        public BlogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddBlog(int id, BlogDTO blog)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
    INSERT INTO Blog (Account_id, Title, Content)
    VALUES (@acc_id, @title, @content)";

            await connection.ExecuteAsync(sql, new
            {
                title = blog.Title,
                content = blog.Content,
                acc_id = id
            });
        }



        public async Task DeleteBlog(BlogDTO blog)
        {
            using var connection = _context.CreateConnection();
            var sql = "Delete From Blog where Title = @title";
            await connection.ExecuteAsync(sql, new { title = blog.Title });
        }
        public async Task UpdateStatus(bool Approved, int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "UPDATE Blog SET Approved = @Approved Where Blog_id = @id";
            await connection.ExecuteAsync(sql, new
            {
                Approved = Approved,
                id = id
            });
        }
        public async Task<PagedResult<BlogDTO>> GetAllBlogs(int pageSize, int pageNumber, string? keyword = null, bool? approved = null)
        {
            using var connection = _context.CreateConnection();

            var offset = (pageNumber - 1) * pageSize;
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"
SELECT b.*, a.*
FROM Blog AS b
LEFT JOIN account AS a ON b.Account_id = a.Account_id
WHERE 1=1 "); // Add a default condition to simplify appending additional conditions

            if (!string.IsNullOrEmpty(keyword))
            {
                sqlBuilder.Append("AND b.title LIKE @keyword ");
            }

            if (approved.HasValue)
            {
                sqlBuilder.Append("AND b.Approved = @approved ");
            }

            sqlBuilder.Append("LIMIT @pageSize OFFSET @offset;");

            var queryResult = await connection.QueryAsync<BlogDTO, AccountDTO, BlogDTO>(
                sqlBuilder.ToString(),
                (blog, account) =>
                {
                    blog.account = account;
                    return blog;
                },
                new { pageSize, offset, keyword = $"%{keyword}%", approved = approved.HasValue ? (approved.Value ? 1 : 0) : (int?)null },
                splitOn: "Account_id"
            );

            var countSqlBuilder = new StringBuilder();
            countSqlBuilder.Append("SELECT COUNT(*) FROM Blog WHERE 1=1 ");

            if (!string.IsNullOrEmpty(keyword))
            {
                countSqlBuilder.Append("AND title LIKE @keyword ");
            }

            if (approved.HasValue)
            {
                countSqlBuilder.Append("AND Approved = @approved ");
            }

            var totalCount = await connection.ExecuteScalarAsync<int>(countSqlBuilder.ToString(),
                new { keyword = $"%{keyword}%", approved = approved.HasValue ? (approved.Value ? 1 : 0) : (int?)null });

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResult<BlogDTO>
            {
                Data = queryResult.ToList(),
                TotalPages = totalPages
            };
        }




        public async Task<PagedResult<BlogDTO>> GetAllBlogsTrue(int pageNumber)
        {
            using var connection = _context.CreateConnection();
            var pageSize = 6;
            var offset = (pageNumber - 1) * pageSize;

            var sql = @"
SELECT b.*, a.*
FROM Blog AS b
LEFT JOIN account AS a ON b.Account_id = a.Account_id
WHERE Approved = true
LIMIT @pageSize OFFSET @offset";

            var queryResult = await connection.QueryAsync<BlogDTO, AccountDTO, BlogDTO>(
                sql,
                (blog, account) =>
                {
                    blog.account = account;
                    return blog;
                },
                new { pageSize, offset },
                splitOn: "Account_id"
            );

            var countSql = "SELECT COUNT(*) FROM Blog WHERE Approved = true";
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResult<BlogDTO>
            {
                Data = queryResult.ToList(),
                TotalPages = totalPages
            };
        }
        public async Task<PagedResult<BlogDTO>> GetAllBlogsByAccId(int acc_id, int pageNumber)
        {
            using var connection = _context.CreateConnection();
            const int pageSize = 6;
            var offset = (pageNumber - 1) * pageSize;

            var sql = @"
SELECT b.*
FROM Blog AS b
WHERE Account_id = @acc_id
LIMIT @pageSize OFFSET @offset";

            var queryResult = await connection.QueryAsync<BlogDTO>(sql, new { pageSize, offset, acc_id });

            var countSql = "SELECT COUNT(*) FROM Blog WHERE Account_id = @acc_id";
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { acc_id });

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResult<BlogDTO>
            {
                Data = queryResult.ToList(),
                TotalPages = totalPages
            };
        }

        public async Task<BlogDTO> GetBlog(int id)
        {

            using var connection = _context.CreateConnection();
            var sql = """
        SELECT b.*,a.*
        FROM Blog AS b
        LEFT JOIN account AS a ON b.Account_id = a.Account_id
        WHERE Blog_id = @id;
        """;
            var blog = await connection.QueryAsync<BlogDTO, AccountDTO, BlogDTO>(
        sql,
        (blog, account) =>
        {
            blog.account = account;

            return blog;
        }, new { id },
        splitOn: "Account_id"
    );
            return blog.FirstOrDefault();
        }


        public async Task<BlogDTO> GetBlogsByTitle(string title)
        {
            using var connection = _context.CreateConnection();
            var sql = """
        SELECT b.*, a.*
        FROM Blog AS b
         LEFT JOIN account AS a ON b.Account_id = a.Account_id
        WHERE Title = @title;
        """;
            var blog = await connection.QueryAsync<BlogDTO, AccountDTO, BlogDTO>(
        sql,
        (blog, account) =>
        {
            blog.account = account;

            return blog;
        }, new { title },
        splitOn: "Account_id"
    );
            return blog.FirstOrDefault();
        }

        public async Task UpdateBlog(int blog_id, BlogDTO blog)
        {
            using var connection = _context.CreateConnection();
            var sql = """
                update Blog Set Title= @title , Content = @content  , CreatedAt = CURRENT_TIMESTAMP() , Approved = 0 WHERE Blog_id = @Blog_id
                """;
            await connection.ExecuteAsync(sql, new
            {
                Blog_id = blog_id,
                title = blog.Title,
                content = blog.Content,

            });
        }
    }
}
