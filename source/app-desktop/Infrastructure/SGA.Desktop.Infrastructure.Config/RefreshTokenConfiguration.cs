using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Desktop.Infrastructure.Config;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.ConfigureBaseEntity();
        builder.Property(r => r.Token).HasMaxLength(256).IsRequired();
        builder.HasIndex(r => r.Token).IsUnique();
        builder.Property(r => r.ReplacedByToken).HasMaxLength(256);
        builder.Property(r => r.RowVersion).IsRowVersion();
    }
}
