using backend.Database;
using DRCS.Extensions;
using DRCS.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllersWithViews();

// Register DbContext with PostgreSQL
builder.Services.AddDbContext<DrcsContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DrcsDb")));

// Register services
builder.Services.AddApplicationServices();

// --- CORS configuration for frontend ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:7291") 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // important for cookies
    });
});

// JWT Authentication setup
var jwtSecretKey = builder.Configuration["JWT:Secret"] ?? "ThisIsASecretKeyForDemoOnly!";
var key = Encoding.ASCII.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
});
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = false,
//        ValidateAudience = false,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        IssuerSigningKey = new SymmetricSecurityKey(key),
//        ClockSkew = TimeSpan.FromMinutes(1), // small leeway to avoid immediate expiry issues
//        RoleClaimType = "role",   // <--- map your 'role' claim
//        NameClaimType = "userId"  // optional, maps your userId
//    };
//    options.Events = new JwtBearerEvents
//    {
//        OnMessageReceived = context =>
//        {
//            if (context.Request.Cookies.ContainsKey("access_token"))
//            {
//                context.Token = context.Request.Cookies["access_token"];
//            }
//            return Task.CompletedTask;
//        }
//    };
//});

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

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token **_only_"
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

var app = builder.Build();

// --- Middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "DRCS API v1");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// CORS must come BEFORE authentication & custom middleware
app.UseCors("AllowFrontend");
app.UseAuthMiddleware(); // custom middleware sets HttpContext.User from cookie

app.UseAuthentication();               // JWT Authentication
//app.UseMiddleware<AuthMiddleware>();   // Custom middleware for cookie JWT validation
app.UseAuthorization();

// Map controllers
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
