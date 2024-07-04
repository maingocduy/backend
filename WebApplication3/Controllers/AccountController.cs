using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.DTOs.Account;
using WebApplication3.DTOs.Auth;
using WebApplication3.DTOs.Otp;
using WebApplication3.Entities;
using WebApplication3.Helper.Data;
using WebApplication3.Service.AccountService;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AccountController : Controller
    {
        private readonly IAccountService accountService;
        public AccountController(IAccountService accountService)
        {
            this.accountService = accountService;
        }
        [HttpGet, Authorize(Roles ="Admin")]
        [HttpPost("get_all_accounts")]
        public async Task<IActionResult> GetAllAccounts(GetAllAccountRequest request)
        {
            try
            {
                var result = await accountService.GetAllAcc(request.pageNumber,request.keyword);
                return Ok(new { accounts = result.Data, totalPages = result.TotalPages });
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("update_role"), Authorize(Roles = "Admin")]
        public async Task<ActionResult> updateRole(string username)
        {
            try
            {
                await accountService.ChangeRole(username);
                return Ok(new { message  = "Đổi quyền thành công"});
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
        [HttpGet("id")]
        public async Task<ActionResult<account>> GetAccount(int id)
        {
            var account = await accountService.GetAccountsAsync(id);
            return Ok(account);
        }

        [HttpGet("Username"),Authorize]
        public async Task<ActionResult<account>> GetAccountByUsername(string username)
        {
            var account = await accountService.GetAccountsByUserName(username);
            return Ok(account);
        }
   

        [HttpDelete("delete_acc"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string username)
        {
            try
            {
                await accountService.DeleteAccount(username);
                return Ok(new { message = "Xóa thành công" });
                    }
            catch(KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống" });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(string token)
        {
           

            // Trả về một kết quả thành công hoặc thông tin khác tùy thuộc vào logic của bạn
            return Ok("Logout");
        }
        [HttpPost("forgot")]
        public async Task<IActionResult> ForgotPass([FromBody]  ForgetPassDTO forgetPass)
        {
            try
            {
                await accountService.ForgotPassword(forgetPass.email);
                return Ok(new { Message = "Gửi thành công." });
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
        public async Task<IActionResult> EnterOtp([FromBody]  otpRequest request)
        {
            try
            {
                await accountService.EnterOtp(request.Otp);
                return Ok("Thành công");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message =ex.Message });
            }
        }
        [HttpPost("re_send_otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ForgetPassDTO request)
        {
            try
            {
                await accountService.ReSendOtp(request.email);
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
        [HttpPost("changeForgetPass")]
        public async Task<IActionResult> ChangeForgetPass(EnterPassRequest request)
        {
            try
            {
                await accountService.ChangeForgetPass(request.Email, request.Password, request.Otp);
                return Ok(new { Message = "Đặt lại mật khẩu thành công" });
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
        [HttpPost("change_pass"), Authorize]
        public async Task<IActionResult> ChangePass(UpdatePasswordRequestDTO request)
        {
            try
            {
                await accountService.UpdatePasswordAcc(request);
                return Ok(new { Message = "Đặt lại mật khẩu thành công" });
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
