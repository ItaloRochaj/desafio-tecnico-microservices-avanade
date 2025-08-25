using Stock.API.Data;
using Microsoft.EntityFrameworkCore;
using Common;
using Stock.API.RabbitMQ;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);


// Para rodar localmente, use a porta 5263
builder.WebHost.UseUrls("http://localhost:5263");

// Configuração do Banco de Dados
builder.Services.AddDbContext<StockContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// Autenticação JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings != null)
{
    builder.Services.AddJwtAuthentication(jwtSettings);
}

// RabbitMQ
builder.Services.AddSingleton<RabbitMQConsumer>();

// Logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger configurado para integração com Gateway + Autenticação
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Stock API",
        Version = "v1",
        Description = "Serviço de Estoque - acessível via API Gateway"
    });

    // Configuração de segurança para JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insira o token JWT com o prefixo Bearer. Ex: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Inicializar banco de dados
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<StockContext>();
    context.Database.EnsureCreated();
}

// Sempre habilita Swagger para integração com o Gateway
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stock API v1");
    // Deixe o Swagger disponível em /swagger (padrão)
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// RabbitMQ Consumer inicia automaticamente como BackgroundService
app.Run();
