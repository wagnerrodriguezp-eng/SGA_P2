using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGA.SharedKernel.Domain.Common;

namespace SGA.Web.Infrastructure.Config;

public static class EntityTypeBuilderExtensions
{
    public static void ConfigureBaseEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity<Guid>
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RecordStatus).HasConversion<int>().IsRequired();
        builder.Property(e => e.CreatedAtUtc).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(50).IsRequired();
        builder.Property(e => e.ModifiedBy).HasMaxLength(50);
    }
}
