using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Desktop.Infrastructure.Config;

public class AuthorizationConfiguration : IEntityTypeConfiguration<Authorization>
{
    public void Configure(EntityTypeBuilder<Authorization> builder)
    {
        builder.ToTable("Authorizations");
        builder.ConfigureBaseEntity();
        builder.Property(a => a.AuthorizationType).HasConversion<int>();
        builder.Property(a => a.AuthorizationStatus).HasConversion<int>();
        builder.Property(a => a.RowVersion).IsRowVersion();
    }
}
