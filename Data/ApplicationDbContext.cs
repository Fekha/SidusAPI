using Microsoft.EntityFrameworkCore;
using SidusAPI.ServerModels;

namespace SidusAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<GameMatch> GameMatches { get; set; }
        public DbSet<GameTurn> GameTurns { get; set; }
        public DbSet<ServerNode> ServerNodes { get; set; }
        public DbSet<GamePlayer> GamePlayers { get; set; }
        public DbSet<ServerTechnology> ServerTechnologies { get; set; }
        public DbSet<ServerAction> ServerActions { get; set; }
        public DbSet<ServerUnit> ServerUnits { get; set; }
        public DbSet<ServerModule> ServerModules { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=data.gravitas-games.com, 1433;Database=SydusDB;User Id=SA;Password=Veersion101#;")
                   .EnableSensitiveDataLogging(true);
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Account>().HasKey(gt => new { gt.PlayerGuid });
            modelBuilder.Entity<GameMatch>().HasKey(gt => new { gt.GameGuid });
            modelBuilder.Entity<GameTurn>().HasKey(gt => new { gt.GameGuid, gt.TurnNumber });
            modelBuilder.Entity<ServerNode>().HasKey(gt => new { gt.GameGuid, gt.TurnNumber, gt.X, gt.Y });
            modelBuilder.Entity<GamePlayer>().HasKey(gt => new { gt.GameGuid, gt.TurnNumber, gt.PlayerId });
            modelBuilder.Entity<ServerTechnology>().HasKey(gt => new { gt.GameGuid, gt.TurnNumber, gt.PlayerId, gt.TechnologyId });
            modelBuilder.Entity<ServerAction>().HasKey(gt => new { gt.GameGuid, gt.TurnNumber, gt.PlayerId, gt.ActionOrder });
            modelBuilder.Entity<ServerUnit>().HasKey(gt => new { gt.GameGuid, gt.TurnNumber, gt.PlayerId, gt.UnitGuid });
            modelBuilder.Entity<ServerModule>().HasKey(gt => new { gt.GameGuid, gt.TurnNumber, gt.ModuleGuid });
        }
    }
}
