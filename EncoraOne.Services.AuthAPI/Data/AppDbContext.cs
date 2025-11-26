using Microsoft.EntityFrameworkCore;
using EncoraOne.Grievance.API.Models;

namespace EncoraOne.Grievance.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tables
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Complaint> Complaints { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // 1. Configure Inheritance Strategy (TPT)
            // ============================================
            // Since User is abstract and we have [Table] attributes on children,
            // EF Core will create a "Users" table for shared fields, 
            // and "Employees"/"Managers" tables for specific fields.
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Employee>().ToTable("Employees");
            modelBuilder.Entity<Manager>().ToTable("Managers");

            // ============================================
            // 2. Configure Relationships
            // ============================================

            // Department -> Managers (One-to-Many)
            modelBuilder.Entity<Manager>()
                .HasOne(m => m.Department)
                .WithMany(d => d.Managers)
                .HasForeignKey(m => m.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting Dept if Managers exist

            // Department -> Complaints (One-to-Many)
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Department)
                .WithMany(d => d.Complaints)
                .HasForeignKey(c => c.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employee -> Complaints (One-to-Many)
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Employee)
                .WithMany(e => e.Complaints)
                .HasForeignKey(c => c.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================
            // 3. Seed Data (Initial Data)
            // ============================================

            // Seed Departments
            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentId = 1, Name = "Administration" },
                new Department { DepartmentId = 2, Name = "Human Resources" },
                new Department { DepartmentId = 3, Name = "IT Support" }
            );

            // Seed Super Admin (As a Manager of Administration)
            // Password: "Admin@123" (In a real app, hash this!)
            modelBuilder.Entity<Manager>().HasData(
                new Manager
                {
                    Id = 1,
                    FullName = "Super Admin",
                    Email = "admin@encora.com",
                    PasswordHash = "Admin@123", // We will implement hashing later
                    Role = UserRole.Admin,
                    DepartmentId = 1, // Administration
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            );
        }
    }
}