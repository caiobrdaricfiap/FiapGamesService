using FIAP.Games.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.Games.Infrastructure.Configuration
{
    public class GameConfiguration : IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
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
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.HasIndex(x => x.Genre);
        }
    }
}
