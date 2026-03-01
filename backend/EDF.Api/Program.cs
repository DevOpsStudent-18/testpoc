using EDF.Api.Repositories;
using EDF.Api.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// add EF Core context after configuration is available

// Configure forwarded headers to respect proxy headers (required for many PaaS/load-balanced setups)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
	options.KnownNetworks.Clear();
	options.KnownProxies.Clear();
});

// Allow CORS from configured frontend origins (use env var or default to localhost during development)
var allowedOrigins = builder.Configuration["AllowedOrigins"] ?? "http://localhost:4200";
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowLocalhost4200", policy =>
	{
		policy.WithOrigins(allowedOrigins.Split(';', StringSplitOptions.RemoveEmptyEntries))
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// register the device repository and optionally configure EF Core for SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    // add context and SQL-backed repository
    builder.Services.AddDbContext<DeviceContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services.AddScoped<IDeviceRepository, SqlDeviceRepository>();
}
else
{
    // no connection string configured; fallback to simple in‑memory store
    builder.Services.AddSingleton<IDeviceRepository, InMemoryDeviceRepository>();
}

builder.Services.AddScoped<IDeviceService, DeviceService>();

var app = builder.Build();

// if we registered a SQL context, ensure database is created/migrated
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetService<DeviceContext>();
    if (ctx != null)
    {
        ctx.Database.Migrate();
    }
}

// Use forwarded headers early in the pipeline so URL/scheme are correct behind proxies
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS before routing/authorization so controllers receive the CORS headers
app.UseCors("AllowLocalhost4200");

app.UseAuthorization();

app.MapControllers();

// simple health endpoint for orchestration/monitoring
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.Run();
