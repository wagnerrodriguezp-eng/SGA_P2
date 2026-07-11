using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Desktop.Infrastructure.Config;

public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
        builder.ToTable("StudentProfiles");
        builder.ConfigureBaseEntity();
        builder.Property(s => s.StudentCode).HasMaxLength(20).IsRequired();
        builder.Property(s => s.Career).HasMaxLength(100);
        builder.HasIndex(s => s.UserId).IsUnique();
    }
}
