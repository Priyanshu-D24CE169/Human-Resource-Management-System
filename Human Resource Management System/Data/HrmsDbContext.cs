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
                entity.Property(e => e.Password).HasMaxLength(255); // Ensure adequate length for BCrypt hashes
            });

            // Configure Employee entity
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmployeeId);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.EmployeeCode).IsUnique();
                entity.Property(e => e.Salary).HasColumnType("decimal(10,2)"); // MySQL compatible decimal precision
                entity.Property(e => e.EmployeeCode).HasMaxLength(20);
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Department).HasMaxLength(50);
                entity.Property(e => e.Position).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
                entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.Nationality).HasMaxLength(50);
                entity.Property(e => e.ProfileImagePath).HasMaxLength(255);
                entity.Property(e => e.RegistrationToken).HasMaxLength(100);
            });

            // Configure Attendance entity
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.AttendanceId);
                entity.HasOne(e => e.Employee)
                      .WithMany(e => e.Attendances)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.WorkingHours).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Status).HasMaxLength(20);
            });

            // Configure Payroll entity
            modelBuilder.Entity<Payroll>(entity =>
            {
                entity.HasKey(e => e.PayrollId);
                entity.HasOne(e => e.Employee)
                      .WithMany(e => e.Payrolls)
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.BasicSalary).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Allowances).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Deductions).HasColumnType("decimal(10,2)");
                entity.Property(e => e.NetSalary).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Status).HasMaxLength(20);
            });

            // Configure LeaveRequest entity
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(e => e.LeaveRequestId);
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Property(e => e.LeaveType).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.Reason).HasMaxLength(500);
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

            // Seed sample employees - Simplified for compatibility
            var sampleEmployees = new[]
            {
                new
                {
                    EmployeeId = 1,
                    EmployeeCode = "OIJODO20230001",
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@hrms.com",
                    Phone = "+1234567890",
                    Department = "IT",
                    Position = "Software Developer",
                    HireDate = new DateTime(2023, 1, 15),
                    Salary = 75000m,
                    Status = "Active",
                    IsRegistrationComplete = true,
                    CreatedDate = fixedDate
                },
                new
                {
                    EmployeeId = 2,
                    EmployeeCode = "OIJASM20220001",
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@hrms.com",
                    Phone = "+1234567891",
                    Department = "HR",
                    Position = "HR Manager",
                    HireDate = new DateTime(2022, 8, 20),
                    Salary = 85000m,
                    Status = "Active",
                    IsRegistrationComplete = true,
                    CreatedDate = fixedDate
                },
                new
                {
                    EmployeeId = 3,
                    EmployeeCode = "OIMIJO20230002",
                    FirstName = "Mike",
                    LastName = "Johnson",
                    Email = "mike.johnson@hrms.com",
                    Phone = "+1234567892",
                    Department = "Finance",
                    Position = "Accountant",
                    HireDate = new DateTime(2023, 3, 10),
                    Salary = 65000m,
                    Status = "Active",
                    IsRegistrationComplete = false,
                    CreatedDate = fixedDate
                }
            };

            // Only seed if no employees exist
            modelBuilder.Entity<Employee>().HasData(sampleEmployees.Select(e => new Employee
            {
                EmployeeId = e.EmployeeId,
                EmployeeCode = e.EmployeeCode,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Phone = e.Phone,
                Department = e.Department,
                Position = e.Position,
                HireDate = e.HireDate,
                Salary = e.Salary,
                Status = e.Status,
                IsRegistrationComplete = e.IsRegistrationComplete,
                CreatedDate = e.CreatedDate
            }));
        }
    }
}