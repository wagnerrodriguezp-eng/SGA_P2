using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Desktop.Infrastructure.Config;

public class StopConfiguration : IEntityTypeConfiguration<Stop>
{
    public void Configure(EntityTypeBuilder<Stop> builder)
    {
        builder.ToTable("Stops");
        builder.ConfigureBaseEntity();
        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.Property(s => s.StopStatus).HasConversion<int>();
    }
}
