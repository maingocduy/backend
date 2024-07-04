using AutoMapper;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Newtonsoft.Json.Linq;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using WebApplication3.DTOs.Account;
using WebApplication3.DTOs.Auth;
using WebApplication3.DTOs.Member;
using WebApplication3.DTOs.Otp;
using WebApplication3.Helper.Data;
using WebApplication3.repository.AccountRepository;
using WebApplication3.repository.MemberRepository;
using static WebApplication3.DTOs.Auth.ServiceResponses;
using MailKit.Net.Smtp;
using MimeKit;
using WebApplication3.Entities;
using WebApplication3.DTOs;

namespace WebApplication3.Service.AccountService
{
    public interface IAccountService
    {
        Task<PagedResult<AccountDTO>> GetAllAcc(int pageNumber, string keyword = null);
        Task<AccountDTO> GetAccountsAsync(int id);

        Task<AccountDTO> GetAccountsByUserName(string username);

        Task UpdatePasswordAcc(UpdatePasswordRequestDTO acc);
        Task DeleteAccount(string username);

        Task ForgotPassword(string email);

        Task EnterOtp(string otp);
        Task SendEmailAsync(string email, string subject, string messager);

        Task ChangeForgetPass(string email, string newPass, string otp);
        Task ReSendOtp(string email);

