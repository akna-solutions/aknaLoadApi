using AknaLoad.Application.Services;
using AknaLoad.Domain.Interfaces.Repositories;
using AknaLoad.Domain.Interfaces.Services;
using AknaLoad.Domain.Interfaces.UnitOfWorks;
using AknaLoad.Infrastructure.Persistence;
using AknaLoad.Infrastructure.Repositories;
using AknaLoad.Infrastructure.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Database Configuration
builder.Services.AddDbContext<AknaLoadDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<ILoadRepository, LoadRepository>();
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IPricingCalculationRepository, PricingCalculationRepository>();

// Business Services
builder.Services.AddScoped<ILoadService, LoadService>();
// builder.Services.AddScoped<IMatchingService, MatchingService>(); // Not implemented yet
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<PricingService>(); // Concrete type for controller
builder.Services.AddScoped<IGeminiAIService, GeminiAIService>(); // AI service for pricing and vehicle matching
// builder.Services.AddScoped<ITrackingService, TrackingService>(); // Will be implemented later

// External Services
// VehicleService not implemented yet, commenting out to prevent runtime errors
// builder.Services.AddHttpClient<IVehicleService, VehicleService>(client =>
// {
//     var identityServiceBaseUrl = builder.Configuration["Services:IdentityService:BaseUrl"] ?? "https://localhost:7001";
//     client.BaseAddress = new Uri(identityServiceBaseUrl);
//     client.Timeout = TimeSpan.FromSeconds(30);
// });

// Controllers
// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Use exact property names
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Akna Load & Matching API",
        Version = "v1",
        Description = "API for load management, automatic matching, and dynamic pricing",
        Contact = new OpenApiContact
        {
            Name = "Akna Team",
            Email = "info@akna.com"
        }
    });

    // Include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add Authorization header
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AknaLoadDbContext>();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Akna Load & Matching API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at root
    });
}

// Global error handling middleware (must be first in pipeline)
app.UseMiddleware<GlobalErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

app.Run();

// Global Error Handling Middleware
public class GlobalErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalErrorHandlingMiddleware> _logger;

    public GlobalErrorHandlingMiddleware(RequestDelegate next, ILogger<GlobalErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            message = "An error occurred while processing your request",
            details = exception.Message,
            timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case ArgumentException:
                context.Response.StatusCode = 400;
                break;
            case UnauthorizedAccessException:
                context.Response.StatusCode = 401;
                break;
            case NotImplementedException:
                context.Response.StatusCode = 501;
                break;
            default:
                context.Response.StatusCode = 500;
                break;
        }

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
}