using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Web.Infrastructure.Config;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.ConfigureBaseEntity();
        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(50);
        builder.Property(a => a.Details).HasMaxLength(2000);
        builder.Property(a => a.IpAddress).HasMaxLength(45);
    }
}
