using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Worker;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient<ITraineeDirectoryClient,TraineeDirectoryClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["TrainingDirectoryApi:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
});

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
