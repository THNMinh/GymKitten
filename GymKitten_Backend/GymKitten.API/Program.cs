using GymKitten.Api.Middleware;
using GymKitten.API.Hangfire;
using GymKitten.API.Hubs;
using GymKitten.API.Middleware;
using GymKitten.API.Services;
using GymKitten.Application;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Infrastructure;
using Hangfire;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog Host
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

// Add services to the container.
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Swagger with Bearer Auth
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GymKitten API",
        Version = "v1",
        Description = "GymKitten Backend API"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
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
            Array.Empty<string>()
        }
    });
});

// Register Application & Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// SignalR Realtime Services
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationHubService, NotificationHubService>();

// Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Custom Request Context Logging & Serilog Request Logging
app.UseRequestContextLogging();
app.UseSerilogRequestLogging();

// Enable Swagger documentation UI in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GymKitten API v1");
    c.RoutePrefix = "swagger";
});

// Root welcome & health status endpoint
app.MapGet("/", () => Results.Ok(new
{
    service = "GymKitten API",
    status = "Online",
    version = "v1.0.0",
    documentation = "/swagger",
    hangfire = "/hangfire",
    serverTime = DateTime.UtcNow
}));

app.UseExceptionHandler();

app.UseCors("frontend");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard UI
var hangfireUser = builder.Configuration["Hangfire:Username"] ?? "admin";
var hangfirePass = builder.Configuration["Hangfire:Password"] ?? "admin";

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[]
    {
        new HangfireAuthorizationFilter(hangfireUser, hangfirePass)
    },
    DashboardTitle = "GymKitten - Hangfire Dashboard",
    DisplayStorageConnectionString = false
});

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification");

app.Run();
