using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WebApplication3.DTOs;
using WebApplication3.DTOs.Account;
using WebApplication3.DTOs.Sponsor;
using WebApplication3.Helper.Data;

namespace WebApplication3.repository.SponsorRepository
{
    public interface ISponsorRepository
    {
        Task<PagedResult<SponsorDTO>> GetAllSponsor(int pageNumber, int? ProjectId = null);
        Task AddSponsor(int Project_id, SponsorDTO sponsor);
        Task DeleteSponsor(string name);
        Task<SponsorDTO> GetSponsor(string name);
        Task UpdateSponsors(SponsorDTO sponsor);

    }
    public class SponsorRepository : ISponsorRepository
    {
        private AppDbContext _context;
        public SponsorRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddSponsor(int project_id,SponsorDTO sponsor)
        {
            using var connection = _context.CreateConnection();
            var sql = """
            INSERT INTO sponsor (Name, Contact,Address,ContributionAmount)
            VALUES (@name, @contact,@address,@contributionAmount);
            SELECT LAST_INSERT_ID();
        """;
            var sponsor_id = await connection.ExecuteScalarAsync<int>(sql, new
            {
                Name = sponsor.name,
                Contact = sponsor.contact,
                Address = sponsor.address,
                ContributionAmount = sponsor.contributionAmount
            });
            var sqlAddToProject = @"insert into ProjectSponsor (Sponsor_id ,Project_id) values (@sponsor_id, @project_id);";
            await connection.ExecuteAsync(sqlAddToProject, new
            {
                sponsor_id = sponsor_id,
                project_id = project_id
            });
            var sqlUpdateContribution = @"UPDATE Projects
SET Contributions = Contributions + (
    SELECT ContributionAmount
    FROM sponsor
    WHERE sponsor_id = @sponsor_id
)
WHERE Project_id = @project_id;";
            await connection.ExecuteAsync(sqlUpdateContribution, new
            {
                sponsor_id = sponsor_id,
                project_id = project_id
            });
        }

        public async Task DeleteSponsor(string name)
        {
            using var connection = _context.CreateConnection();
            var sql = """
            DELETE FROM sponsor 
            WHERE Name = @name
        """;
            await connection.ExecuteAsync(sql, new { name });
        }

        public async Task<PagedResult<SponsorDTO>> GetAllSponsor(int pageNumber, int? ProjectId = null)
        {
            using var connection = _context.CreateConnection();
            int pageSize = 6;
            var offset = (pageNumber - 1) * pageSize;

            // Build the base SQL query
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT * FROM sponsor");

            // If a ProjectId is specified, add a WHERE clause
            if (ProjectId.HasValue)
            {
                sqlBuilder.Append(" WHERE Sponsor_id IN (SELECT Sponsor_id FROM projectsponsor WHERE Project_id = @ProjectId)");
            }

            // Add LIMIT and OFFSET for pagination
            sqlBuilder.Append(" LIMIT @pageSize OFFSET @offset;");

            // Execute the query
            var sponsors = await connection.QueryAsync<SponsorDTO>(sqlBuilder.ToString(), new { ProjectId, pageSize, offset });

            // Get the total count of records
            var countSql = "SELECT COUNT(*) FROM sponsor";
            if (ProjectId.HasValue)
            {
                countSql += " WHERE Sponsor_id IN (SELECT Sponsor_id FROM projectsponsor WHERE Project_id = @ProjectId)";
            }
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { ProjectId });

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PagedResult<SponsorDTO>
            {
                Data = sponsors.ToList(),
                TotalPages = totalPages
            };
        }

        public async Task<SponsorDTO> GetSponsor(string name)
        {
            using var connection = _context.CreateConnection();
            var sql = """
            SELECT * FROM sponsor 
            WHERE Name = @name
        """;
            return await connection.QuerySingleOrDefaultAsync<SponsorDTO>(sql, new { name });
        }

        public async Task UpdateSponsors(SponsorDTO sponsor)
        {
            using var connection = _context.CreateConnection();
            var sql = """
            UPDATE sponsor 
        SET contact = @contact, address = @address
        WHERE Name = @name
        """;
            await connection.ExecuteAsync(sql, sponsor);
        }
    }
}
