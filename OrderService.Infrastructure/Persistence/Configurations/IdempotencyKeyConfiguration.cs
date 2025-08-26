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
    public class IdempotencyKeyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
    {
        public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
        {
            builder.ToTable("IdempotencyKey");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Key).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Endpoint).IsRequired().HasMaxLength(150);
            builder.Property(x => x.RequestHash).HasMaxLength(128);
            builder.Property(x => x.ResponseBody).HasColumnType("text");
            builder.HasIndex(x => new { x.Key, x.Endpoint }).IsUnique();
        }
        //Field'ları bu şekilde sınırlamamızın sebebi nedir?
    }
}
