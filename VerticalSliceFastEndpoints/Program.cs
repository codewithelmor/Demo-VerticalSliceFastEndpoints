var builder = WebApplication.CreateBuilder(args);

builder.Services
.AddFastEndpoints()
.AddResponseCaching()
.SwaggerDocument();

var app = builder.Build();

app
    .UseResponseCaching()
    .UseFastEndpoints()
    .UseSwaggerGen();

app.Run();
