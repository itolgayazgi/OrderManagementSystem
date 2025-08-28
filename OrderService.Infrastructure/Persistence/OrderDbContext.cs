using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entity;

namespace OrderService.Infrastructure.Persistence
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options ) : base(options) { }
        public DbSet<Product> Product => Set<Product>();
        public DbSet<Order> Order => Set<Order>();
        public DbSet<OrderItem> OrderItem => Set<OrderItem>();
        public DbSet<Invoice> Invoice => Set<Invoice>();
        public DbSet<IdempotencyKey> IdempotencyKey => Set<IdempotencyKey>();
        public DbSet<InvoiceJob> InvoiceJob => Set<InvoiceJob>();

        // (EDA için öneri) Outbox/Inbox ekleyeceğiz; şimdilik atlayabilirsin.
        public DbSet<OutboxMessage> OutboxMessage => Set<OutboxMessage>();
        public DbSet<InboxMessage> InboxMessage => Set<InboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
