using Dapper;
using Microsoft.EntityFrameworkCore;
using WebApplication3.DTOs.Account;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using WebApplication3.Entities;
using WebApplication3.DTOs.Groups;
using WebApplication3.DTOs.Member;
using WebApplication3.Helper.Data;
using WebApplication3.DTOs.Otp;
using Org.BouncyCastle.Crypto;
using CloudinaryDotNet;
using System.Xml.Linq;
using WebApplication3.DTOs;
using System.Data.Common;
using System.Text;

namespace WebApplication3.repository.AccountRepository
{
    public interface IAccountRepository
    {
        Task<PagedResult<AccountDTO>> GetAllAccounts(int pageNumber, string keyword = null);
        Task<AccountDTO> GetAccounts(int id);
        Task<AccountDTO> GetAccountsByUserName(string username);
        Task UpdatePasswordAcc(AccountDTO acc);
        Task DeleteAccount(AccountDTO acc);
        Task<int> getIDAcount(string name);
        Task<OtpDTO> GetOtp(string otp);

        Task UpdateOtp(OtpDTO otp);
        Task<Account> GetAccountByEmail(string email);
        Task SaveOtp(string otp, string emailAccount);


        Task UpdatePasswordAccByEmail(string email, string newPassword);

        Task<List<OtpDTO>> GetOtpByEmail(string email);
    }
    public class AccountRepository : IAccountRepository
    {
        private AppDbContext _context;
        public AccountRepository(AppDbContext context) {
            _context = context;
        }

        public async Task DeleteAccount(AccountDTO acc)
        {
            using var connection = _context.CreateConnection();


            var sqlBlog = @"
        DELETE FROM Blog 
        WHERE Account_id = (select Account_id from account where Username = @name)
    ";
            await connection.ExecuteAsync(sqlBlog, new { name = acc.Username });

            // Xóa tài khoản từ bảng account
            var sqlAccount = @"
        DELETE FROM account 
        WHERE Username = @username
    ";
            await connection.ExecuteAsync(sqlAccount, new { username = acc.Username });

            var deleteMemberProjectsSql = @"
DELETE FROM MemberProjects
WHERE Member_id IN (
    SELECT Member_id FROM account 
        WHERE Username = @username
);";

            // Thực hiện câu lệnh SQL xóa MemberProjects
            await connection.ExecuteAsync(deleteMemberProjectsSql, new { username = acc.Username });
            // Xóa thành viên từ bảng Members
            var sqlMember = @"
        DELETE FROM Members 
        WHERE Member_id = @member_id
    ";
            await connection.ExecuteAsync(sqlMember, new { member_id = acc.Member.Member_id });


        }


        public async Task<AccountDTO> GetAccounts(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = """
            SELECT a.*, m.*,g.*
        			FROM account AS a
        			JOIN Members AS m ON a.Member_id = m.Member_id
                    JOIN `Groups` AS g ON m.Group_id = g.Group_id 
            WHERE Account_id = @id
        """;
            var acc = await connection.QueryAsync<AccountDTO, MemberDTO, Group, AccountDTO>(
        sql,
        (account, member, group) =>
        {
            account.Member = member;
            member.groups = group; // Assuming 'MemberDTO' has a property 'groups' of type 'GroupsDTOs'
            return account; // Return the account with its member populated
        },
        new { id },
        splitOn: "Member_id,Group_id"
    );
            return acc.FirstOrDefault();
        }
      
        public async Task<AccountDTO> GetAccountsByUserName(string username)
        {
            using var connection = _context.CreateConnection();
            var sql = """
            SELECT a.*, m.*,g.*
        			FROM account AS a
        			JOIN Members AS m ON a.Member_id = m.Member_id
                    JOIN `Groups` AS g ON m.Group_id = g.Group_id 
            WHERE Username = @username
        """;
            var acc= await connection.QueryAsync<AccountDTO, MemberDTO, Group, AccountDTO>(
       sql,
       (account, member, group) =>
       {
           account.Member = member;
           member.groups = group; // Assuming 'MemberDTO' has a property 'groups' of type 'GroupsDTOs'
           return account; // Return the account with its member populated
       },
       new { username },
       splitOn: "Member_id,Group_id"
   );
            return acc.FirstOrDefault();
        }

