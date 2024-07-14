using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Server.Models.Cards;
using Server.Models.Common;

namespace Server.Persistence
{
    public class ServerDbContext : DbContext
    {
        public ServerDbContext(DbContextOptions<ServerDbContext> options) : base(options) 
        { 
            
        }

        public DbSet<EchelonSetting> EchelonSettings { get; set; }
        public DbSet<CardProfile> CardProfiles { get; set; }
        public DbSet<OfflinePvpBattleResult> OfflinePvpBattleResults { get; set; }
        public DbSet<Snapshot> Snapshots { get; set; }
        public DbSet<TagTeamData> TagTeamData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override int SaveChanges()
        {
            var entityEntries = ChangeTracker.Entries().ToList();

            entityEntries.ForEach(entityEntry =>
            {
                if (entityEntry.State == EntityState.Added)
                {
                    Entry(entityEntry.Entity).Property(nameof(BaseEntity.CreateTime)).CurrentValue = DateTime.Now;
                    Entry(entityEntry.Entity).Property(nameof(BaseEntity.UpdateTime)).CurrentValue = DateTime.Now;
                }
                if (entityEntry.State == EntityState.Modified)
                {
                    Entry(entityEntry.Entity).Property(nameof(BaseEntity.UpdateTime)).CurrentValue = DateTime.Now;
                }
            });
            
            return base.SaveChanges();
        }
    }
}