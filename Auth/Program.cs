using Auth.Middleware;
using Domain.Context;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, sp, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", "Auth")
    .WriteTo.Console()
    .WriteTo.Seq(ctx.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341"));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var jwtSettings = builder.Configuration.GetSection("Jwt");
using var loggerFactory = LoggerFactory.Create(l => l.AddConsole());
var rsaKeyProvider = new RsaKeyProvider(builder.Configuration, loggerFactory.CreateLogger<RsaKeyProvider>());

builder.Services.AddDbContext<AuthSistemaSupermercadoContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CadenaSQL"),
        x => x.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)
    )
);

builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddValidators();
builder.Services.AddSingleton(rsaKeyProvider);
builder.Services.AddSingleton<Token>();

// JWT Auth
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
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = rsaKeyProvider.PublicKey,
        ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 },
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

// Configuración de Rate Limiting
builder.Services.AddRateLimiter(option =>
{
    option.RejectionStatusCode =
    StatusCodes.Status429TooManyRequests;

    option.AddFixedWindowLimiter("fijo", opt =>
    {
        opt.PermitLimit = 15;
        opt.Window = TimeSpan.FromSeconds(30);
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
});

builder.Services.AddControllers();


// Agregar Swagger para documentación API  https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "API-Auth", Version = "v1" });

    // Configuración de seguridad JWT
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese el token JWT en el campo de texto: **Bearer {su token}**"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthSistemaSupermercadoContext>();
    dbContext.Database.Migrate();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseSerilogRequestLogging(o =>
{
    o.GetLevel = (ctx, _, ex) =>
        ex != null || ctx.Response.StatusCode >= 500 ? LogEventLevel.Error
        : ctx.Response.StatusCode >= 400 ? LogEventLevel.Warning
        : LogEventLevel.Information;
    o.EnrichDiagnosticContext = (d, ctx) =>
    {
        d.Set("IdUsuario", ctx.User.FindFirstValue(ClaimTypes.NameIdentifier));
        d.Set("CorrelationId", ctx.Items[CorrelationIdMiddleware.ItemKey]);
    };
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoints de descubrimiento (OpenID Connect) para que los demás microservicios obtengan la clave pública. Son públicos: no contienen nada secreto.
app.MapGet("/.well-known/openid-configuration", (HttpRequest request) =>
{
    var baseUrl = $"{request.Scheme}://{request.Host}";
    return Results.Ok(new
    {
        issuer = jwtSettings["Issuer"],
        jwks_uri = $"{baseUrl}/.well-known/jwks.json",
        id_token_signing_alg_values_supported = new[] { SecurityAlgorithms.RsaSha256 }
    });
}).AllowAnonymous().ExcludeFromDescription();

app.MapGet("/.well-known/jwks.json", (RsaKeyProvider keys) => Results.Ok(keys.GetJwks())).AllowAnonymous().ExcludeFromDescription();

app.Run();
