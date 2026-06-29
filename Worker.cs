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
    private readonly int MaxRetry = 10;
    public Worker(ILogger<Worker> logger, ConnectionFactory connectionFactory, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            Console.WriteLine("We are herer");
            Console.WriteLine("RabbitMQ:HostName" + _configuration.GetSection("RabbitMQ")["HostName"]);
            Console.WriteLine("RabbitMQ:UserName" +  _configuration.GetSection("RabbitMQ")["UserName"]);
            Console.WriteLine("RabbitMQ:Password" +  _configuration.GetSection("RabbitMQ")["Password"]);
            Console.WriteLine("RabbitMQ:VirtualHost" +  _configuration.GetSection("RabbitMQ")["VirtualHost"]);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _connection = await _connectionFactory.CreateConnectionAsync(stoppingToken);
                    _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                    Console.WriteLine("Connected to RabbitMQ.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"RabbitMQ not ready: {ex.Message}");
                    await Task.Delay(5000, stoppingToken);
                }
            }
            // _connection = await _connectionFactory.CreateConnectionAsync(stoppingToken);
            // _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
            await _channel.QueueDeclareAsync(
                queue: "submissionProcessingQueue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    ["x-dead-letter-exchange"] = "sub-dlx",
                    ["x-dead-letter-routing-key"] = "sub-dlr"
                },
                cancellationToken: stoppingToken
            );

            // creating dlq
            await _channel.ExchangeDeclareAsync(exchange: "sub-dlx", type: ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(
                queue: "sub-dlq",
                exclusive: false,
                durable: true,
                autoDelete: false,
                cancellationToken: stoppingToken
            );

            await _channel.QueueBindAsync(
                queue: "sub-dlq",
                exchange: "sub-dlx",
                routingKey: "sub-dlr",
                cancellationToken: stoppingToken
            );

            Console.WriteLine("Connected to Queue");

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, args) =>
            {
                Console.WriteLine("Consumer consuming message");

                var body = args.Body.ToArray();
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

                        await submissionBgService.GetFileMetaData(message!.SubmissionFileId, messageId: message.MessageId, cancellationToken: stoppingToken);
                        Console.WriteLine("Completed Funcrion ....");
                    }
                }
                catch (MaxAttemptExeption e)
                {
                    Console.WriteLine("CATCH ATTEMPT : " + e.AttemptCount);
                    Console.WriteLine("CATCH Error : " + e.Message);
                    if (e.AttemptCount >= MaxRetry)
                    {
                        Console.WriteLine("Stop Requeing ...");
                        await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
                    }
                    else
                    {
                        if (e.RetryReque)
                        {
                            Console.WriteLine("Not Requeuing as bool is true");
                            await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
                            return;
                        }
                        Console.WriteLine("Requeing ...");
                        await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true);
                    }

                    return;
                }
                catch(Exception e)
                {
                    Console.WriteLine("Inside Catch of worker as Exception Catch : " + e.Message);
                }

                await _channel.BasicAckAsync(deliveryTag: args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
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
        catch (Exception ex)
        {
            Console.WriteLine("Error in WOrker catecjed as : " + ex.ToString());
            Console.WriteLine("Error in WOrker catecjed as : " + ex.Message);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Inside Stop Async Function");
        if (_channel is not null) await _channel.CloseAsync(cancellationToken);
        if (_connection is not null) await _connection.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}

// TODO : Still the error is visible in terminal when TraineeDirectory.API server is unreachable
// handle it properly