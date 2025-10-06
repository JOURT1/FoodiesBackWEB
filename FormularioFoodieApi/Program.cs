using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FormularioFoodieApi.Data;
using FormularioFoodieApi.Data.Repositories;
using FormularioFoodieApi.Data.Repositories.Interfaces;
using FormularioFoodieApi.Services;
using FormularioFoodieApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var connectionString = builder.Configuration.GetConnectionString("ConnectionDataBase");
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Dependency Injection
builder.Services.AddScoped<IFormularioFoodieRepository, FormularioFoodieRepository>();
builder.Services.AddScoped<IFormularioFoodieService, FormularioFoodieService>();
builder.Services.AddScoped<IUsersApiService, UsersApiService>();
builder.Services.AddHttpClient<IUsersApiService, UsersApiService>();

builder.Services.AddDbContext<FormularioFoodieDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

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

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, _) =>
    {
        document.Info = new()
        {
            Title = "API de Formulario Foodie",
            Version = "v1",
            Description = "API que permite manejar formularios de aplicación para foodie bloggers"
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FormularioFoodieDbContext>();
    context.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();