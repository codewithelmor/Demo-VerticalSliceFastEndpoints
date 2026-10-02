var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Read JWT settings once
var apiSettings = builder.Configuration.GetSection(JwtSettings.SectionName);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

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
            .WithMethods(HttpMethods.Get, HttpMethods.Post)
            .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization);
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(apiSettings["Secret"]!)),

            ValidateIssuer = true,
            ValidIssuer = apiSettings["Issuer"],

            ValidateAudience = true,
            ValidAudience = apiSettings["Audience"],

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers(options =>
{
    // Force the server to return 406 if it cannot satisfy the Accept header
    options.ReturnHttpNotAcceptable = true;
})
.AddXmlDataContractSerializerFormatters();

builder.Services
.AddFastEndpoints()
.AddResponseCaching();

// Configure the v1 Swagger Document (Filters endpoints to only show Version 1)
//builder.Services.SwaggerDocument(o =>
//{
//    o.MinEndpointVersion = 1;
//    o.MaxEndpointVersion = 1; // Strictly keeps out anything version 2 or higher
//    o.ShortSchemaNames = true;

//    // FIX: Set this to 0 to disable automated URL routing folders entirely
//    o.AutoTagPathSegmentIndex = 0;

//    o.DocumentSettings = s =>
//    {
//        s.Title = "Product API v1";
//        s.Version = "v1";
//        s.DocumentName = "v1"; // The URL slug will be /swagger/v1/swagger.json
//    };
//});

// Configure the v1 OpenApi Document (Filters endpoints to only show Version 1)
builder.Services.OpenApiDocument(options =>
{
    options.DocumentName = "v1";
    options.Title = "Product API v1";
    options.Version = "v1";

    options.ShortSchemaNames = true;

    // Disable automatic route-segment tagging
    options.AutoTagPathSegmentIndex = 0;

    // Automatically add JWT Bearer authentication
    options.EnableJWTBearerAuth = true;

    options.MinEndpointVersion = 1;
    options.MaxEndpointVersion = 1;
}).OpenApiDocument(options =>
{
    options.DocumentName = "v2";
    options.Title = "Product API v2";
    options.Version = "v2";

    options.ShortSchemaNames = true;

    // Disable automatic route-segment tagging
    options.AutoTagPathSegmentIndex = 0;

    // Automatically add JWT Bearer authentication
    options.EnableJWTBearerAuth = true;

    options.MinEndpointVersion = 2;
    options.MaxEndpointVersion = 2;
});

var app = builder.Build();

app
    .UseResponseCaching()
    .UseFastEndpoints(c =>
    {
        c.Endpoints.RoutePrefix = "api";     // Base route prefix -> /api
        c.Versioning.Prefix = "v";           // Version number prefix -> v
        c.Versioning.PrependToRoute = true;  // COMBINES THEM INTO: /api/v1/...

        c.Endpoints.Configurator = ep =>
        {
            ep.PreProcessor<AcceptHeaderValidator>(Order.Before);
        };

        // Define options once outside the delegate to avoid per-request allocations
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        c.Serializer.RequestDeserializer = async (req, tDto, jsonSerializerContext, ct) =>
        {
            if (req.ContentType?.Contains("xml") == true)
            {
                using var reader = new StreamReader(req.Body);
                var xml = await reader.ReadToEndAsync(ct);
                var serializer = new XmlSerializer(tDto);
                using var stringReader = new StringReader(xml);
                return serializer.Deserialize(stringReader)!;
            }

            return await JsonSerializer.DeserializeAsync(req.Body, tDto, jsonOptions, ct);
        };

        c.Serializer.ResponseSerializer = (rsp, dto, contentType, jsonContext, ct) =>
        {
            // Respect Accept header for negotiation, falling back to the passed contentType
            var acceptHeader = rsp.HttpContext.Request.Headers.Accept.ToString();
            var wantsXml = acceptHeader.Contains("xml") || contentType?.Contains("xml") == true;

            if (wantsXml)
            {
                rsp.ContentType = MediaTypeNames.Application.Xml;
                var serializer = new XmlSerializer(dto!.GetType());
                using var ms = new MemoryStream();
                serializer.Serialize(ms, dto);
                return rsp.Body.WriteAsync(ms.ToArray(), ct).AsTask();
            }

            rsp.ContentType = MediaTypeNames.Application.Json;
            return JsonSerializer.SerializeAsync(rsp.Body, dto, dto!.GetType(), jsonOptions, ct);
        };
    });
//.UseSwaggerGen();

app.MapOpenApi();

app.MapScalarApiReference(options =>
{
    options
        .WithTitle("Product API v1")
        .AddDocument(
            "v1",
            "Product API v1",
            "/openapi/v1.json"
        ).AddDocument(
            "v2",
            "Product API v2",
            "/openapi/v2.json"
        );
});

app.Run();
