using Microsoft.EntityFrameworkCore;
using Human_Resource_Management_System.Models;

namespace Human_Resource_Management_System.Data
{
    public class HrmsDbContext : DbContext
    {
        public HrmsDbContext(DbContextOptions<HrmsDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.Role).HasMaxLength(20);
            });

            // Configure Employee entity
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmployeeId);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
            });

            // Configure Attendance entity
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.AttendanceId);
                entity.HasOne(e => e.Employee)
                      .WithMany(e => e.Attendances)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.WorkingHours).HasColumnType("decimal(18,2)");
            });

            // Configure Payroll entity
            modelBuilder.Entity<Payroll>(entity =>
            {
                entity.HasKey(e => e.PayrollId);
                entity.HasOne(e => e.Employee)
                      .WithMany(e => e.Payrolls)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.BasicSalary).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Allowances).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Deductions).HasColumnType("decimal(18,2)");
                entity.Property(e => e.NetSalary).HasColumnType("decimal(18,2)");
            });

            // Configure LeaveRequest entity
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(e => e.LeaveRequestId);
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Seed data - Note: DateTime.Now in seed data might cause issues, using fixed date instead
            var fixedDate = new DateTime(2024, 1, 1);
            
            // Seed default admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Email = "admin@hrms.com",
                    Password = "$2a$11$dummy.hash.for.admin123", // Will be replaced with actual hash
                    FirstName = "System",
                    LastName = "Administrator",
                    Role = "Admin",
                    CreatedDate = fixedDate,
                    IsActive = true
                }
            );

            // Seed sample employees
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    EmployeeId = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@hrms.com",
                    Phone = "+1234567890",
                    Department = "IT",
                    Position = "Software Developer",
                    HireDate = new DateTime(2023, 1, 15),
                    Salary = 75000,
                    Status = "Active",
                    CreatedDate = fixedDate
                },
                new Employee
                {
                    EmployeeId = 2,
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@hrms.com",
                    Phone = "+1234567891",
                    Department = "HR",
                    Position = "HR Manager",
                    HireDate = new DateTime(2022, 8, 20),
                    Salary = 85000,
                    Status = "Active",
                    CreatedDate = fixedDate
                },
                new Employee
                {
                    EmployeeId = 3,
                    FirstName = "Mike",
                    LastName = "Johnson",
                    Email = "mike.johnson@hrms.com",
                    Phone = "+1234567892",
                    Department = "Finance",
                    Position = "Accountant",
                    HireDate = new DateTime(2023, 3, 10),
                    Salary = 65000,
                    Status = "Active",
                    CreatedDate = fixedDate
                }
            );
        }
    }
}