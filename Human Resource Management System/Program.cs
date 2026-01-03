using Human_Resource_Management_System.Data;
using Human_Resource_Management_System.Services;
using Human_Resource_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for network access
builder.WebHost.ConfigureKestrel(options =>
{
    var networkSettings = builder.Configuration.GetSection("NetworkSettings");
    var enableNetworkAccess = networkSettings.GetValue<bool>("EnableNetworkAccess");
    
    if (enableNetworkAccess)
    {
        var port = networkSettings.GetValue<int>("Port");
        var httpPort = networkSettings.GetValue<int>("HttpPort");
        
        // Get local IP address
        var localIP = GetLocalIPAddress();
        
        options.Listen(IPAddress.Any, httpPort); // HTTP
        options.Listen(IPAddress.Any, port, listenOptions =>
        {
            listenOptions.UseHttps(); // HTTPS
        });
        
        Console.WriteLine($"Server will be accessible on:");
        Console.WriteLine($"  Local: https://localhost:{port}");
        Console.WriteLine($"  Local: http://localhost:{httpPort}");
        Console.WriteLine($"  Network: https://{localIP}:{port}");
        Console.WriteLine($"  Network: http://{localIP}:{httpPort}");
    }
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add logging
builder.Services.AddLogging();

// Determine the appropriate connection string
var connectionString = await GetWorkingConnectionStringAsync(builder.Configuration);

// Validate connection string before using it
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("? No valid connection string found. Using fallback...");
    connectionString = "Server=192.168.0.114;Port=3306;Database=hrms_database;Uid=root;Pwd=;Connect Timeout=30;Command Timeout=30;";
}

Console.WriteLine($"?? Using connection string: {connectionString.Replace("Pwd=", "Pwd=***")}");

// Add Entity Framework with MySQL using Pomelo provider
builder.Services.AddDbContext<HrmsDbContext>(options =>
{
    try
    {
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
            mysqlOptions =>
            {
                mysqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Error configuring MySQL: {ex.Message}");
        // Fallback to basic configuration
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));
    }
});

// Add services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();

// Add session support for authentication
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Auto-migrate database on startup
await MigrateDatabaseAsync(app);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();

static string GetLocalIPAddress()
{
    try
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting local IP: {ex.Message}");
    }
    return "localhost";
}

