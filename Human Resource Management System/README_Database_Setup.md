# HRMS Database Setup Instructions

## Prerequisites
1. Install SQL Server Express if not already installed
2. Install SQL Server Management Studio (SSMS)

## Database Setup Steps

### 1. Restore NuGet Packages
Run the following command in Package Manager Console or terminal:
```
dotnet restore
```

### 2. Build the Project
```
dotnet build
```

### 3. Run the Application
```
dotnet run
```

The application will automatically create the database on first run.

### 4. Connect to Database via SSMS
- Server name: `.\SQLEXPRESS` or `(local)\SQLEXPRESS`
- Authentication: Windows Authentication
- Database: `HrmsDatabase`

### 5. Default Login Credentials
- Email: `admin@hrms.com`
- Password: `admin123`

## Database Connection Strings

### For Local SQL Server Express (recommended for sharing)
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=HrmsDatabase;Integrated Security=true;TrustServerCertificate=true;"
```

### For LocalDB (development only)
```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=HrmsDatabase;Trusted_Connection=true;TrustServerCertificate=true;"
```

## Manual Database Creation (if needed)

If automatic database creation fails, you can create the database manually:

1. Open SSMS
2. Connect to your SQL Server instance
3. Right-click on "Databases" ? "New Database"
4. Name: `HrmsDatabase`
5. Click OK

Then run the application again.

## Troubleshooting

### Connection Issues
- Ensure SQL Server Express is running
- Check Windows Services for "SQL Server (SQLEXPRESS)"
- Verify Windows Authentication is enabled

### Permission Issues
- Run Visual Studio as Administrator
- Ensure your Windows user has access to SQL Server

## Features Implemented
- User authentication with BCrypt password hashing
- Session management
- Database integration with Entity Framework Core
- Admin user seeding
- Base controller for authentication checks