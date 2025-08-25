using Common;
using Microsoft.EntityFrameworkCore;
using Sales.API.Data;
using Sales.API.RabbitMQ;
using Sales.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Banco de Dados
builder.Services.AddDbContext<SalesContext>(options =>
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

// Services
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<RabbitMQPublisher>();

// HTTP Client para Stock API
builder.Services.AddHttpClient<StockService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["StockApiUrl"] ?? "http://localhost:5001");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Controllers e JSON config
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Swagger sempre ativo (para integração com API Gateway)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Para rodar localmente, use a porta 5285 (ajustável conforme necessário)
if (args.Contains("--local"))
{
    builder.WebHost.UseUrls("http://localhost:5285");
}

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
        logger.LogError(ex, "Erro ao inicializar banco de dados");
    }
}

// Pipeline HTTP
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
