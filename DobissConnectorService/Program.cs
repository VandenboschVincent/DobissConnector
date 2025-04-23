using DobissConnectorService;
using DobissConnectorService.Consumers;
using DobissConnectorService.Consumers.Messages;
using DobissConnectorService.Dobiss;
using DobissConnectorService.Dobiss.Models;
using MQTTnet.Server;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Mqtt;
using SlimMessageBus.Host.Serialization.SystemTextJson;

var builder = Host.CreateApplicationBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Logging.ClearProviders().SetMinimumLevel(LogLevel.Debug)
        .AddDebug();
}

builder.Logging.AddConsole();
builder.Services.AddSingleton<DobissClientFactory>();
builder.Services.AddHostedService<Worker>();
builder.Services.Configure<Settings>(builder.Configuration.GetSection("dobiss"));
builder.Services.AddSlimMessageBus(slimBuilder =>
{
    slimBuilder.PerMessageScopeEnabled();
    slimBuilder.Consume<LightToggledMessage>(cfg => cfg.Topic("homeassistant/light/set/+").WithConsumerOfContext<LightToggledConsumer>());
    slimBuilder.Produce<LightStateMessage>(cfg => cfg.DefaultPath("homeassistant"));
    slimBuilder.Produce<LightConfigMessage>(cfg => cfg.DefaultPath("homeassistant"));
    slimBuilder.WithCustomProviderMqtt(cfg =>
    {
        cfg.ClientBuilder
            .WithTcpServer(builder.Configuration["dobiss:MqttIp"], int.Parse(builder.Configuration["dobiss:Mqttport"]!))
            // Use MQTTv5 to use message headers (if the broker supports it)
            .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500);
        if (!string.IsNullOrEmpty(builder.Configuration["dobiss:MqttUser"]))
        {
            cfg.ClientBuilder.WithCredentials(builder.Configuration["dobiss:MqttUser"], builder.Configuration["dobiss:MqttPassword"]);
        }
    })
    .AddServicesFromAssemblyContaining<LightToggledConsumer>()
    .AddJsonSerializer();
});

var host = builder.Build();
await host.RunAsync();
