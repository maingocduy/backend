using K4os.Compression.LZ4.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using WebApplication3.DTOs;
using WebApplication3.DTOs.Blog;
using WebApplication3.DTOs.ImageDto;
using WebApplication3.DTOs.Project;
using WebApplication3.DTOs.Sponsor;
using WebApplication3.Entities;
using WebApplication3.repository.AccountRepository;
using WebApplication3.Service.AccountService;
using WebApplication3.Service.BlogService;
using WebApplication3.Service.MemberService;
using WebApplication3.Service.ProjectService;
using WebApplication3.Service.SponsorService;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService IProjectService;
        public ProjectController(IProjectService IProjectService)
        {
            this.IProjectService = IProjectService;
        }
        [HttpGet("get_all_project")]
        public async Task<IActionResult> GetAllProject([FromQuery] int pageNumber = 1)
        {
            var result = await IProjectService.GetAllProject(pageNumber);
            return Ok(new { projects = result.Data, totalPages = result.TotalPages });
        }
        [HttpGet("get_all_project_out_date")]
        public async Task<IActionResult> GetAllProjectOutDate([FromQuery] int pageNumber = 1)
        {
            var result = await IProjectService.GetAllProjectEndDate(pageNumber);
            return Ok(new { projects = result.Data, totalPages = result.TotalPages });
        }
        [HttpGet("get_all_project_in_date")]
        public async Task<IActionResult> GetAllProjectInDate([FromQuery] int pageNumber = 1)
        {
            var result = await IProjectService.GetAllProjectNotExpired(pageNumber);
            return Ok(new { projects = result.Data, totalPages = result.TotalPages });
        }
        [HttpGet("get_all_project_aprove")]
        public async Task<IActionResult> GetAllProjectAprove([FromQuery] int pageNumber = 1)
        {
            var result = await IProjectService.GetAllProjectAprove(pageNumber);
            return Ok(new { projects = result.Data, totalPages = result.TotalPages });
        }
        [HttpGet("overview")]
        public async Task<ActionResult<OverViewDTO>> GetOverView()
        {
            var overview = await IProjectService.GetOverView();
            return Ok(overview);
        }
        [HttpPost("get_project")]
        public async Task<ActionResult<Project>> GetProject([FromBody]GetProjectRequest request)
        {
            var pro = await IProjectService.GetProjectsByName(request.ProjectName);
            return Ok(pro);
        }
        [HttpGet("get_image")]
        public async Task<ActionResult<ImageDtos>> getImage(int project_id)
        {
            try
            {
                var images = await IProjectService.GetImagesAsync(project_id);
                return Ok(images);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("get_project_by_id")]
        public async Task<ActionResult<Project>> GetProjectByID([FromBody] GetProjectRequest request)
        {
            var pro = await IProjectService.GetProject(request.ProjectId);
            return Ok(pro);
        }
        [HttpDelete("{name}"), Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(string name)
        {
            await IProjectService.DeleteProject(name);
            return Ok(new { message = $"Xóa dự án thành công!" });
        }
        [HttpPost("add_project"), Authorize(Roles = "Manager")]
        public async Task<IActionResult> AddProject(CreateProjectRequest createProject)
        {
            try
            {
                await IProjectService.AddProject(createProject);
                return Ok(new { message = "Tạo thành công dự án mới !" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error Created project: {ex.Message}");
            };
        }
        [HttpPut]
        public async Task<ActionResult> UpdateProject(string name, UpdateProjectRequest up)
        {
            try
            {
                await IProjectService.UpdateProject(name, up);
                return Ok($"Project '{name}' updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating project: {ex.Message}");
            };
        }
        [HttpPost("update_status"), Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateStatus(updateStatusRequest request)
        {
            try
            {
                await IProjectService.UpdateStatus(request);
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

    }
}
