using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Worker;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient<ITraineeDirectoryClient,TraineeDirectoryClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["TraineeDirectoryApi:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(5);
}).AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 2;
    options.Retry.Delay = TimeSpan.FromSeconds(2);
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.MinimumThroughput = 5;
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
});

var rabbitMQSection = builder.Configuration.GetSection("RabbitMQ");

builder.Services.AddSingleton(sp => 
{
    var factory = new ConnectionFactory
    {
    HostName = rabbitMQSection["HostName"] ?? "localhost",
    UserName = rabbitMQSection["UserName"] ?? "guest",
    Password = rabbitMQSection["Password"] ?? "guest",
    VirtualHost = rabbitMQSection["VirtualHost"] ?? "/",
    Port = int.Parse(rabbitMQSection["Port"] ?? "5672"),
    AutomaticRecoveryEnabled = true,
    NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
    };

    Console.WriteLine($"RAbbit Mq -> {factory.HostName} : {factory.Port}");
    return factory;
});

Console.WriteLine("RabbitMQ:HostName" + rabbitMQSection["HostName"]);
Console.WriteLine("RabbitMQ:UserName" +  rabbitMQSection["UserName"]);
Console.WriteLine("RabbitMQ:Password" +  rabbitMQSection["Password"]);
Console.WriteLine("RabbitMQ:VirtualHost" +  rabbitMQSection["VirtualHost"]);

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
