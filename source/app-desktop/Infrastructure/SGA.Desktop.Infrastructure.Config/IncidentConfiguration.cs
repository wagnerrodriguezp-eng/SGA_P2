using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Desktop.Infrastructure.Config;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("Incidents");
        builder.ConfigureBaseEntity();
        builder.Property(i => i.IncidentType).HasConversion<int>();
        builder.Property(i => i.IncidentStatus).HasConversion<int>();
        builder.Property(i => i.Description).HasMaxLength(500).IsRequired();
    }
}
