using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TpvVyber.Client.Classes;

namespace TpvVyber.Data;

public class TpvVyberContext : DbContext
{
    public TpvVyberContext(DbContextOptions<TpvVyberContext> options)
    : base(options)
    {
    }

    public DbSet<Student> Students { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<OrderCourse> OrderCourses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set default schema for all tables
        modelBuilder.HasDefaultSchema("tpv_schema");

        // Optional: configure tables explicitly
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Courses");
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<OrderCourse>(entity =>
        {
            entity.ToTable("OrderCourses");
            entity.HasKey(e => e.Id);
        });

        base.OnModelCreating(modelBuilder);
    }
}
