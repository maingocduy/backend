using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS;
using MySqlX.XDevAPI.Common;
using WebApplication3.DTOs.Account;
using WebApplication3.DTOs.Member;
using WebApplication3.DTOs.Otp;
using WebApplication3.DTOs.Project;
using WebApplication3.Entities;
using WebApplication3.repository.AccountRepository;
using WebApplication3.Service.AccountService;
using WebApplication3.Service.MemberService;
using WebApplication3.Service.ProjectService;
using WebApplication3.Service.SponsorService;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService IMemberService;
        public MemberController(IMemberService IMemberService)
        {
            this.IMemberService = IMemberService;
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateRequestMemberDTO mem)
        {
            await IMemberService.AddMember(mem.Project_id, mem);
            return Ok(new { message = "Thành công" });
        }
        [HttpPost("get_all_member")]
        public async Task<ActionResult<List<MemberDTO>>> GetAllMember(GetlAllMemberRequest request)
        {
            var mem = await IMemberService.GetAllMember(request.pageNumber, request.ProjectId,request.groupName);
            return Ok(new { mems = mem.Data, totalPages = mem.TotalPages });
        }
        [HttpGet("name")]
        public async Task<ActionResult<Member>> GetMember(string name)
        {
            var mem = await IMemberService.GetMember(name);
            return Ok(mem);
        }
        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            await IMemberService.DeleteMember(name);
            return Ok(new { message = "Member deleted" });
        }
        [HttpPost("update_member")]
        public async Task<ActionResult> UpdateMember(UpdateRequestMember up)
        {
            try
            {
                await IMemberService.UpdateMember(up);
                return Ok(new { messenger = $"Cập nhật thành công !" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating member: {ex.Message}");
            };
        }

        [HttpPost("JoinProject"),Authorize]
        public async Task<ActionResult> JoinProject(JoinProjectRequest request)
        {
            try
            {
                await IMemberService.JoinProject(request.Project_id, request.username);
                return Ok(new {messenger =  $"Đăng ký tham gia thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { messenger = ex.Message });
            };
        }
        [HttpPost("re_send_otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ForgetPassDTO request)
        {
            try
            {
                await IMemberService.ReSendOtp(request.email);
                return Ok(new { Message = "Gửi otp mới thành công." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Đã xảy ra lỗi. Vui lòng thử lại sau." });
            }
        }
        [HttpPost("enter_otp")]
        public async Task<IActionResult> EnterOtp([FromBody] EnterOtpMemberRequest request)
        {
            try
            {
                await IMemberService.EnterOtp(request.Otp,request.Project_id, request.Email);
                return Ok(new { messenger = "Tạo thành Công"});
            }
            catch (Exception ex)
            {
                return BadRequest(new { messenger = ex.Message});
            }
        }
       
    }
}
