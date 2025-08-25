using Common;
using Microsoft.EntityFrameworkCore;
using Sales.API.Data;
using Sales.API.RabbitMQ;
using Sales.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Banco de Dados
builder.Services.AddDbContext<SalesContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

// Autenticação JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings != null)
{
    builder.Services.AddJwtAuthentication(jwtSettings);
}

// Services
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<RabbitMQPublisher>();

// HTTP Client para Stock API
builder.Services.AddHttpClient<StockService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["StockApiUrl"] ?? "http://localhost:5001");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar para lidar com referências circulares
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

// Forçar escuta na porta 80 para Docker
builder.WebHost.UseUrls("http://*:80");

var app = builder.Build();

// Inicializar banco de dados
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SalesContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error initializing database");
    }
}

// Configure the HTTP request pipeline.

// Sempre habilita Swagger para integração com o Gateway
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