        public async Task<PagedResult<AccountDTO>> GetAllAccounts(int pageNumber, string keyword = null)
        {
            using var connection = _context.CreateConnection();
            int pageSize = 6;
            var offset = (pageNumber - 1) * pageSize;

            // Xây dựng câu lệnh SQL cơ bản
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(@"
        SELECT a.*, m.*, g.*
        FROM account AS a
        JOIN Members AS m ON a.Member_id = m.Member_id
        JOIN `groups` AS g ON m.Group_id = g.Group_id
    ");

            // Nếu có từ khóa tìm kiếm, thêm điều kiện WHERE
            if (!string.IsNullOrEmpty(keyword))
            {
                sqlBuilder.Append("WHERE a.username LIKE @keyword ");
            }

            sqlBuilder.Append("LIMIT @pageSize OFFSET @offset;");

            // Thực hiện truy vấn
            var queryResult = await connection.QueryAsync<AccountDTO, MemberDTO, Group, AccountDTO>(
                sqlBuilder.ToString(),
                (account, member, group) =>
                {
                    account.Member = member;
                    member.groups = group;
                    return account;
                },
                new { keyword = $"%{keyword}%", pageSize, offset },
                splitOn: "Member_id,Group_id"
            );

            // Tạo câu truy vấn để tính tổng số lượng bản ghi
            var countSqlBuilder = new StringBuilder();
            countSqlBuilder.Append("SELECT COUNT(*) FROM account");

            // Nếu có từ khóa tìm kiếm, thêm điều kiện WHERE
            if (!string.IsNullOrEmpty(keyword))
            {
                countSqlBuilder.Append(" WHERE username LIKE @keyword");
            }

            // Thực hiện truy vấn để lấy tổng số bản ghi
            var totalCount = await connection.ExecuteScalarAsync<int>(countSqlBuilder.ToString(), new { keyword = $"%{keyword}%" });

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResult<AccountDTO>
            {
                Data = queryResult.ToList(),
                TotalPages = totalPages
            };
        }


        public async Task UpdatePasswordAcc(AccountDTO acc)
        {
            using var connection = _context.CreateConnection();
            var sql = """
            UPDATE account 
            SET Password = @Password
            WHERE Username = @Username
        """;

            await connection.ExecuteAsync(sql, acc);
        }
        public async Task<int> getIDAcount(string name)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
            SELECT a.Account_id
            FROM Account AS a
            WHERE Username = @name";

            var memberId = await connection.QueryFirstOrDefaultAsync<int>(sql, new { name });

            return memberId;
        }
        public async Task SaveOtp(string otp,string emailAccount)
        {
            using var connection = _context.CreateConnection();
            var sql = """
            INSERT INTO otp_table (otp_code, created_at,expires_at,Account_id)
            VALUES (@otp, @created_at,@expires_at,(Select Account_id from Account where Member_id = (Select Member_id from Members where email = @email)))
        """;
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Mã này tương ứng với GMT+7
            var vnTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);
            await connection.ExecuteAsync(sql, new
            {
                otp = otp,
                created_at = vnTime,
                expires_at = vnTime.AddMinutes(5),
                email = emailAccount
            });
        }
        public async Task<OtpDTO> GetOtp(string otp)
        {
            using var connection = _context.CreateConnection();
            var sql = """
    SELECT *
           FROM otp_table
           WHERE otp_code = @otp
    """;
            var acc = await connection.QueryAsync<OtpDTO>(sql, new { otp });
            return acc.FirstOrDefault();
        }
        public async Task<List<OtpDTO>> GetOtpByEmail(string email)
        {
            using var connection = _context.CreateConnection();
            var sql = """
    SELECT *
           FROM otp_table
           WHERE Account_id = (Select Account_id from account where Member_id = (select Member_id from members where email = @email)) and IsVerified = 0
    """;
            var otp = await connection.QueryAsync<OtpDTO>(sql, new { email });
            return otp.ToList();
        }
        public async Task DeleteOtp(string otp)
        {
            using var connection = _context.CreateConnection();
            var sql = """
            Delete 
        			FROM otp_table
        			
            WHERE otp_code = @otp
        """;
            await connection.QueryAsync(sql, new { otp });
            
        }
        public async Task UpdateOtp(OtpDTO otp)
        {
            using var connection = _context.CreateConnection();
            var sql = """
        UPDATE otp_table
        SET IsVerified = @IsVerified
        WHERE otp_code = @OtpCode
        """;
            await connection.ExecuteAsync(sql, new
            {
                IsVerified = otp.IsVerified,
                OtpCode = otp.otp_code
            });
        }
        public async Task UpdatePasswordAccByEmail(string email, string newPassword)
        {
            using var connection = _context.CreateConnection();

            // Query to get the username based on email from the member table
            var sqlGetUsername = """
    SELECT a.Username 
    FROM Account a
    JOIN Members m ON a.Member_id = m.Member_id
    WHERE m.email = @Email
    """;

            // Fetch the username
            var username = await connection.QuerySingleOrDefaultAsync<string>(sqlGetUsername, new { Email = email });

            if (username == null)
            {
                throw new Exception("No account found for the provided email.");
            }

            // Query to update the password based on username
            var sqlUpdatePassword = """
    UPDATE Account 
    SET Password = @Password
    WHERE Username = @Username
    """;

            // Execute the update password query
            await connection.ExecuteAsync(sqlUpdatePassword, new { Password = newPassword, Username = username });
        }
        public async Task<Account> GetAccountByEmail(string email)
        {
            using var connection = _context.CreateConnection();

            // Query to get the account based on email from the member table
            var sqlGetAccount = """
    SELECT a.*
    FROM Account a
    JOIN Members m ON a.Member_id = m.Member_id
    WHERE m.email = @Email
    """;

            // Fetch the account
            var account = await connection.QuerySingleOrDefaultAsync<Account>(sqlGetAccount, new { Email = email });

            if (account == null)
            {
                throw new KeyNotFoundException("Email không tồn tại trong hệ thống");
            }

            return account;
        }
    }
}
