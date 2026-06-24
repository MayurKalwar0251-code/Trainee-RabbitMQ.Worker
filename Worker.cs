using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TrainineeAPI.Models;

namespace RabbitMQ.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ConnectionFactory _connectionFactory;
    private IConnection? _connection;
    private IChannel? _channel;
    public Worker(ILogger<Worker> logger,ConnectionFactory connectionFactory, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("We are herer");
        _connection = await _connectionFactory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await _channel.QueueDeclareAsync(
            queue: "submissionProcessingQueue",
            durable: true, 
            exclusive: false, 
            autoDelete: false, 
            arguments: null,
            cancellationToken: stoppingToken
        );

        Console.WriteLine("Connected to Queue");

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model,ea) =>
        {
            Console.WriteLine("Consumer consuming message");

            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            Console.WriteLine("Json string : " + json);
            var message = JsonSerializer.Deserialize<SubmissionProcessingRequestModel>(json);

            if (message == null)
            {
                Console.WriteLine("Message desrialize gave null");
            }

            Console.WriteLine("Revieved Message : " + message);

            // Adding service di 
            try
            {
                await using (var scope = _serviceScopeFactory.CreateAsyncScope())
                {
                    var submissionBgService = scope.ServiceProvider.GetRequiredService<ISubissionBgService>();

                    await submissionBgService.GetFileMetaData(message!.SubmissionFileId, cancellationToken: stoppingToken);
                }
            }
            catch (System.Exception)
            {
                Console.WriteLine("Not able to inject service file");
                throw;
            }

            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
        };

        await _channel.BasicConsumeAsync(
            queue: "submissionProcessingQueue", 
            autoAck: false, 
            consumer: consumer,
            cancellationToken: stoppingToken
        );

        // Keep the thread alive while listening for messages
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Inside Stop Async Function");
        if (_channel is not null) await _channel.CloseAsync(cancellationToken);
        if (_connection is not null) await _connection.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }     
}