static async Task<string> GetWorkingConnectionStringAsync(IConfiguration configuration)
{
    var databaseSettings = configuration.GetSection("DatabaseSettings");
    var useCustomServer = databaseSettings.GetValue<bool>("UseCustomServer");
    
    // Priority order for connection strings
    var connectionStringsToTest = new List<(string name, string connectionString)>();
    
    if (useCustomServer)
    {
        // Add custom MySQL server options first
        var mysqlHost = databaseSettings.GetValue<string>("MySqlHost") ?? "localhost";
        var mysqlPort = databaseSettings.GetValue<int>("MySqlPort");
        if (mysqlPort == 0) mysqlPort = 3306;
        var databaseName = databaseSettings.GetValue<string>("DatabaseName") ?? "hrms_database";
        var username = databaseSettings.GetValue<string>("Username") ?? "root";
        var password = databaseSettings.GetValue<string>("Password") ?? "";
        
        var customConnectionString = $"Server={mysqlHost};Port={mysqlPort};Database={databaseName};Uid={username};Pwd={password};Connect Timeout=30;Command Timeout=30;";
        connectionStringsToTest.Add(("Custom MySQL Server", customConnectionString));
        
        var customMySqlConnectionString = configuration.GetConnectionString("CustomMySql");
        if (!string.IsNullOrEmpty(customMySqlConnectionString))
        {
            // Replace placeholders with actual values
            customMySqlConnectionString = customMySqlConnectionString
                .Replace("YOUR_MYSQL_HOST", mysqlHost)
                .Replace("YOUR_USERNAME", username)
                .Replace("YOUR_PASSWORD", password);
            connectionStringsToTest.Add(("Custom MySQL (Config)", customMySqlConnectionString));
        }
    }
    
    // Add local XAMPP options as default
    var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
    var xamppConnectionString = configuration.GetConnectionString("XamppConnection");
    
    if (!string.IsNullOrEmpty(xamppConnectionString))
        connectionStringsToTest.Add(("XAMPP MySQL", xamppConnectionString));
    
    if (!string.IsNullOrEmpty(defaultConnectionString))
        connectionStringsToTest.Add(("Default MySQL", defaultConnectionString));
    
    // Test each connection string in order
    foreach (var (name, connectionString) in connectionStringsToTest)
    {
        try
        {
            Console.WriteLine($"?? Testing connection: {name}");
            Console.WriteLine($"    Connection string: {connectionString.Replace("Pwd=", "Pwd=***")}");
            
            var optionsBuilder = new DbContextOptionsBuilder<HrmsDbContext>();
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));
            
            using var testContext = new HrmsDbContext(optionsBuilder.Options);
            
            // Try to connect with a reasonable timeout
            using var connection = testContext.Database.GetDbConnection();
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await connection.OpenAsync(cancellationTokenSource.Token);
            
            // Test if we can query the database
            var canConnect = await testContext.Database.CanConnectAsync();
            if (canConnect)
            {
                Console.WriteLine($"? Successfully connected using: {name}");
                await connection.CloseAsync();
                return connectionString;
            }
            else
            {
                Console.WriteLine($"? Connection test failed for: {name}");
                await connection.CloseAsync();
            }
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine($"?? Connection timeout for {name}: {ex.Message}");
        }
        catch (MySqlConnector.MySqlException ex)
        {
            Console.WriteLine($"?? MySQL error for {name}: {ex.Message} (Error Code: {ex.ErrorCode})");
            if (ex.ErrorCode == MySqlConnector.MySqlErrorCode.AccessDenied)
            {
                Console.WriteLine("    ?? Hint: Check username/password credentials");
            }
            else if (ex.ErrorCode == MySqlConnector.MySqlErrorCode.UnknownDatabase)
            {
                Console.WriteLine("    ?? Hint: Database 'hrms_database' doesn't exist - it will be created");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Failed to connect using {name}: {ex.GetType().Name} - {ex.Message}");
        }
    }
    
    // If all connections fail, return the first available connection string and let normal error handling take over
    Console.WriteLine("?? All connection attempts failed. Using first available connection string.");
    Console.WriteLine("?? Please check:");
    Console.WriteLine("   - MySQL server is running on 192.168.0.114:3306");
    Console.WriteLine("   - Network connectivity to the MySQL server");
    Console.WriteLine("   - MySQL user 'root' has access from your IP");
    Console.WriteLine("   - MySQL server accepts connections from external IPs");
    
    return connectionStringsToTest.FirstOrDefault().connectionString ?? defaultConnectionString ?? xamppConnectionString ?? "";
}

