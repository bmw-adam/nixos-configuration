using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TpvVyber.Classes;

namespace TpvVyber.Data;

public class TpvVyberContext : DbContext
{
    public TpvVyberContext(DbContextOptions<TpvVyberContext> options)
        : base(options) { }

    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<OrderCourse> OrderCourses { get; set; } = null!;
    public DbSet<LoggingEnding> LoggingEndings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set default schema for all tables
        modelBuilder.HasDefaultSchema("tpv_schema");

        // Optional: configure tables explicitly
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(e => e.Id);

            entity.HasAlternateKey(e => e.Email);
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
            entity
                .HasOne(oc => oc.Student)
                .WithMany(c => c.OrderCourses)
                .HasForeignKey(oc => oc.StudentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(oc => oc.Course)
                .WithMany(c => c.OrderCourses)
                .HasForeignKey(oc => oc.CourseId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }
}
