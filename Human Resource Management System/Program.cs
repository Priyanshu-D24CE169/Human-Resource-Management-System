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

// Determine the appropriate connection string
var connectionString = await GetWorkingConnectionStringAsync(builder.Configuration);

// Add Entity Framework with MySQL using Pomelo provider
builder.Services.AddDbContext<HrmsDbContext>(options =>
{
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));
});

// Add services
builder.Services.AddScoped<IUserService, UserService>();

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
        
        var customConnectionString = $"Server={mysqlHost};Port={mysqlPort};Database={databaseName};Uid={username};Pwd={password};";
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
            var optionsBuilder = new DbContextOptionsBuilder<HrmsDbContext>();
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));
            
            using var testContext = new HrmsDbContext(optionsBuilder.Options);
            
            // Try to connect with a reasonable timeout
            using var connection = testContext.Database.GetDbConnection();
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await connection.OpenAsync(cancellationTokenSource.Token);
            await connection.CloseAsync();
            
            Console.WriteLine($"? Successfully connected using: {name}");
            return connectionString;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Failed to connect using {name}: {ex.GetType().Name} - {ex.Message}");
        }
    }
    
    // If all connections fail, return the first available connection string and let normal error handling take over
    Console.WriteLine("? All connection attempts failed. Using first available connection string.");
    return connectionStringsToTest.FirstOrDefault().connectionString ?? defaultConnectionString ?? xamppConnectionString ?? "";
}

static async Task MigrateDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<HrmsDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Starting database migration...");
        
        // Check if database exists and create if needed
        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogInformation("Database does not exist. Creating database...");
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database created successfully.");
        }
        else
        {
            logger.LogInformation("Database connection successful.");
            
            try
            {
                // Try to apply pending migrations
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying pending migrations...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully.");
                }
            }
            catch (Exception migrationEx)
            {
                logger.LogWarning(migrationEx, "Migration failed, trying EnsureCreated instead...");
                
                // If migration fails (e.g., tables already exist), try to ensure database is created
                try
                {
                    await context.Database.EnsureCreatedAsync();
                    logger.LogInformation("Database ensured successfully.");
                }
                catch (Exception ensureEx)
                {
                    logger.LogWarning(ensureEx, "EnsureCreated also failed, but continuing...");
                    // Continue anyway as tables might already exist
                }
            }
        }
        
        // Ensure admin user exists
        var userExists = await context.Users.AnyAsync();
        if (!userExists)
        {
            logger.LogInformation("Seeding default admin user...");
            var adminUser = new User
            {
                Email = "admin@hrms.com",
                Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                FirstName = "System",
                LastName = "Administrator",
                Role = "Admin",
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
            logger.LogInformation("Default admin user created successfully.");
        }
        else
        {
            logger.LogInformation("Users table already contains data.");
            
            // Check if admin user exists
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@hrms.com");
            if (adminUser == null)
            {
                logger.LogInformation("Creating missing admin user...");
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
                logger.LogInformation("Admin user created successfully.");
            }
            else
            {
                logger.LogInformation("Admin user already exists.");
            }
        }
        
        logger.LogInformation("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration: {ErrorMessage}", ex.Message);
        
        // Don't throw the exception, just log it and continue
        logger.LogWarning("Continuing startup despite database migration issues...");
    }
}
