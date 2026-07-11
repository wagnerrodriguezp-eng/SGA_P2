using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Desktop.Infrastructure.Config;

public class DriverProfileConfiguration : IEntityTypeConfiguration<DriverProfile>
{
    public void Configure(EntityTypeBuilder<DriverProfile> builder)
    {
        builder.ToTable("DriverProfiles");
        builder.ConfigureBaseEntity();
        builder.Property(d => d.LicenseNumber).HasMaxLength(20).IsRequired();
        builder.Property(d => d.PhoneNumber).HasMaxLength(20);
        builder.HasIndex(d => d.UserId).IsUnique();
    }
}
