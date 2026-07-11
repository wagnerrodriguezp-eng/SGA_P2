using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGA.SharedKernel.Domain.Entities;

namespace SGA.Web.Infrastructure.Config;

public class NotificationMessageConfiguration : IEntityTypeConfiguration<NotificationMessage>
{
    public void Configure(EntityTypeBuilder<NotificationMessage> builder)
    {
        builder.ToTable("NotificationMessages");
        builder.ConfigureBaseEntity();
        builder.Property(n => n.RecipientEmail).HasMaxLength(256).IsRequired();
        builder.Property(n => n.Subject).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Body).HasMaxLength(4000).IsRequired();
        builder.Property(n => n.MessageStatus).HasConversion<int>();
    }
}
