using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VerticalSliceFastEndpoints.Data;
using VerticalSliceFastEndpoints.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.AddDbContext<ApplicationDbContext>(option =>
option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin();
        policy.AllowAnyMethod();
        policy.AllowAnyHeader();
    });

    options.AddPolicy("RestrictedPolicy", policy =>
    {
        policy.WithOrigins(
                "https://myfrontend.com",
                "https://admin.myfrontend.com"
            )
            .WithMethods("GET", "POST")
            .WithHeaders("Content-Type", "Authorization");
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["ApiSettings:Secret"]!)),
            ValidateAudience = false,
            ValidateIssuer = false
        };
    });
builder.Services.AddAuthorization();

builder.Services
.AddFastEndpoints()
.AddResponseCaching();

// Configure the v1 Swagger Document (Filters endpoints to only show Version 1)
builder.Services.SwaggerDocument(o =>
{
    o.MinEndpointVersion = 1;
    o.MaxEndpointVersion = 1; // Strictly keeps out anything version 2 or higher
    o.ShortSchemaNames = true;

    // FIX: Set this to 0 to disable automated URL routing folders entirely
    o.AutoTagPathSegmentIndex = 0;

    o.DocumentSettings = s =>
    {
        s.Title = "Product API v1";
        s.Version = "v1";
        s.DocumentName = "v1"; // The URL slug will be /swagger/v1/swagger.json
    };
});

var app = builder.Build();

app
    .UseResponseCaching()
    .UseFastEndpoints(c =>
    {
        c.Endpoints.RoutePrefix = "api";     // Base route prefix -> /api
        c.Versioning.Prefix = "v";           // Version number prefix -> v
        c.Versioning.PrependToRoute = true;  // COMBINES THEM INTO: /api/v1/...
    })
    .UseSwaggerGen();

app.Run();
