using AutoMapper;
using K4os.Compression.LZ4.Internal;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Xml.Linq;
using WebApplication3.DTOs;
using WebApplication3.DTOs.Account;
using WebApplication3.DTOs.Blog;
using WebApplication3.DTOs.Member;
using WebApplication3.repository.AccountRepository;
using WebApplication3.repository.BlogRepository;
using WebApplication3.repository.GroupsRepository;
using WebApplication3.repository.MemberRepository;
using WebApplication3.repository.ProjectReposiotry;

namespace WebApplication3.Service.BlogService
{
    public interface IBlogService
    {
        Task<PagedResult<BlogDTO>> GetAllBlogByAcc_id(int acc_id, int pageNumber);
        Task<PagedResult<BlogDTO>> GetAllBlog(int pageSize, int pageNumber, string? keyword = null, bool? approved = null);
        Task<BlogDTO> GetBlogsAsync(int id);

        Task<BlogDTO> GetBlogsByTitle(string title);

        Task UpdateBlog(updateBlogRequestDTO Blog);
        Task DeleteBlog(string title);
        Task UpdateStatus(updateApprovedRequest request);
        Task AddBlog(string jwt, CreateRequestBLogDTO create);
        Task<PagedResult<BlogDTO>> GetAllBlogTrue(int pageNumber);
    }
    public class BlogService : IBlogService
    {
        private readonly repository.BlogRepository.IBlogRepository IBlogRepository;
        private readonly IMapper _mapper;
        private readonly IAccountRepository IAccountRepository;
        public BlogService(IBlogRepository IBlogRepository, IAccountRepository IAccountRepository, IMapper mapper)
        {
            this.IBlogRepository = IBlogRepository;
            this.IAccountRepository = IAccountRepository;
            _mapper = mapper;
        }

        public async Task AddBlog(string jwt, CreateRequestBLogDTO create)
        {

            // Giải mã JWT và trích xuất thông tin người dùng
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(jwt);
            // Lấy các claim từ JWT để lấy thông tin người dùng
            var userName = jwtToken.Claims.Where(
                c => c.Type == ClaimTypes.Name
                ).FirstOrDefault().Value;
            var BlogDTO = _mapper.Map<BlogDTO>(create);
            if(string.IsNullOrEmpty(create.Content) || string.IsNullOrEmpty(create.Title)) {
                throw new Exception("Nhập thiếu trường bắt buộc!");
            }
            var id = await IAccountRepository.getIDAcount(userName);
            await IBlogRepository.AddBlog(id, BlogDTO);
      
        }

        public async Task UpdateStatus(updateApprovedRequest request)
        {
            try
            {
                var getBlog = await IBlogRepository.GetBlog(request.Id);
                if (getBlog == null)
                {
                    throw new KeyNotFoundException("Không tìm thấy blog");
                }
                if (getBlog.Approved)
                {
                    await IBlogRepository.UpdateStatus(false, request.Id);
                }
                else
                {
                    await IBlogRepository.UpdateStatus(true, request.Id);
                }
            }
            catch(KeyNotFoundException ex) {
                throw ex;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task DeleteBlog(string title)
        {
            var blog = await GetBlogsByTitle(title);
            if(blog == null)
            {
                throw new KeyNotFoundException("Không tìm thấy blog!");
            }
            await IBlogRepository.DeleteBlog(blog);
        }

        public async Task<PagedResult<BlogDTO>> GetAllBlog(int pageSize, int pageNumber, string? keyword = null, bool? approved = null)
        {
            return await IBlogRepository.GetAllBlogs(pageSize,pageNumber,keyword,approved);
        }

        public async Task<PagedResult<BlogDTO>> GetAllBlogTrue(int pageNumber)
        {
            return await IBlogRepository.GetAllBlogsTrue(pageNumber);
        }
        public async Task<PagedResult<BlogDTO>> GetAllBlogByAcc_id(int acc_id, int pageNumber)
        {
            return await IBlogRepository.GetAllBlogsByAccId(acc_id, pageNumber);
        }
       
        public async Task<BlogDTO> GetBlogsAsync(int id)
        {
            var blog = await IBlogRepository.GetBlog(id);

            if (blog == null)
                throw new KeyNotFoundException("Không tìm thấy Blog");

            return blog;
        }

        public async Task<BlogDTO> GetBlogsByTitle(string title)
        {
            var blog = await IBlogRepository.GetBlogsByTitle(title);

            if (blog == null)
                throw new KeyNotFoundException("Không tìm thấy Blog");

            return blog;
        }

        public async Task UpdateBlog(updateBlogRequestDTO Blog)
        {
            var existingBlog = await IBlogRepository.GetBlog(Blog.Blog_id);
            if (existingBlog == null)
            {
                throw new Exception($"Không tìm thấy blog!");
            }

            // Kiểm tra xem dự án có bị trùng tên không

            // Validate update request


            // Cập nhật dữ liệu dự án
            existingBlog.Content = Blog.Content;
            existingBlog.Title = Blog.Title;
            // Assume other necessary fields are set accordingly

            // Gọi repository để cập nhật dự án
            await IBlogRepository.UpdateBlog(Blog.Blog_id, existingBlog);

            
        }
    }
}
