using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using WebApplication3.DTOs.Groups;
using WebApplication3.DTOs.Sponsor;
using WebApplication3.Entities;
using WebApplication3.Helper.Data;

namespace WebApplication3.repository.GroupsRepository
{
    public interface IGroupsRepository
    {
        Task<List<Group>> GetAllGroups();
        Task AddGroups(GroupsDTOs Group);
        Task DeleteGroups(string name);
        Task<Group> GetGroup(int id);
        Task<Group> GetGroup(string name);
    }
    public class GroupsRepository : IGroupsRepository
    {
        private AppDbContext _context;
        public GroupsRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddGroups(GroupsDTOs Group)
        {
            using var connection = _context.CreateConnection();
            var sql = @"
       INSERT INTO `Groups` (group_name)
    VALUES (@group_name)
    ";
            await connection.ExecuteAsync(sql, Group);
        }

        public async Task DeleteGroups(string name)
        {
            using var connection = _context.CreateConnection();
            var sql = """
            DELETE FROM `Groups` 
            WHERE group_name = @name
        """;
            await connection.ExecuteAsync(sql, new { name });
        }

        public async Task<List<Group>> GetAllGroups()
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT * FROM `Groups`";
            var sponsor = await connection.QueryAsync<Group>(sql);
            return sponsor.ToList();
        }

        public async Task<Group> GetGroup(int id)
        {

            using var connection = _context.CreateConnection();
            var sql = """
            SELECT * FROM `Groups` 
            WHERE Group_id = @id
        """;
            return await connection.QuerySingleOrDefaultAsync<Group>(sql, new { id });
        }
        public async Task<Group> GetGroup(string name)
        {

            using var connection = _context.CreateConnection();
            var sql = """
            SELECT * FROM `Groups` 
            WHERE group_name = @name
        """;
            return await connection.QuerySingleOrDefaultAsync<Group>(sql, new { name });
        }
    }
}
