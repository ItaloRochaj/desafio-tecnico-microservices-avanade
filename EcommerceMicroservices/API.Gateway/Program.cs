using Microsoft.IdentityModel.Tokens;
using System.Text;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot; // precisa para o UseSwaggerForOcelotUI
using Common; // Confirma se o namespace realmente é esse

var builder = WebApplication.CreateBuilder(args);

// Configuração do Ocelot (inclui ocelot.json)
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Configuração de JwtSettings a partir do appsettings.json
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddSingleton<JwtSettings>(jwtSettings);
builder.Services.AddSingleton<TokenService>();

// Autenticação JWT
builder.Services.AddAuthentication("Jwt")
    .AddJwtBearer("Jwt", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();

// Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ocelot + SwaggerForOcelot
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddSwaggerForOcelot(builder.Configuration);


var app = builder.Build();

// Log de endpoints mapeados para depuração
app.Lifetime.ApplicationStarted.Register(() =>
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("EndpointLogger");
    var dataSource = app.Services.GetRequiredService<Microsoft.AspNetCore.Routing.EndpointDataSource>();
    foreach (var endpoint in dataSource.Endpoints)
    {
        logger.LogInformation($"Endpoint: {endpoint.DisplayName}");
    }
});


// Swagger apenas
app.UseSwagger();


app.UseAuthentication();
app.UseAuthorization();




// Controllers
app.MapControllers();


// Ocelot
await app.UseOcelot();
await app.RunAsync();
