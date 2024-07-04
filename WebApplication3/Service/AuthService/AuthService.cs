using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using WebApplication3.DTOs.Account;
using WebApplication3.DTOs.Auth;
using WebApplication3.DTOs.Member;
using WebApplication3.Helper;
using WebApplication3.Helper.Data;
using WebApplication3.repository.AccountRepository;
using WebApplication3.repository.AuthRepository;
using WebApplication3.repository.MemberRepository;
using WebApplication3.Service.AccountService;
using static WebApplication3.DTOs.Auth.ServiceResponses;

namespace WebApplication3.Service.AuthService
{
    public interface IAuthService
    {
        Task<LoginResponse> login(LoginDTO login);
        Task<GeneralResponse> RegisterNewAccount(CreateAccountRequestDTO registerDTO);
        Task<string> GenerateJwtTokenFromRefreshToken();
        Task<bool> Logout();
        Task<GeneralResponse> ConfirmEmailAsync(string userId, string code);
    }
    public class AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration _config,IAccountRepository accountRepository, IAuthRepository AuthRepository, IMemberRepository memberRepository, IMapper _mapper, IHttpContextAccessor httpContextAccessor, WhitelistStorage _whitelistStorage ,IAccountService accountService
) : IAuthService
    {
        public async Task<LoginResponse> login(LoginDTO login)
        {
            if (string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
                return new LoginResponse(false, null!, null!, "Nhập thiếu thông tin đăng nhập",null,null);

            var getUser = await userManager.FindByNameAsync(login.Username);
            if (getUser is null)
                return new LoginResponse(false, null!, null!, "Không tìm thấy tài khoản", null, null);

            bool checkUserPasswords = await userManager.CheckPasswordAsync(getUser, login.Password);
            if (!checkUserPasswords)
                return new LoginResponse(false, null!, null!, "Sai mật khẩu", null, null);
            bool checkConfirmEmail = await userManager.IsEmailConfirmedAsync(getUser);
            if (!checkConfirmEmail)
            {
                return new LoginResponse(false, null!, null!, "Email chưa được xác thực", null, null);
            }
            var getUserRole = await userManager.GetRolesAsync(getUser);
            var userSession = new UserSession(getUser.Id, getUser.UserName, getUserRole.First());

            // Generate JWT token
            var tokens = TokenUtils.GenerateJwtToken(userSession,_config);
            var rtoken = TokenUtils.GenerateRefreshToken(userSession, _config);
            
            return new LoginResponse(true, tokens!, rtoken!, "Đăng nhập thành công",userSession.Username,userSession.Role);
        }
        public async Task<GeneralResponse> RegisterNewAccount(CreateAccountRequestDTO registerDTO)
        {
                var user = _mapper.Map<MemberDTO>(registerDTO);
                // 1. Thêm một bản ghi mới vào bảng Members
                if(await memberRepository.GetMemberByEmail(registerDTO.Email) != null)
            {
                throw new Exception("Email đã đăng ký tài khoản. Vui lòng thử lại!");
            }
            if (await accountRepository.GetAccountsByUserName(registerDTO.username) != null)
            {
                throw new Exception("Tên đăng nhập đã có!");
            }
            var memberId = await memberRepository.AddNewMember(user);
                
                var member = await memberRepository.GetMemberById(memberId);

                // 2. Thêm một bản ghi mới vào bảng Account
                  await AddAcount(registerDTO, memberId,member.email);
            var userByUsername = await userManager.FindByNameAsync(registerDTO.username);
            string code = await userManager.GenerateEmailConfirmationTokenAsync(userByUsername);
            string confirmationLink = $"http://localhost:5173/ResponseRegister?userId={userByUsername.Id}&code={Uri.EscapeDataString(code)}&user={userByUsername.UserName}";
             accountService.SendEmailAsync(userByUsername.Email, "Xác nhận email của bạn",
        $"Vui lòng xác nhận email của bạn bằng cách nhấp vào liên kết này: <a href='{confirmationLink}'>link</a>");
            return new GeneralResponse(true, "Tài khoản đã được tạo thành công. Vui lòng kiểm tra email để xác nhận tài khoản của bạn.");
        }
        private async Task<GeneralResponse> AddAcount(CreateAccountRequestDTO acc, int MemberId,string email)
        {
            // Check if the username already exists
            if (await accountRepository.GetAccountsByUserName(acc.username) != null)
                throw new Exception("Username này đã tồn tại");
            var getUser = await userManager.FindByEmailAsync(email);
            if (getUser != null)
            {
                throw new Exception("Email này đã được đăng ký");
            }
            // Map model to new user object
            var user = _mapper.Map<AccountDTO>(acc);



            // Save user
       

            // Create the ApplicationUser object
            var newUser = new ApplicationUser()
            {
                UserName = acc.username,
                PasswordHash = user.Password,
                Email =email,
                EmailConfirmed = false
            };

            // Check if the user with the same Username already exists
            var userByUsername = await userManager.FindByNameAsync(newUser.UserName);
            if (userByUsername != null)
                return new GeneralResponse(false, "User already registered");

            // Create the user
            var createUser = await userManager.CreateAsync(newUser, user.Password);
            user.Password = newUser.PasswordHash;
            await AuthRepository.AddAcount(user, MemberId);
            if (!createUser.Succeeded)
                return new GeneralResponse(false, "Error occurred.. please try again");
            else
            {
             
                // Assign Default Role: Admin to the first registrar; rest are Users
                var checkAdmin = await roleManager.FindByNameAsync("Manager");
                if (checkAdmin == null)
                {
                    await roleManager.CreateAsync(new IdentityRole() { Name = "Manager" });
                    await userManager.AddToRoleAsync(newUser, "Manager");
                    return new GeneralResponse(true, "Tài khoản đã được tạo");
                }
                else
                {
                    var checkUser = await roleManager.FindByNameAsync("User");
                    if (checkUser == null)
                        await roleManager.CreateAsync(new IdentityRole() { Name = "User" });

                    await userManager.AddToRoleAsync(newUser, "User");

                    return new GeneralResponse(true, "Tài khoản đã được tạo");
                }
            }
        }
        public async Task<string> GenerateJwtTokenFromRefreshToken()
        {
            var refreshToken = httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                ValidateLifetime = false // Không kiểm tra hết hạn ở đây, vì đây là refresh token
            };

            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out validatedToken);

            // Trích xuất thông tin từ principal
            var claimsIdentity = principal.Identity as ClaimsIdentity;

            // Lấy ra claim id
            var userIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new InvalidOperationException("User Id claim not found in the refresh token.");
            }
            var name = await userManager.FindByIdAsync(userIdClaim.Value);
            var roles = await userManager.GetRolesAsync(name);
            var userRole = roles.First();
            // Gọi hàm GenerateJwtTokenFromRefreshToken từ service
            var jwtToken = TokenUtils.GenerateJwtTokenFromRefreshToken(refreshToken, name.UserName, userRole, _config);

            return jwtToken;
        }
        public async Task<bool> Logout()
        {
            var jwt = httpContextAccessor.HttpContext.Request.Cookies["jwtToken"];
            if (!string.IsNullOrEmpty(jwt) && _whitelistStorage.Whitelist.ContainsKey(jwt))
            {
                // JWT có trong whitelist
                // Kiểm tra xem JWT còn hạn hay không
                if (_whitelistStorage.Whitelist[jwt] > DateTime.UtcNow)
                {
                    // JWT còn hạn
                    // Tiến hành loại bỏ JWT khỏi whitelist
                    _whitelistStorage.Whitelist.Remove(jwt);

                    // Thực hiện các bước logout cần thiết
                    // Ví dụ: Invalidate token, xóa thông tin session, vv.

                    return true; // Logout thành công
                }
            }

            return false; // Logout không thành công
        }
        public async Task<GeneralResponse> ConfirmEmailAsync(string userId, string code)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
            {
                return new GeneralResponse(false, "UserId và Code là bắt buộc");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new GeneralResponse(false, "Không tìm thấy người dùng");
            }

            var result = await userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {

                return new GeneralResponse(true, "Xác nhận email thành công");
            }
            else
            {
                return new GeneralResponse(false, "Lỗi xác nhận email");
            }
        }


    }
}
