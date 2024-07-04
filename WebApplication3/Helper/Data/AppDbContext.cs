using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Data;
using WebApplication3.Entities;

namespace WebApplication3.Helper.Data
{
    public class AppDbContext
    {
        private DbSettings _dbSettings;

        public AppDbContext(IOptions<DbSettings> dbSettings)
        {
            _dbSettings = dbSettings.Value;
        }

        public IDbConnection CreateConnection()
        {
            var connectionString = $"Server={_dbSettings.Server}; Database={_dbSettings.Database}; Uid={_dbSettings.UserId}; Pwd={_dbSettings.Password};";
            return new MySqlConnection(connectionString);
        }
    }

}
