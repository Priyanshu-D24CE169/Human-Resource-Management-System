using Human_Resource_Management_System.Data;
using Human_Resource_Management_System.Services;
using Human_Resource_Management_System.Models;
using Microsoft.EntityFrameworkCore;
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

// Add Entity Framework with retry policy using the working connection string
builder.Services.AddDbContext<HrmsDbContext>(options =>
{
    options.UseSqlServer(connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
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
    var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
    var localDbConnectionString = configuration.GetConnectionString("LocalDbConnection");
    
    // Test SQL Server Express connection first
    try
    {
        var optionsBuilder = new DbContextOptionsBuilder<HrmsDbContext>();
        optionsBuilder.UseSqlServer(defaultConnectionString);
        
        using var testContext = new HrmsDbContext(optionsBuilder.Options);
        
        // Try to connect with a short timeout
        using var connection = testContext.Database.GetDbConnection();
        await connection.OpenAsync();
        await connection.CloseAsync();
        
        Console.WriteLine("Using SQL Server Express connection.");
        return defaultConnectionString;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"SQL Server Express not available ({ex.GetType().Name}). Falling back to LocalDB.");
        
        // Test LocalDB connection
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<HrmsDbContext>();
            optionsBuilder.UseSqlServer(localDbConnectionString);
            
            using var testContext = new HrmsDbContext(optionsBuilder.Options);
            using var connection = testContext.Database.GetDbConnection();
            await connection.OpenAsync();
            await connection.CloseAsync();
            
            Console.WriteLine("LocalDB connection successful.");
            return localDbConnectionString;
        }
        catch (Exception localEx)
        {
            Console.WriteLine($"LocalDB also failed: {localEx.Message}");
            // Fall back to original connection string and let normal error handling take over
            return defaultConnectionString;
        }
    }
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
        if (!await context.Database.CanConnectAsync())
        {
            logger.LogInformation("Database does not exist. Creating database...");
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database created successfully.");
        }
        else
        {
            logger.LogInformation("Database connection successful.");
        }
        
        // Ensure admin user exists
        if (!context.Users.Any())
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
        
        logger.LogInformation("Database migration completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration: {ErrorMessage}", ex.Message);
        throw; // Re-throw to prevent application startup with broken database
    }
}
