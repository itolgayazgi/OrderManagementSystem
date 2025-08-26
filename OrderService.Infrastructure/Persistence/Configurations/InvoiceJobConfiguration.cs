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
    public class InvoiceJobConfiguration : IEntityTypeConfiguration<InvoiceJob>
    {
        public void Configure(EntityTypeBuilder<InvoiceJob> builder)
        {
            builder.ToTable("InvoiceJob");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PayloadJson).HasColumnType("jsonb");
            builder.HasIndex(x => x.NextAttemptAt);
        }
    }
}
