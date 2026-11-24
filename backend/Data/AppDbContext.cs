using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Filter otomatis untuk Soft Delete (mengabaikan data yang sudah di-soft-delete)
        modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);
    }
}
