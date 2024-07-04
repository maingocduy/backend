using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApplication3.DTOs.Auth;
using WebApplication3.Helper.Data;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using Microsoft.AspNetCore.Identity;

namespace WebApplication3.Helper
{
    public static class TokenUtils
    {
        
        public static string GenerateJwtToken(UserSession user, IConfiguration _config)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role)
    };
            // Tạo JWT
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: userClaims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials
            );

            // Tạo refresh token
            return (new JwtSecurityTokenHandler().WriteToken(token));
        }


        public static string GenerateRefreshToken(UserSession userSession, IConfiguration _config)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userSession.Id)
    };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7), // Đặt thời gian hết hạn cho refresh token
                SigningCredentials = credentials,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var refreshToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(refreshToken);
        }


        public static string GenerateJwtTokenFromRefreshToken(string refreshToken, string name, string role, IConfiguration _config)
        {
            // Giải mã refresh token để lấy thông tin
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

            // Tạo mới các claim cho JWT mới và thêm hai claim name và role
            var newClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, name),
        new Claim(ClaimTypes.Role, role)
    };

            // Thêm các claim từ refresh token vào danh sách mới
            newClaims.AddRange(claimsIdentity.Claims);

            // Tạo JWT mới
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(newClaims),
                Expires = DateTime.UtcNow.AddDays(3), // Thời gian hết hạn của JWT mới
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var newToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(newToken);
        }



    }
}
