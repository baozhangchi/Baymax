using System;
using Microsoft.EntityFrameworkCore;

namespace Baymax.Dal
{
    public class BaymaxDbContext : DbContext
    {
        private readonly string _connectionString;

        public BaymaxDbContext(string connectionString) : base()
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
            base.OnConfiguring(optionsBuilder);
        }
    }
}
