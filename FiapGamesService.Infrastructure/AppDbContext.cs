using FiapGamesService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FiapGamesService.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<GameCreatedEvent> GameCreatedEvent { get; set; }
        public DbSet<GameChangedEvent> GameChangedEvent { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
