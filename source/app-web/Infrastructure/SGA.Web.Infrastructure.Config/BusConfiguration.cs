using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Web.Infrastructure.Config;

public class BusConfiguration : IEntityTypeConfiguration<Bus>
{
    public void Configure(EntityTypeBuilder<Bus> builder)
    {
        builder.ToTable("Buses");
        builder.ConfigureBaseEntity();
        builder.Property(b => b.PlateNumber).HasMaxLength(15).IsRequired();
        builder.HasIndex(b => b.PlateNumber).IsUnique();
        builder.Property(b => b.Model).HasMaxLength(100);
        builder.Property(b => b.BusStatus).HasConversion<int>();
    }
}
