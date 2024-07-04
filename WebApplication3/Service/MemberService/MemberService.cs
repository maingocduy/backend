using AutoMapper;
using AutoMapper.Execution;
using CloudinaryDotNet.Core;
using K4os.Compression.LZ4.Internal;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using System.Xml.Linq;
using WebApplication3.DTOs;
using WebApplication3.DTOs.Member;
using WebApplication3.DTOs.Sponsor;
using WebApplication3.Entities;
using WebApplication3.repository.GroupsRepository;
using WebApplication3.repository.MemberRepository;
using WebApplication3.repository.ProjectReposiotry;
using WebApplication3.repository.SponsorRepository;
using WebApplication3.Service.AccountService;
using static System.Net.WebRequestMethods;

namespace WebApplication3.Service.MemberService
{
    public interface IMemberService
    {
        Task<PagedResult<MemberDTO>> GetAllMember(int pageNumber, int? ProjectId = null, string? groupName = null);
        Task<MemberDTO> GetMemberAsync(int id);
        Task ReSendOtp(string email);
        Task<MemberDTO> GetMember(string member);
        Task AddMember(int project_id, CreateRequestMemberDTO acc);
        Task UpdateMember(UpdateRequestMember update);
        Task DeleteMember(string member);

        Task JoinProject(int project_id, string username);

        Task EnterOtp(string otp, int project_id, string email);
    }
    public class MemberService : IMemberService
    {
        private readonly IMemberRepository IMemberRepository;
        private readonly IMapper _mapper;
        private readonly IProjectRepository projectRepository;
        private readonly IGroupsRepository groupsRepository;
        public MemberService(IMemberRepository IMemberRepository,IProjectRepository projectRepository, IGroupsRepository groupsRepository,IAccountService accountService, IMapper mapper)
        {
            this.IMemberRepository = IMemberRepository;
            _mapper = mapper;
            this.projectRepository = projectRepository;

        }
        public async Task AddMember(int project_id, CreateRequestMemberDTO mem)
        {
            // Kiểm tra xem dự án có tồn tại không
            var project = await projectRepository.GetProjectByID(project_id);
            var member = await IMemberRepository.GetMemberByEmail(mem.Email);
            if (project == null)
            {
                throw new KeyNotFoundException("Không tìm thấy dự án !");
            }
            var check = await IMemberRepository.CheckIsVerified(mem.Email);
            if (!check && member != null)
            {
                var otp = GenerateOTP();
                IMemberRepository.SaveOtp(otp, mem.Email);

                SendEmailAsync(mem.Email, "Xác Nhận Email", $"Để xác nhận Email vui lòng dùng OTP này: {otp}");
            }
            else if (member != null)
            {
                await IMemberRepository.UpdateRole(mem.Email, mem.Group_name);
                var otp = GenerateOTP();
                IMemberRepository.SaveOtp(otp, mem.Email);

                SendEmailAsync(mem.Email, "Xác Nhận Tham gia dự án", $"Để xác nhận tham gia dự án với vai trò là {mem.Group_name} vui lòng dùng OTP này: {otp}");
            }
            else if (await IMemberRepository.CheckIsInProject(member.Member_id, project.Project_id))
            {
                throw new Exception("Email này đã đăng ký tham gia dự án này");
            }
            else
            {
                var memberDTO = _mapper.Map<MemberDTO>(mem);
                var otp = GenerateOTP();
                await IMemberRepository.AddMember(project.Project_id, memberDTO);
                IMemberRepository.SaveOtp(otp, memberDTO.email);

                SendEmailAsync(memberDTO.email, "Xác Nhận Email", $"Để xác nhận Email vui lòng dùng OTP này: {otp}");

            };
            
        }
        public async Task ReSendOtp(string email)
        {
            var lstOtp = await IMemberRepository.GetOtpByEmail(email);
            if (lstOtp == null)
            {
                throw new KeyNotFoundException("Không tìm thấy Otp");
            }
            foreach (var otp in lstOtp)
            {
                otp.IsVerified = true;
                IMemberRepository.UpdateOtp(otp);
            }
            var otps = GenerateOTP();
            IMemberRepository.SaveOtp(otps, email);

             SendEmailAsync(email, "Xác Nhận Email", $"Để xác nhận Email vui lòng dùng OTP này: {otps}");
        }
        public async Task EnterOtp(string otp, int Project_id, string email)
        {
            try
            {
                var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Mã này tương ứng với GMT+7
                var vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

                // Lấy thông tin mã OTP từ cơ sở dữ liệu
                var OTP = await IMemberRepository.GetOtp(otp);

                if (OTP == null)
                {
                    throw new KeyNotFoundException("Otp bị sai, mời nhập lại");
                }
                else if (OTP.expires_at < vnTime)
                {
                    throw new Exception("OTP đã hết hạn");
                }
                else if (OTP.IsVerified)
                {
                    throw new Exception("OTP đã được xác nhận");
                }
                else
                {
                    // Xác nhận mã OTP và cập nhật trạng thái
                    OTP.IsVerified = true;
                    IMemberRepository.UpdateOtp(OTP);

                    // Thêm thành viên vào dự án
                    var member = await IMemberRepository.GetMemberByEmail(email);
                    await IMemberRepository.JoinProject(Project_id, member.Member_id);
                    
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
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
        public async Task DeleteMember(string name)
        {
          
            await IMemberRepository.DeleteMember(name);
            
        }

        public async Task<PagedResult<MemberDTO>> GetAllMember(int pageNumber,int? ProjectId = null, string? groupName = null)
        {
            return await IMemberRepository.GetAllMember(pageNumber,ProjectId, groupName);
        }

        public async Task<MemberDTO> GetMember(string member)
        {
            var group = await IMemberRepository.GetMember(member);

            if (group == null)
                throw new KeyNotFoundException("member not found");

            return group;
        }

        public Task<MemberDTO> GetMemberAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task JoinProject(int project_id, string username)
        {
           

            var project = await projectRepository.GetProjectByID(project_id);

            if (project == null)
            {
                throw new Exception($"Member or project does not exist");
            }
            var member_id = await IMemberRepository.GetMemberIDByUsername(username);
            if (await IMemberRepository.CheckIsInProject(member_id, project.Project_id))
            {
                throw new Exception("Tài khoản này đã được tham gia dự án này");
            }
            else
            {
                await IMemberRepository.JoinProject(project.Project_id, member_id);
            }
        }

        public async Task UpdateMember(UpdateRequestMember update)
        {
            // Check if the new email is already taken by another member
            var existingMemberByEmail = await IMemberRepository.GetMemberByEmail(update.Email);
            if (existingMemberByEmail == null )
            {
                throw new Exception($"Không tìm thấy thành viên!");
            }
            // Update member information
            existingMemberByEmail.name = update.Name;
            existingMemberByEmail.phone = update.Phone;
            


            // Retrieve the group_id based on the provided group_name
        
            // Call repository to update member information
            await IMemberRepository.UpdateMember(existingMemberByEmail, update.Group_name );
        }


    }
}
