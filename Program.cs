using Microsoft.EntityFrameworkCore;
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

// 1. Retrieve the connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Automatically detect or define the MySQL Server version
var serverVersion = ServerVersion.AutoDetect(connectionString);

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseMySql(connectionString,serverVersion));

builder.Services.AddScoped<ISubissionBgService,SubissionBgService>();
builder.Services.AddScoped<ILocalFileStorage,LocalFileStorage>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
