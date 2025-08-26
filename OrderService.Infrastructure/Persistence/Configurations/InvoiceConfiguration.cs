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
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoice");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.InvoiceNo).IsRequired().HasMaxLength(50);
            builder.HasIndex(x => x.InvoiceNo).IsUnique();
            builder.HasIndex(x => x.OrderId).IsUnique(); // her siparişe tek fatura
        }
    }
}
