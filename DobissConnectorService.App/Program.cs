using DobissConnectorService.Dobiss.Models;
using DobissConnectorService.Dobiss;
using DobissConnectorService;
using SlimMessageBus.Host;
using DobissConnectorService.Consumers.Messages;
using DobissConnectorService.Consumers;
using SlimMessageBus.Host.Serialization.SystemTextJson;
using SlimMessageBus.Host.Memory;
using DobissConnectorService.Dobiss.Interfaces;

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
builder.Services.AddTransient<ILightCacheService, LightCacheService>();
var dobissConfig = builder.Configuration.GetSection("dobiss");
var mqttConfig = builder.Configuration.GetSection("mqtt");
builder.Services.Configure<DobissSettings>(dobissConfig);
builder.Services.Configure<MqttSettings>(mqttConfig);
builder.Services.AddMediator();
builder.Services.AddSlimMessageBus(slimBuilder =>
{
    slimBuilder.PerMessageScopeEnabled()
        .Consume<ChangeLigthMessage>(cfg => cfg.Topic("homeassistant/light/set/+").WithConsumerOfContext<LightChangedConsumer>())
        .Produce<LightChangedMessage>(cfg => cfg.DefaultPath("homeassistant"))
        .Produce<LightConfigMessage>(cfg => cfg.DefaultPath("homeassistant"))
        .AddServicesFromAssemblyContaining<LightChangedConsumer>()
        .AddJsonSerializer();
    if (mqttConfig.Exists())
    {
        slimBuilder.WithCustomProviderMqtt(cfg =>
        {
            cfg.ClientBuilder
                .WithTcpServer(mqttConfig["Ip"], int.Parse(mqttConfig["Port"]!))
                // Use MQTTv5 to use message headers (if the broker supports it)
                .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500);
            if (!string.IsNullOrEmpty(mqttConfig["User"]))
            {
                cfg.ClientBuilder.WithCredentials(mqttConfig["User"], mqttConfig["Password"]);
            }
        });
    }
    else
    {
        slimBuilder.WithProviderMemory();
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
