using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Desktop.Infrastructure.Config;

public class EmployeeProfileConfiguration : IEntityTypeConfiguration<EmployeeProfile>
{
    public void Configure(EntityTypeBuilder<EmployeeProfile> builder)
    {
        builder.ToTable("EmployeeProfiles");
        builder.ConfigureBaseEntity();
        builder.Property(e => e.EmployeeCode).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Department).HasMaxLength(100);
        builder.HasIndex(e => e.UserId).IsUnique();
    }
}
