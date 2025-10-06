using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReservasApi.Data;
using ReservasApi.Data.Repositories;
using ReservasApi.Data.Repositories.Interfaces;
using ReservasApi.Services;
using ReservasApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("ConnectionDataBase");
builder.Services.AddHttpContextAccessor();

// Dependency Injection
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
builder.Services.AddScoped<IEntregableRepository, EntregableRepository>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<IEntregableService, EntregableService>();
builder.Services.AddScoped<IUserApiService, UserApiService>();
builder.Services.AddHttpClient<IUserApiService, UserApiService>();

// Database Context
builder.Services.AddDbContext<ReservasDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// JWT Configuration - Siguiendo el patrón de UsersApi
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new ArgumentNullException("JWT SecretKey is required");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database Migration on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ReservasDbContext>();
    try
    {
        context.Database.Migrate();
        Console.WriteLine("Database migrated successfully - ReservasApi");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
    }
}

Console.WriteLine("ReservasApi starting on http://localhost:5004");

app.Run();