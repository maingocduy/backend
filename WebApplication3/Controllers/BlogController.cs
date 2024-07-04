using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.DTOs.Blog;
using WebApplication3.DTOs.Project;
using WebApplication3.Entities;
using WebApplication3.Helper;
using WebApplication3.Helper.Data;
using WebApplication3.Service.AccountService;
using WebApplication3.Service.BlogService;
using WebApplication3.Service.ProjectService;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService IBlogService;
        public BlogController(IBlogService IBlogService)
        {
            this.IBlogService = IBlogService;
        }
        [HttpPost("all_blog"), Authorize(Roles = "Manager")]
        public async Task<ActionResult> GetAllBlog(GetAllBlogRequest request)
        {
            try
            {
                var result = await IBlogService.GetAllBlog(request.pageSize, request.pageNumber,request.keyword,request.approved);
                return Ok(new { blogs = result.Data, totalPages = result.TotalPages });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("delete_blog"), Authorize]
        public async Task<IActionResult> Delete(string title)
        {
            try
            {
                await IBlogService.DeleteBlog(title);
                return Ok(new { message = "Xóa thành công" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống" });
            }
        }
        [HttpPost("all_blog_approve")]
        public async Task<ActionResult> GetAllBlogApprove(int pageNumber = 1)
        {
            try
            {
                var result = await IBlogService.GetAllBlogTrue(pageNumber);
                return Ok(new { blogs = result.Data, totalPages = result.TotalPages });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("all_blog_by_id"),Authorize]
        public async Task<ActionResult> GetAllBlogById(GetAllAccountByIdRequestDto request)
        {
            try
            {
                var result = await IBlogService.GetAllBlogByAcc_id(request.acc_id,request.pageNumber);
                return Ok(new { blogs = result.Data, totalPages = result.TotalPages });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> AddBlog(CreateRequestBLogDTO createBlog)
        {
            try
            {
                var jwt = Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
                await IBlogService.AddBlog(jwt,createBlog);
                return Ok(new { Message = "Thêm thành công blog mới!" });
            }
            catch (System.UnauthorizedAccessException ex)
            {
                // Xử lý và trả về lỗi 401
                return StatusCode(401, $"Unauthorized access: {ex.Message}");
            }
            catch (ForbiddenAccessException ex)
            {
                // Xử lý và trả về lỗi 403
                return StatusCode(403, $"Forbidden access: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Xử lý các ngoại lệ khác và trả về lỗi 500
                return BadRequest(new { Message = ex.Message });
            };
          
        }

        [HttpPost("update_status"), Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateStatus(updateApprovedRequest request)
        {
            try
            {
                await IBlogService.UpdateStatus(request);
                return Ok(new { Message = "Duyệt thành công" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpGet("get_blog")]
        public async Task<IActionResult> GetBlog(string title)
        {
            try
            {
                return Ok(await IBlogService.GetBlogsByTitle(title));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new {message = ex.Message});
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message
                    );
            }
        }
        [HttpGet("get_blog_by_id")]
        public async Task<IActionResult> GetBlogById(int id)
        {
            try
            {
                return Ok(await IBlogService.GetBlogsAsync(id));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message
                    );
            }
        }
        [HttpPost("update_blog"), Authorize]
        public async Task<IActionResult> UpdateBlog(updateBlogRequestDTO request)
        {
            try
            {
                await IBlogService.UpdateBlog(request);
                return Ok(new { Message = "Sửa thành công! Hãy chờ để Manager duyệt blog" });  
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message
                    );
            }
        } 
    }
}
