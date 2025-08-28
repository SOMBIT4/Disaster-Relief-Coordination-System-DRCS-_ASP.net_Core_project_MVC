using backend.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();

// Register DbContext with PostgreSQL
builder.Services.AddDbContext<DrcsContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DrcsDb")));

// Configure Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "DRCS API",
        Description = "API Documentation for DRCS project"
    });

    // Optional: include XML comments if available
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Middleware order
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DRCS API v1");
        // Keep default RoutePrefix so /swagger/index.html works
    });
}

// Keep HTTPS redirection optional but compatible
app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

app.Run();
