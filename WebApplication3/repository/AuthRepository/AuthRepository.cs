using Dapper;
using Microsoft.EntityFrameworkCore;
using WebApplication3.DTOs.Account;
using WebApplication3.Helper.Data;

namespace WebApplication3.repository.AuthRepository
{
    public interface IAuthRepository
    {
        Task AddAcount(AccountDTO acc, int Member_id);

    }
    public class AuthRepository : IAuthRepository
    {
        private AppDbContext _context;
        public AuthRepository(AppDbContext context)

        {
            _context = context;
        }
        public async Task AddAcount(AccountDTO acc, int Member_id)
        {
            using var connection = _context.CreateConnection();
            var sql = """
            INSERT INTO account (Username, Password,Member_id)
            VALUES (@Username, @Password,@Member_id)
        """;
            await connection.ExecuteAsync(sql, new
            {
                Username = acc.Username,
                Password = acc.Password,
                Member_id = Member_id
            });
        }
    }
}
