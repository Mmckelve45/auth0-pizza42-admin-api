using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PizzaAdminApi.Data;
using PizzaAdminApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Configure PostgreSQL database
builder.Services.AddDbContext<PizzaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                  "http://localhost:3000",           // React dev server (CRA)
                  "http://localhost:3002",           // React dev server (alt port)
                  "http://localhost:5173",           // React dev server (Vite)
                  "https://auth0-pizza42.vercel.app" // Production Vercel app
              )
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure Authentication & Authorization
var domain = $"https://{builder.Configuration["Auth0:Domain"]}/";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = domain;
        options.Audience = builder.Configuration["Auth0:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "name",
            RoleClaimType = "https://pizza42.com/role"
        };
    });

// Middleware to check if the user has the Employee role
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmployeeOnly", policy =>
        policy.RequireClaim("https://pizza42.com/role", "Employee"));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pizza42 Admin API",
        Version = "v1",
        Description = "Admin API for managing pizzas and orders with Auth0 authentication"
    });

    // Add JWT Bearer authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token from Auth0"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in all environments (for internal admin API)
app.UseSwagger();
app.UseSwaggerUI();

// Conditional HTTPS Redirection
// AWS Gateway sends requests via HTTP to backend (port 8080), but we want
// direct access to force HTTPS. This middleware:
// 1. Always allows HTTP on port 8080 (AWS Gateway backend port)
// 2. Redirects other HTTP requests to HTTPS on port 443 (direct access)
// This gives us security for direct access while allowing Gateway's HTTP backend
app.Use(async (context, next) =>
{
    // Port 8080 is the backend port for AWS Gateway - always allow HTTP
    var isBackendPort = context.Request.Host.Port == 8080;

    // If not backend port and not HTTPS, redirect to HTTPS on port 443
    if (!isBackendPort && !context.Request.IsHttps)
    {
        // Build HTTPS URL on standard port 443
        var httpsUrl = $"https://{context.Request.Host.Host}{context.Request.Path}{context.Request.QueryString}";
        context.Response.Redirect(httpsUrl, permanent: false);
        return;
    }

    // Allow the request to continue (backend port or already HTTPS)
    await next();
});

// Enable CORS
app.UseCors("AllowReactApp");

// Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// ===== Pizza Endpoints =====

// GET all pizzas (Private - Any logged in user can view)
app.MapGet("/api/pizzas", async (PizzaDbContext db) =>
{
    return Results.Ok(await db.Pizzas.ToListAsync());
})
.WithName("GetAllPizzas")
.WithOpenApi()
.RequireAuthorization();
// .AllowAnonymous();

// GET a specific pizza by ID (Private - Any logged in user can view)
app.MapGet("/api/pizzas/{id}", async (int id, PizzaDbContext db) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    return pizza is not null ? Results.Ok(pizza) : Results.NotFound(new { message = "Pizza not found" });
})
.WithName("GetPizzaById")
.WithOpenApi()
.RequireAuthorization();
// .AllowAnonymous();

// PUT update pizza price (Employee only)
app.MapPut("/api/pizzas/{id}/price", async (int id, decimal newPrice, PizzaDbContext db) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null)
    {
        return Results.NotFound(new { message = "Pizza not found" });
    }

    pizza.UnitPrice = newPrice;
    pizza.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(pizza);
})
.WithName("UpdatePizzaPrice")
.WithOpenApi()
.RequireAuthorization("EmployeeOnly");

// PUT update pizza sold out status (Employee only)
app.MapPut("/api/pizzas/{id}/soldout", async (int id, bool soldOut, PizzaDbContext db) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null)
    {
        return Results.NotFound(new { message = "Pizza not found" });
    }

    pizza.SoldOut = soldOut;
    pizza.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(pizza);
})
.WithName("UpdatePizzaSoldOutStatus")
.WithOpenApi()
.RequireAuthorization("EmployeeOnly");

// ===== Order Endpoints =====

// GET most recent 25 orders (Employee only)
app.MapGet("/api/orders", async (PizzaDbContext db) =>
{
    var orders = await db.Orders
        .OrderByDescending(o => o.CreatedAt)
        .Take(25)
        .ToListAsync();
    return Results.Ok(orders);
})
.WithName("GetRecentOrders")
.WithOpenApi()
.RequireAuthorization("EmployeeOnly");

// GET a specific order by ID (Employee only)
app.MapGet("/api/orders/{id}", async (string id, PizzaDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);
    return order is not null ? Results.Ok(order) : Results.NotFound(new { message = "Order not found" });
})
.WithName("GetOrderById")
.WithOpenApi()
.RequireAuthorization("EmployeeOnly");

// PUT update order status (Employee only)
app.MapPut("/api/orders/{id}/status", async (string id, string newStatus, PizzaDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order is null)
    {
        return Results.NotFound(new { message = "Order not found" });
    }

    order.Status = newStatus;
    order.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(order);
})
.WithName("UpdateOrderStatus")
.WithOpenApi()
.RequireAuthorization("EmployeeOnly");

// PUT update order priority (Employee only)
app.MapPut("/api/orders/{id}/priority", async (string id, bool priority, PizzaDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order is null)
    {
        return Results.NotFound(new { message = "Order not found" });
    }

    order.Priority = priority;
    order.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(order);
})
.WithName("UpdateOrderPriority")
.WithOpenApi()
.RequireAuthorization("EmployeeOnly");

app.Run();
