using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Dobiss;
using DobissConnectorService;
using DobissConnectorService.Dobiss.Interfaces;
using ToMqttNet;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddDebug();
}

builder.Logging.AddConsole();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IDobissClientFactory, DobissClientFactory>();
builder.Services.AddHostedService<BackgroundWorker>();
builder.Services.AddHostedService<HomeAssistantSubscriber>();
builder.Services.AddTransient<ILightCacheService, LightCacheService>();
var dobissConfig = builder.Configuration.GetSection("dobiss");
var mqttConfig = builder.Configuration.GetSection("mqtt");
builder.Services.Configure<DobissSettings>(dobissConfig);
builder.Services.Configure<MqttSettings>(mqttConfig);
builder.Services.AddMediator();
builder.Services.AddMqttConnection();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapOpenApi();
app.MapHealthChecks("/health");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
