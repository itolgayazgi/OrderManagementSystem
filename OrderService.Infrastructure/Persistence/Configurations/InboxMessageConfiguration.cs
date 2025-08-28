using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entity;

namespace OrderService.Infrastructure.Persistence.Configurations
{
    public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
    {
        public void Configure(EntityTypeBuilder<InboxMessage> builder)
        {
            builder.ToTable("InboxMessage");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.MessageId).IsRequired().HasMaxLength(128);
            builder.Property(x => x.EventType)
               .IsRequired()
               .HasMaxLength(64);
            builder.Property(x => x.ProcessedAt)
               .IsRequired();
            builder.HasIndex(x => new { x.MessageId, x.EventType })
               .IsUnique();
            builder.HasIndex(x => x.ProcessedAt);
        }
    }
}
