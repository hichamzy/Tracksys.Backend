using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Tracksys.Modules.Alerting.Api;
using Tracksys.Modules.Alerting.Infrastructure;
using Tracksys.Modules.Citizen.Api;
using Tracksys.Modules.Citizen.Infrastructure;
using Tracksys.Modules.Fleet.Api;
using Tracksys.Modules.Fleet.Infrastructure;
using Tracksys.Modules.Identity.Api;
using Tracksys.Modules.Identity.Application.Options;
using Tracksys.Modules.Identity.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ----- Modules (Infrastructure : DbContext + UnitOfWork par module, schéma SQL Server dédié) -----
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddFleetModule(builder.Configuration);
builder.Services.AddCitizenModule(builder.Configuration);
builder.Services.AddAlertingModule(builder.Configuration);

// ----- Contrôleurs des modules (Api layer de chaque module, montés dans le Host) -----
var mvcBuilder = builder.Services.AddControllers();
mvcBuilder
    .AddIdentityApiModule()
    .AddFleetApiModule()
    .AddCitizenApiModule()
    .AddAlertingApiModule();

// ----- JWT Bearer -----
JwtOptions jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Section de configuration 'Jwt' introuvable.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
