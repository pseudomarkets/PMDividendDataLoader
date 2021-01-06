using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PMCommonEntities.Models;
using PMUnifiedAPI.Models;

namespace PMDividendDataLoader
{
    public class PseudoMarketsDbContext : DbContext
    {
        public DbSet<Positions> Positions { get; set; }
        public DbSet<ApiKeys> ApiKeys { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Grab the connection string from appsettings.json
            var connectionString = Program.Configuration.GetConnectionString("SqlServer");

            // Use the SQL Server Entity Framework Core connector
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
