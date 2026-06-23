using RabbitMQ.Client;
using RabbitMQ.Worker;

var builder = Host.CreateApplicationBuilder(args);

var rabbitMQSection = builder.Configuration.GetSection("RabbitMQ");

builder.Services.AddSingleton(sp => new ConnectionFactory()
{
    HostName = rabbitMQSection["HostName"] ?? "localhost",
    UserName = rabbitMQSection["UserName"] ?? "guest",
    Password = rabbitMQSection["Password"] ?? "guest",
    VirtualHost = rabbitMQSection["VirtualHost"] ?? "/",
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
