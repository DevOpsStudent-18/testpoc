using EDF.Api.Repositories;
using EDF.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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

var azureAdInstance = builder.Configuration["AzureAd:Instance"] ?? "https://login.microsoftonline.com/";
var azureAdTenantId = builder.Configuration["AzureAd:TenantId"];
var azureAdClientId = builder.Configuration["AzureAd:ClientId"];
var azureAdAudience = builder.Configuration["AzureAd:Audience"];
var azureAdScope = builder.Configuration["AzureAd:Scope"];
var swaggerClientId = builder.Configuration["AzureAd:SwaggerClientId"];
string? resolvedAudience = null;
string? resolvedScope = null;

if (!isDevelopment && (string.IsNullOrWhiteSpace(azureAdTenantId) || string.IsNullOrWhiteSpace(azureAdClientId)))
{
    throw new InvalidOperationException("Missing required auth configuration: AzureAd__TenantId and AzureAd__ClientId");
}

var authBuilder = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);
if (!string.IsNullOrWhiteSpace(azureAdTenantId) && !string.IsNullOrWhiteSpace(azureAdClientId))
{
    var authority = $"{azureAdInstance.TrimEnd('/')}/{azureAdTenantId}/v2.0";
    resolvedAudience = string.IsNullOrWhiteSpace(azureAdAudience)
        ? $"api://{azureAdClientId}"
        : azureAdAudience;
    resolvedScope = string.IsNullOrWhiteSpace(azureAdScope)
        ? $"{resolvedAudience.TrimEnd('/')}/access_as_user"
        : azureAdScope;

    authBuilder.AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidAudience = resolvedAudience,
                ValidateAudience = true
            };
        });

    builder.Services.AddSwaggerGen(options =>
    {
        var oauthScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{azureAdInstance.TrimEnd('/')}/{azureAdTenantId}/oauth2/v2.0/authorize"),
                    TokenUrl = new Uri($"{azureAdInstance.TrimEnd('/')}/{azureAdTenantId}/oauth2/v2.0/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { resolvedScope, "Access EDF API" }
                    }
                }
            }
        };

        options.AddSecurityDefinition("oauth2", oauthScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            { oauthScheme, new[] { resolvedScope } }
        });
    });
}
else
{
    authBuilder.AddJwtBearer();
    builder.Services.AddSwaggerGen();
}

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
    app.UseSwaggerUI(options =>
    {
        if (!string.IsNullOrWhiteSpace(swaggerClientId) && !string.IsNullOrWhiteSpace(resolvedScope))
        {
            options.OAuthClientId(swaggerClientId);
            options.OAuthScopes(resolvedScope);
            options.OAuthUsePkce();
        }
    });
}

app.UseHttpsRedirection();
app.UseCors("FrontendCors");
app.UseAuthentication();
app.UseAuthorization();

if (isDevelopment)
{
    app.MapControllers();
}
else
{
    app.MapControllers().RequireAuthorization();
}
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

app.Run();
