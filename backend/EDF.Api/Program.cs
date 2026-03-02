using EDF.Api.Repositories;
using EDF.Api.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var allowedOriginsSetting = builder.Configuration["AllowedOrigins"] ?? (isDevelopment ? "http://localhost:4200" : string.Empty);
var allowedOrigins = allowedOriginsSetting
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

if (!isDevelopment && allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("Missing required configuration: AllowedOrigins");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<DeviceContext>(options =>
        options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

    builder.Services.AddScoped<IDeviceRepository, SqlDeviceRepository>();
}
else
{
    if (!isDevelopment)
    {
        throw new InvalidOperationException("Missing required configuration: ConnectionStrings__DefaultConnection");
    }

    builder.Services.AddSingleton<IDeviceRepository, InMemoryDeviceRepository>();
}

builder.Services.AddScoped<IDeviceService, DeviceService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetService<DeviceContext>();
    if (ctx != null)
    {
        var hasMigrations = ctx.Database.GetMigrations().Any();
        if (hasMigrations)
        {
            ctx.Database.Migrate();
        }
        else
        {
            ctx.Database.EnsureCreated();
        }
    }
}

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendCors");
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.Run();
