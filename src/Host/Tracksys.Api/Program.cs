using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Tracksys.Modules.Alerting.Api;
using Tracksys.Modules.Alerting.Infrastructure;
using Tracksys.Modules.Citizen.Api;
using Tracksys.Modules.Citizen.Infrastructure;
using Tracksys.Modules.Fleet.Api;
using Tracksys.Modules.Fleet.Infrastructure;
using Tracksys.Modules.Identity.Api;
using Tracksys.Modules.Identity.Application.Options;
using Tracksys.Modules.Identity.Infrastructure;
using Tracksys.Modules.Ingestion.Api;
using Tracksys.Modules.Ingestion.Infrastructure;
using Tracksys.Modules.Reports.Api;
using Tracksys.Modules.Reports.Infrastructure;
using Tracksys.Modules.Ingestion.Application.Abstractions;
using Tracksys.Modules.Tenancy.Api;
using Tracksys.Modules.Tenancy.Infrastructure;
using Tracksys.Shared.Infrastructure.Auth;
using Tracksys.Shared.Kernel.Auth;
using Tracksys.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ----- Modules (Infrastructure : DbContext + UnitOfWork par module, schéma PostgreSQL dédié) -----
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddFleetModule(builder.Configuration);
builder.Services.AddCitizenModule(builder.Configuration);
builder.Services.AddAlertingModule(builder.Configuration);
builder.Services.AddIngestionModule(builder.Configuration);
builder.Services.AddReportsModule(builder.Configuration);
builder.Services.AddTenancyModule(builder.Configuration);

// ----- Multi-tenant par ville : résout city_id/rôle SuperAdmin depuis le JWT courant,
// injecté dans chaque DbContext de module pour le HasQueryFilter global -----
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenantAccessor, HttpContextTenantAccessor>();

// ----- Temps réel positions (SignalR) — push après chaque ingestion Flespi réussie -----
builder.Services.AddSignalR();
builder.Services.AddSingleton<IPositionBroadcaster, SignalRPositionBroadcaster>();

// ----- Contrôleurs des modules (Api layer de chaque module, montés dans le Host) -----
var mvcBuilder = builder.Services.AddControllers();
mvcBuilder
    .AddIdentityApiModule()
    .AddFleetApiModule()
    .AddCitizenApiModule()
    .AddAlertingApiModule()
    .AddIngestionApiModule(builder.Configuration)
    .AddReportsApiModule()
    .AddTenancyApiModule();

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Tracksys API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Coller uniquement le token JWT (sans le préfixe 'Bearer ')",
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

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
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tracksys API v1");
    });
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PositionsHub>("/hubs/positions");

app.Run();
