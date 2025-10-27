using Microsoft.EntityFrameworkCore;
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
        policy.WithOrigins("http://localhost:3000", "http://localhost:3002", "http://localhost:5173") // React dev servers (CRA and Vite)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowReactApp");

// ===== Pizza Endpoints =====

// GET all pizzas
app.MapGet("/api/pizzas", async (PizzaDbContext db) =>
{
    return Results.Ok(await db.Pizzas.ToListAsync());
})
.WithName("GetAllPizzas")
.WithOpenApi();

// GET a specific pizza by ID
app.MapGet("/api/pizzas/{id}", async (int id, PizzaDbContext db) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    return pizza is not null ? Results.Ok(pizza) : Results.NotFound(new { message = "Pizza not found" });
})
.WithName("GetPizzaById")
.WithOpenApi();

// PUT update pizza price
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
.WithOpenApi();

// ===== Order Endpoints =====

// GET most recent 25 orders
app.MapGet("/api/orders", async (PizzaDbContext db) =>
{
    var orders = await db.Orders
        .OrderByDescending(o => o.CreatedAt)
        .Take(25)
        .ToListAsync();
    return Results.Ok(orders);
})
.WithName("GetRecentOrders")
.WithOpenApi();

// GET a specific order by ID
app.MapGet("/api/orders/{id}", async (string id, PizzaDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);
    return order is not null ? Results.Ok(order) : Results.NotFound(new { message = "Order not found" });
})
.WithName("GetOrderById")
.WithOpenApi();

// PUT update order status
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
.WithOpenApi();

// PUT update order priority
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
.WithOpenApi();

app.Run();
