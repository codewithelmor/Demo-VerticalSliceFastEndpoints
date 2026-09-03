using Microsoft.EntityFrameworkCore;
using VerticalSliceFastEndpoints.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services
.AddFastEndpoints()
.AddResponseCaching()
.SwaggerDocument();

builder.Services.AddDbContext<ApplicationDbContext>(option =>
option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app
    .UseResponseCaching()
    .UseFastEndpoints()
    .UseSwaggerGen();

app.Run();
