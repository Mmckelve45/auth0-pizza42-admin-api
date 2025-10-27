# Pizza42 Admin API

A .NET 8 Web API for managing pizza orders and menu items.

## Features

- **Pizza Management**: View and update pizza prices
- **Order Management**: View recent orders, update order status and priority
- **PostgreSQL Database**: Using Entity Framework Core with Npgsql
- **CORS Enabled**: Configured for React frontend integration
- **Swagger/OpenAPI**: Interactive API documentation

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL database (using Neon serverless PostgreSQL)

## Setup

### 1. Clone the repository

```bash
git clone <your-repo-url>
cd auth0-pizza42-admin-api
```

### 2. Configure Database Connection

Create `appsettings.Development.json` in the project root:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_HOST;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD;SSL Mode=Require"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

Replace the connection string values with your actual database credentials.

### 3. Restore Dependencies

```bash
dotnet restore
```

### 4. Run the Application

```bash
dotnet run
```

The API will start on:
- HTTP: `http://localhost:5041`
- Swagger UI: `http://localhost:5041/swagger`

## API Endpoints

### Pizza Endpoints

- `GET /api/pizzas` - Get all pizzas
- `GET /api/pizzas/{id}` - Get a specific pizza
- `PUT /api/pizzas/{id}/price?newPrice={price}` - Update pizza price

### Order Endpoints

- `GET /api/orders` - Get 25 most recent orders (sorted by creation date, newest first)
- `GET /api/orders/{id}` - Get a specific order
- `PUT /api/orders/{id}/status?newStatus={status}` - Update order status
- `PUT /api/orders/{id}/priority?priority={true|false}` - Update order priority

## CORS Configuration

The API is configured to accept requests from:
- `http://localhost:3000` (Create React App)
- `http://localhost:3002` (Custom port)
- `http://localhost:5173` (Vite)

To add more origins, update the CORS policy in `Program.cs`.

## Database Schema

The API expects the following tables:

- `pizzas` - Menu items with prices and details
- `orders` - Order records with status and metadata
- `users` - User information

## Technologies

- **.NET 8** - Web framework
- **ASP.NET Core Minimal API** - Endpoint routing
- **Entity Framework Core** - ORM
- **Npgsql** - PostgreSQL provider
- **Swagger/Swashbuckle** - API documentation

## Development

### Build

```bash
dotnet build
```

### Watch Mode (auto-reload)

```bash
dotnet watch run
```

## Security Notes

- `appsettings.Development.json` is gitignored and contains sensitive credentials
- Never commit real database credentials to the repository
- Use environment variables or Azure Key Vault for production secrets

## License

MIT
