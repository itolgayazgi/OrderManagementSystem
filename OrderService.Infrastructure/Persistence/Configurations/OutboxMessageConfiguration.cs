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
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessage");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.EventType).IsRequired().HasMaxLength(128);
            builder.Property(x => x.Payload).IsRequired().HasColumnType("jsonb");
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.ProcessedAt);
            builder.Property(x => x.ProcessAt);
            builder.Property(x => x.RetryCount).IsRequired().HasDefaultValue(0);
            builder.Property(x => x.Error).HasMaxLength(1000);
           

            // Dispatcher taraması için faydalı indexler
            builder.HasIndex(x => new { x.ProcessedAt, x.ProcessAt });
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => x.EventType);
        }
    }
}
