using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Web.Infrastructure.Config;

public class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("Trips");
        builder.ConfigureBaseEntity();
        builder.Property(t => t.TripStatus).HasConversion<int>();

        // Backs the optimistic-concurrency capacity control — without this, a RowVersion mismatch
        // never throws and the capacity race-condition protection silently does nothing.
        builder.Property(t => t.RowVersion).IsRowVersion();
    }
}
