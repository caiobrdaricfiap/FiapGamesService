using FiapGamesService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiapGamesService.Infrastructure.Configuration
{
    public class GameCreatedEventConfiguration : IEntityTypeConfiguration<GameCreatedEvent>
    {
        public void Configure(EntityTypeBuilder<GameCreatedEvent> builder)
        {
            builder
                .HasKey(u => u.Id);

            builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(250);

            builder.Property(x => x.Genre)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Price)
                .HasPrecision(10, 2)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.HasIndex(x => x.Genre);
        }
    }
}
