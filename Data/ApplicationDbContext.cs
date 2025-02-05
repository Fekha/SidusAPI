using Microsoft.EntityFrameworkCore;
using SidusAPI.ServerModels;

namespace SidusAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }
        public DbSet<Settings> Settings { get; set; }
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
                //optionsBuilder.UseSqlServer("Server=(LocalDb)\\MSSQLLocalDB;Initial Catalog=Sydus;Integrated Security=SSPI;")
                //   .EnableSensitiveDataLogging(true);
                optionsBuilder.UseSqlServer("Server=tcp:sydus.database.windows.net,1433;Initial Catalog=Sydus;Persist Security Info=False;User ID=sql_james;Password=Version101#;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;")
                   .EnableSensitiveDataLogging(true);
                //run the command 'update-database' if rebuilding new server
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Settings>()
                .HasNoKey();

            modelBuilder.Entity<Account>()
                .HasKey(gt => new { gt.PlayerGuid });
            modelBuilder.Entity<Account>()
                .HasIndex(gm => gm.PlayerGuid);

            modelBuilder.Entity<GameMatch>()
                .HasKey(gt => new { gt.GameGuid });
            modelBuilder.Entity<GameMatch>()
                .HasIndex(gm => gm.GameGuid);

            modelBuilder.Entity<GameTurn>()
                .HasKey(gt => new { gt.GameGuid, gt.TurnNumber });
            modelBuilder.Entity<GameTurn>()
                .HasIndex(gt => new { gt.GameGuid, gt.TurnNumber }); 

            modelBuilder.Entity<ServerNode>()
                .HasKey(gt => new { gt.GameGuid, gt.TurnNumber, gt.X, gt.Y });
            modelBuilder.Entity<ServerNode>()
                .HasIndex(sn => new { sn.GameGuid, sn.TurnNumber });

            modelBuilder.Entity<GamePlayer>()
                .HasKey(gt => new { gt.GameGuid, gt.TurnNumber, gt.PlayerGuid });
            modelBuilder.Entity<GamePlayer>()
                .HasIndex(gp => new { gp.GameGuid, gp.TurnNumber, gp.PlayerGuid });

            modelBuilder.Entity<ServerTechnology>()
                .HasKey(gt => new { gt.GameGuid, gt.TurnNumber, gt.PlayerGuid, gt.TechnologyId });
            modelBuilder.Entity<ServerTechnology>()
                .HasIndex(st => new { st.GameGuid, st.TurnNumber, st.PlayerGuid });

            modelBuilder.Entity<ServerAction>()
                .HasKey(gt => new { gt.GameGuid, gt.TurnNumber, gt.PlayerGuid, gt.ActionOrder });
            modelBuilder.Entity<ServerAction>()
                .HasIndex(sa => new { sa.GameGuid, sa.TurnNumber, sa.PlayerGuid }); 

            modelBuilder.Entity<ServerUnit>()
                .HasKey(gt => new { gt.GameGuid, gt.TurnNumber, gt.PlayerGuid, gt.UnitGuid });
            modelBuilder.Entity<ServerUnit>()
                .HasIndex(su => new { su.GameGuid, su.TurnNumber, su.PlayerGuid }); 

            modelBuilder.Entity<ServerModule>()
                .HasKey(gt => new { gt.GameGuid, gt.TurnNumber, gt.ModuleGuid });
            modelBuilder.Entity<ServerModule>()
                .HasIndex(sm => new { sm.GameGuid, sm.TurnNumber });
        }

    }
}
