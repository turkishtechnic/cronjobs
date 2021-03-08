using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace HangfireDemo
{
    public class Sale
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Total { get; set; }
    }

    public class DemoDbContext : DbContext
    {
        public DbSet<Sale> Sales { get; set; }

        public DemoDbContext(DbContextOptions<DemoDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var r = new Random((int) DateTime.UtcNow.Ticks);
            var items = Enumerable.Range(0, r.Next(10, 15)).Select(i => new Sale {Total = r.Next(100)});
            modelBuilder.Entity<Sale>().HasData(items);
        }
    }
}