static async Task MigrateDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<HrmsDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("??? Starting database migration...");
        
        // Drop and recreate database to ensure proper schema
        logger.LogWarning("?? Dropping existing database to ensure schema compatibility...");
        await context.Database.EnsureDeletedAsync();
        
        logger.LogInformation("?? Creating database with updated schema...");
        await context.Database.EnsureCreatedAsync();
        
        // Create admin user
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@hrms.com");
        if (adminUser == null)
        {
            logger.LogInformation("?? Creating admin user...");
            var newAdminUser = new User
            {
                Email = "admin@hrms.com",
                Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FirstName = "System",
                LastName = "Administrator",
                Role = "Admin",
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            
            context.Users.Add(newAdminUser);
            await context.SaveChangesAsync();
            logger.LogInformation("? Admin user created successfully.");
        }
        else
        {
            adminUser.Password = BCrypt.Net.BCrypt.HashPassword("admin123");
            adminUser.IsActive = true;
            await context.SaveChangesAsync();
            logger.LogInformation("? Admin user verified and updated.");
        }
        
        // Create demo employee users
        logger.LogInformation("?? Creating demo employee users...");
        
        // Check existing employees to avoid conflicts
        var existingEmployees = await context.Employees.ToListAsync();
        var existingUsers = await context.Users.ToListAsync();
        
        logger.LogInformation($"Found {existingEmployees.Count} existing employees");
        logger.LogInformation($"Found {existingUsers.Count} existing users");

        // Create user accounts for existing employees that don't have login accounts
        var employeesToCreateUsers = new List<(Employee employee, string password)>
        {
            (existingEmployees.FirstOrDefault(e => e.Email == "john.doe@hrms.com"), "john123"),
            (existingEmployees.FirstOrDefault(e => e.Email == "jane.smith@hrms.com"), "jane123"),
            (existingEmployees.FirstOrDefault(e => e.Email == "mike.johnson@hrms.com"), "mike123")
        };

        foreach (var (employee, password) in employeesToCreateUsers)
        {
            if (employee != null)
            {
                // Check if user already exists
                var existingUser = existingUsers.FirstOrDefault(u => u.Email.ToLower() == employee.Email.ToLower());
                if (existingUser == null)
                {
                    // Update employee to have complete profile
                    employee.IsRegistrationComplete = true;
                    employee.Address = employee.Department == "IT" ? "123 Tech Street, Bangalore, Karnataka, 560001" :
                                    employee.Department == "HR" ? "456 HR Avenue, Mumbai, Maharashtra, 400001" :
                                    "789 Finance Plaza, Delhi, Delhi, 110001";
                    employee.EmergencyContactName = $"{employee.FirstName} Family";
                    employee.EmergencyContactPhone = "+91-9876543200";
                    employee.DateOfBirth = new DateTime(1990, 1, 1);
                    employee.Gender = employee.FirstName == "Jane" ? "Female" : "Male";
                    employee.Nationality = "Indian";

                    // Create user account
                    var newUser = new User
                    {
                        Email = employee.Email,
                        Password = BCrypt.Net.BCrypt.HashPassword(password),
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        Role = "Employee",
                        CreatedDate = DateTime.Now,
                        IsActive = true
                    };
                    
                    context.Users.Add(newUser);
                    logger.LogInformation($"? Created user account for: {employee.FirstName} {employee.LastName}");
                }
                else
                {
                    logger.LogInformation($"User already exists for: {employee.FirstName} {employee.LastName}");
                }
            }
        }

        // Add one additional demo employee with user account
        var additionalEmployee = new Employee
        {
            EmployeeCode = "OIPAPA20240001",
            FirstName = "Priya",
            LastName = "Patel",
            Email = "priya.patel@hrms.com",
            Phone = "+91-9876543216",
            Department = "Marketing",
            Position = "Marketing Specialist",
            HireDate = new DateTime(2024, 1, 5),
            Salary = 55000,
            Status = "Active",
            IsRegistrationComplete = true,
            Address = "321 Marketing Hub, Pune, Maharashtra, 411001",
            EmergencyContactName = "Raj Patel",
            EmergencyContactPhone = "+91-9876543217",
            DateOfBirth = new DateTime(1995, 7, 12),
            Gender = "Female",
            Nationality = "Indian",
            CreatedDate = DateTime.Now
        };

        var additionalUser = new User
        {
            Email = "priya.patel@hrms.com",
            Password = BCrypt.Net.BCrypt.HashPassword("priya123"),
            FirstName = "Priya",
            LastName = "Patel",
            Role = "Employee",
            CreatedDate = DateTime.Now,
            IsActive = true
        };

        // Check if this employee doesn't already exist
        var existingPriya = await context.Employees.FirstOrDefaultAsync(e => e.Email == "priya.patel@hrms.com");
        if (existingPriya == null)
        {
            context.Employees.Add(additionalEmployee);
            context.Users.Add(additionalUser);
            logger.LogInformation("? Created additional demo employee: Priya Patel");
        }

        await context.SaveChangesAsync();
        
        logger.LogInformation("? Demo employees and users created successfully!");
        logger.LogInformation("");
        logger.LogInformation("?? DEMO LOGIN CREDENTIALS:");
        logger.LogInformation("?? ==========================================");
        logger.LogInformation("?? ADMIN LOGIN:");
        logger.LogInformation("   ?? Email: admin@hrms.com");
        logger.LogInformation("   ?? Password: admin123");
        logger.LogInformation("");
        logger.LogInformation("?? EMPLOYEE LOGINS:");
        logger.LogInformation("   ?? john.doe@hrms.com / ?? john123 (Software Developer)");
        logger.LogInformation("   ?? jane.smith@hrms.com / ?? jane123 (HR Manager)");
        logger.LogInformation("   ?? mike.johnson@hrms.com / ?? mike123 (Finance Analyst)");
        logger.LogInformation("   ?? priya.patel@hrms.com / ?? priya123 (Marketing Specialist)");
        logger.LogInformation("?? ==========================================");
        logger.LogInformation("");
        logger.LogInformation("?? Database migration completed successfully.");
        logger.LogInformation("?? All users are ready to login!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "?? An error occurred during database migration: {ErrorMessage}", ex.Message);
        logger.LogWarning("?? Continuing startup despite database migration issues...");
        logger.LogWarning("?? Please ensure:");
        logger.LogWarning("   - MySQL server is running");
        logger.LogWarning("   - Database connection string is correct");
        logger.LogWarning("   - Network connectivity is available");
    }
}
