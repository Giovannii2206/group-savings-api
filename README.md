# Group Savings API

This is a complete ASP.NET Core Web API for group savings and contributions, supporting CRUD operations for users, members, groups, sessions, contributions, notifications, and more.

## Features
- User registration and management
- Member and group management
- Payment methods and account types
- Group sessions and contributions
- Notifications
- Role management
- Complete RESTful API endpoints

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (or LocalDB)

### Setup
1. Restore dependencies:
   ```bash
   dotnet restore
   ```
2. Create the database and apply migrations:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
3. Run the API:
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:5001` or `http://localhost:5000` by default. Swagger UI will be enabled in development mode at `/swagger`.

## Project Structure
- `Models/` - Entity models
- `DTOs/` - Data transfer objects for API
- `Controllers/` - API endpoints
- `Data/` - Entity Framework DbContext

## Endpoints
See Swagger UI for full API documentation.

---

© 2025 Group Savings API
