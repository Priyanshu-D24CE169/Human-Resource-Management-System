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
    var databaseSettings = configuration.GetSection("DatabaseSettings");
    var useIntranetServer = databaseSettings.GetValue<bool>("UseIntranetServer");
    
    // Priority order for connection strings
    var connectionStringsToTest = new List<(string name, string connectionString)>();
    
    if (useIntranetServer)
    {
        // Add intranet server options first
        var preferHostname = databaseSettings.GetValue<bool>("PreferHostnameOverIP");
        var sqlServerIP = databaseSettings.GetValue<string>("SqlServerIP");
        var sqlServerHostname = databaseSettings.GetValue<string>("SqlServerHostname");
        var sqlUsername = databaseSettings.GetValue<string>("SqlUsername");
        var sqlPassword = databaseSettings.GetValue<string>("SqlPassword");
        
        if (preferHostname && !string.IsNullOrEmpty(sqlServerHostname))
        {
            var hostnameConnectionString = configuration.GetConnectionString("IntranetSqlServerHostname");
            if (!string.IsNullOrEmpty(hostnameConnectionString))
            {
                // Replace placeholders with actual values
                hostnameConnectionString = hostnameConnectionString
                    .Replace("YOUR_HOSTNAME", sqlServerHostname)
                    .Replace("YOUR_SQL_USERNAME", sqlUsername)
                    .Replace("YOUR_SQL_PASSWORD", sqlPassword);
                connectionStringsToTest.Add(("Intranet SQL Server (Hostname)", hostnameConnectionString));
            }
        }
        
        if (!string.IsNullOrEmpty(sqlServerIP))
        {
            var ipConnectionString = configuration.GetConnectionString("IntranetSqlServerIP");
            if (!string.IsNullOrEmpty(ipConnectionString))
            {
                // Replace placeholders with actual values
                ipConnectionString = ipConnectionString
                    .Replace("YOUR_IP_ADDRESS", sqlServerIP)
                    .Replace("YOUR_SQL_USERNAME", sqlUsername)
                    .Replace("YOUR_SQL_PASSWORD", sqlPassword);
                connectionStringsToTest.Add(("Intranet SQL Server (IP)", ipConnectionString));
            }
        }
        
        if (!preferHostname && !string.IsNullOrEmpty(sqlServerHostname))
        {
            var hostnameConnectionString = configuration.GetConnectionString("IntranetSqlServerHostname");
            if (!string.IsNullOrEmpty(hostnameConnectionString))
            {
                // Replace placeholders with actual values
                hostnameConnectionString = hostnameConnectionString
                    .Replace("YOUR_HOSTNAME", sqlServerHostname)
                    .Replace("YOUR_SQL_USERNAME", sqlUsername)
                    .Replace("YOUR_SQL_PASSWORD", sqlPassword);
                connectionStringsToTest.Add(("Intranet SQL Server (Hostname)", hostnameConnectionString));
            }
        }
    }
    
    // Add local options as fallback
    var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");
    var localDbConnectionString = configuration.GetConnectionString("LocalDbConnection");
    
    if (!string.IsNullOrEmpty(defaultConnectionString))
        connectionStringsToTest.Add(("Local SQL Server Express", defaultConnectionString));
    
    if (!string.IsNullOrEmpty(localDbConnectionString))
        connectionStringsToTest.Add(("Local DB", localDbConnectionString));
    
    // Test each connection string in order
    foreach (var (name, connectionString) in connectionStringsToTest)
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<HrmsDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            
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
    return connectionStringsToTest.FirstOrDefault().connectionString ?? defaultConnectionString ?? localDbConnectionString ?? "";
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
