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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connectionString);
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<TestStep> TestSteps { get; set; }

        public DbSet<TestResult> TestResults { get; set; }

        public DbSet<TestHistory> TestHistories { get; set; }
    }
}
