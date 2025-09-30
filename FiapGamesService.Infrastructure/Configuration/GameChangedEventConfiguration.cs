using FiapGamesService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiapGamesService.Infrastructure.Configuration
{
    public class GameChangedEventConfiguration : IEntityTypeConfiguration<GameChangedEvent>
    {
        public void Configure(EntityTypeBuilder<GameChangedEvent> builder)
        {
            builder
                .HasKey(x => x.Id);

            builder
                .Property(x => x.OldName)
                .HasMaxLength(100);
            builder
                .Property(x => x.NewName)
                .HasMaxLength(100);

            builder
                .Property(x => x.OldDescription)
                .HasMaxLength(250);
            builder
                .Property(x => x.NewDescription)
                .HasMaxLength(250);

            builder
                .Property(x => x.OldGenre)
                .HasMaxLength(30);
            builder
                .Property(x => x.NewGenre)
                .HasMaxLength(30);

            builder
                .Property(x => x.OldPrice)
                .HasPrecision(10, 2);
            builder
                .Property(x => x.NewPrice)
                .HasPrecision(10, 2);

            builder
                .Property(x => x.ChangedAt)
                .HasColumnType("datetime2");

            builder
                .Property(x => x.Observation)
                .HasMaxLength(250);

            builder.HasOne<GameCreatedEvent>()
                .WithMany()
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasIndex(x => new { x.GameId, x.ChangedAt });
        }
    }
}