        Task ChangeRole(string username);
    }
    public class AccountService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,IAccountRepository _AccountRepository, IMapper _mapper) : IAccountService
    {
        public async Task DeleteAccount(string username)
        {

            var acc = await _AccountRepository.GetAccountsByUserName(username);
            var accIden = await userManager.FindByNameAsync(username);
            if (acc != null && accIden != null)
            {
                await userManager.DeleteAsync(accIden);
                await _AccountRepository.DeleteAccount(acc);
            }
            else
            {
                throw new KeyNotFoundException("không tìm thấy tài khoản !");
            }
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mail = "2051063514@e.tlu.edu.vn";
            var pw = "Mangcut11";

            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Hội bác sĩ tình nguyện", mail)); // Chú ý thay thế "Your Name" bằng tên hiển thị mong muốn
            mimeMessage.To.Add(new MailboxAddress("Recipient", email)); // Chú ý thay thế "Recipient" bằng tên hiển thị mong muốn
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart("plain")
            {
                Text = message
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync("smtp-mail.outlook.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(mail, pw);
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
            }
        }

        public async Task<AccountDTO> GetAccountsAsync(int id)
        {
            var Account = await _AccountRepository.GetAccounts(id);

            if (Account == null)
                throw new KeyNotFoundException("Account not found");

            return Account;
        }

        public async Task<AccountDTO> GetAccountsByUserName(string username)
        {
            var Account = await _AccountRepository.GetAccountsByUserName(username);

            if (Account == null)
                throw new KeyNotFoundException("Account not found");

            return Account;
        }

        public async Task<PagedResult<AccountDTO>> GetAllAcc(int pageNumber, string keyword = null)
        {
            // Sử dụng _AccountRepository để lấy danh sách tài khoản phân trang
            var pagedResult = await _AccountRepository.GetAllAccounts(pageNumber,keyword);
            var lstacc = pagedResult.Data;

            // Tạo danh sách để lưu trữ các tác vụ đợi hoàn thành
            var tasks = new List<Task>();

            // Duyệt qua từng tài khoản để tìm và cập nhật thông tin
            foreach (var acc in lstacc)
            {
                // Tạo task để tìm người dùng theo tên
                var findUserTask = userManager.FindByNameAsync(acc.Username);
                tasks.Add(findUserTask); // Thêm task vào danh sách tác vụ

                // Tìm người dùng
                var user = await findUserTask;
                if (user != null)
                {
                    // Cập nhật vai trò dựa trên vai trò của người dùng
                    if (await userManager.IsInRoleAsync(user, "Admin"))
                    {
                        acc.Role = "Admin";
                    }
                    else if (await userManager.IsInRoleAsync(user, "User"))
                    {
                        acc.Role = "User";
                    }
                    else if (await userManager.IsInRoleAsync(user, "Manager"))
                    {
                        acc.Role = "Manager";
                    }

                    // Cập nhật trạng thái dựa trên EmailConfirmed của người dùng
                    acc.Status = (sbyte)(user.EmailConfirmed ? 1 : 0);
                }
            }

            // Đợi cho tất cả các tác vụ tìm người dùng hoàn thành
            await Task.WhenAll(tasks);

            // Trả về kết quả phân trang với các thông tin đã được cập nhật
            return new PagedResult<AccountDTO>
            {
                Data = lstacc,
                TotalPages = pagedResult.TotalPages
            };
        }




        public async Task UpdatePasswordAcc(UpdatePasswordRequestDTO acc)
        {
            var user = await userManager.FindByNameAsync(acc.username);

            if (user == null)
                throw new KeyNotFoundException("User not found");
            if(!await userManager.CheckPasswordAsync(user, acc.OldPassword))
            {
                throw new Exception("Mật khẩu cũ sai");
            }
            // copy model props to user
            var account = await _AccountRepository.GetAccountsByUserName(acc.username);
            
            var a = await userManager.ChangePasswordAsync(user, acc.OldPassword, acc.Password);
            acc.Password = user.PasswordHash;
            _mapper.Map(acc, account);
            // save user
            _AccountRepository.UpdatePasswordAcc(account);
        }

        public async Task ForgotPassword(string email)
        {
            var acc = await _AccountRepository.GetAccountByEmail(email);
            if (acc == null)
            {
                throw new KeyNotFoundException("Email không tồn tại");
            }

            var otp = GenerateOTP();
             _AccountRepository.SaveOtp(otp, email);

              SendEmailAsync(email, "Lấy lại mật khẩu", $"Để lấy lại mật khẩu vui lòng dùng OTP này: {otp}");
        }
        public async Task EnterOtp(string otp)
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Mã này tương ứng với GMT+7
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            var OTP = await _AccountRepository.GetOtp(otp);
            if (OTP == null){
                throw new KeyNotFoundException("Otp bị sai, mời nhập lại");
             }
            else if (OTP.expires_at < vnTime) { 
                throw new Exception("OTP đã hết hạn"); 
            }
            else if (OTP.IsVerified == true)
            {
                throw new Exception("OTP đã được xác nhận");
            }

        }
        public async Task ReSendOtp(string email)
        {
            var lstOtp = await _AccountRepository.GetOtpByEmail(email);
            if(lstOtp == null)
            {
                throw new KeyNotFoundException("Không tìm thấy Otp");
            }
            foreach(var otp in lstOtp)
            {
                otp.IsVerified = true;
                _AccountRepository.UpdateOtp(otp);
            }
             ForgotPassword(email);
        }
        private static string GenerateOTP()
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var bytes = new byte[4]; // Tạo mảng byte đủ để lấy số ngẫu nhiên
                rng.GetBytes(bytes);
                var num = BitConverter.ToUInt32(bytes, 0);
                return (num % 1000000).ToString("D6"); // Đảm bảo mã OTP là 6 chữ số
            }
        }
        private async Task saveOtp(string otp,string email)
        {
            await _AccountRepository.SaveOtp(otp, email);
        }
        public async Task ChangeForgetPass(string email, string newPass, string otp)
        {
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Mã này tương ứng với GMT+7
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            var OTP = await _AccountRepository.GetOtp(otp);

            if (OTP == null)
            {
                throw new KeyNotFoundException("Otp bị sai, mời nhập lại");
            }
            else if (OTP.expires_at < vnTime)
            {
                throw new Exception("OTP đã hết hạn");
            }
            else if (OTP.IsVerified == true)
            {
                throw new Exception("OTP đã được xác nhận");
            }
            else
            {
                var getUserIdentity = await userManager.FindByEmailAsync(email);

                if (getUserIdentity == null)
                {
                    throw new KeyNotFoundException("Email này không tồn tại");
                }

                if (string.IsNullOrEmpty(newPass))
                {
                    throw new Exception("Nhập thiếu mật khẩu");
                }

                // Generate password reset token
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(getUserIdentity);

                // Reset the password using the reset token
                var result = await userManager.ResetPasswordAsync(getUserIdentity, resetToken, newPass);
                
                if (result.Succeeded)
                {
                    
                    OTP.IsVerified = true;
                    _AccountRepository.UpdateOtp(OTP);
                    _AccountRepository.UpdatePasswordAccByEmail(email, getUserIdentity.PasswordHash);
                }
                else
                {
                    throw new Exception("Đặt lại mật khẩu không thành công: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }


        public async Task ChangeRole(string username)
        {
            // Tìm người dùng bằng tên người dùng
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                throw new KeyNotFoundException("Tài khoản không tồn tại");
            }

            // Kiểm tra xem người dùng có quyền "User" hay không
            if (await userManager.IsInRoleAsync(user, "User"))
            {
                // Xóa quyền "User"
                var removeResult = await userManager.RemoveFromRoleAsync(user, "User");
                if (!removeResult.Succeeded)
                {
                    throw new Exception("Failed to remove user role");
                }

                // Thêm quyền "Admin"
                var addResult = await userManager.AddToRoleAsync(user, "Manager");
                if (!addResult.Succeeded)
                {
                    throw new Exception("Failed to add Manager role");
                }
            }
            else if (await userManager.IsInRoleAsync(user, "Manager"))
            {
                // Xóa quyền "Admin"
                var removeResult = await userManager.RemoveFromRoleAsync(user, "Manager");
                if (!removeResult.Succeeded)
                {
                    throw new Exception("Failed to remove Manager role");
                }

                // Thêm quyền "User"
                var addResult = await userManager.AddToRoleAsync(user, "User");
                if (!addResult.Succeeded)
                {
                    throw new Exception("Failed to add user role");
                }
            }
        }

    }
}
   