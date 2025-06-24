using FluentValidation;
using MediatR;
using FlightTracker.Api.Application.Behaviors;
using FlightTracker.Api.Configuration;
using FlightTracker.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure URLs explicitly for development
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.UseUrls("https://localhost:7120", "http://localhost:5243");
}

// Add service defaults & Aspire client integrations
builder.AddServiceDefaults();

// Add infrastructure services
builder.Services.AddInfrastructure(builder.Environment);

// Add MediatR for CQRS
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    
    // Add pipeline behaviors
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Add controllers
builder.Services.AddControllers();

// Add CORS policy for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",  // Next.js default
                "http://localhost:3001",  // Alternative Next.js port
                "https://localhost:3000", // HTTPS variant
                "https://localhost:3001"  // HTTPS variant
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
    
    // Allow all origins in development (less secure but more flexible)
    if (builder.Environment.IsDevelopment())
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
});

// Configure HTTPS redirection options
builder.Services.AddHttpsRedirection(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.HttpsPort = 7120; // HTTPS port from launchSettings.json
    }
});

// Add OpenAPI documentation and Swagger
builder.Services.AddOpenApiDocumentation();

// Legacy weather forecast endpoint (remove in production)
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapDefaultEndpoints();

// Configure OpenAPI documentation and Swagger UI
app.UseOpenApiDocumentation();

// Enable CORS
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll"); // More permissive for development
}
else
{
    app.UseCors("AllowFrontend"); // Restrictive for production
}

// Enable static files for custom CSS
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Configure HTTPS redirection based on environment
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
else if (app.Environment.IsDevelopment())
{
    // In development, make HTTPS redirection optional to avoid port conflicts
    // You can access the API via both HTTP and HTTPS
    // Comment out the next line if you want to disable HTTPS redirection in development
    app.UseHttpsRedirection();
}

// Map controllers
app.MapControllers();

// Add a simple redirect from root to Swagger docs in development
if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/api/docs"))
        .WithName("RedirectToSwagger")
        .ExcludeFromDescription();
    
    app.MapGet("/docs", () => Results.Redirect("/api/docs"))
        .WithName("RedirectToSwaggerShort")
        .ExcludeFromDescription();
}

app.Run();

// Make Program class accessible for testing
public partial class Program { }
