using Microsoft.EntityFrameworkCore;
using EncoraOne.Grievance.API.Models;
using System; // Ensure System is imported for DateTime.UtcNow

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


        public DbSet<ComplaintComment> ComplaintComments { get; set; } // NEW

        // 🚨 CRITICAL FIX: The base User class MUST be a DbSet<User> 
        // to enable generic queries (like in AdminService.cs) and to properly
        // set up the TPT inheritance mapping.
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // 1. Configure Inheritance Strategy (TPT)
            // ============================================
            // These configurations explicitly map the base class and derived classes 
            // to their respective tables, confirming the TPT strategy.
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Employee>().ToTable("Employees");
            modelBuilder.Entity<Manager>().ToTable("Managers");

            // Ensure the primary keys are correctly mapped, especially for TPT
            // where the PKs are shared (Id in User is the FK in Employee/Manager).
            modelBuilder.Entity<Employee>()
                .HasBaseType<User>();

            modelBuilder.Entity<Manager>()
                .HasBaseType<User>();

            // ============================================
            // 2. Configure Relationships
            // ============================================

            // Department -> Managers (One-to-Many)
            modelBuilder.Entity<Manager>()
                .HasOne(m => m.Department)
                .WithMany(d => d.Managers)
                .HasForeignKey(m => m.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

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
            // NOTE: When seeding TPT data, you typically use the concrete class (Manager) 
            // but ensure all base class properties (User) are included.
            modelBuilder.Entity<Manager>().HasData(
                new Manager
                {
                    Id = 1,
                    FullName = "Super Admin",
                    Email = "admin@encora.com",
                    // ⚠️ REMINDER: This should be a SHA-256 or similar hash, not plain text!
                    PasswordHash = "Admin@123",
                    Role = UserRole.Admin, // UserRole enum value for Admin
                    DepartmentId = 1, // Administration
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    // Manager specific property
                    JobTitle = "Chief Administrator"
                }
            );
        }
    }
}