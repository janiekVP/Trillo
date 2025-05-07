/*using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Messaging.Events;
using System.Text;
using System.Text.Json;
using CardService.Data;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CardService.Messaging
{
    public class BoardDeletedConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private IConnection _connection;
        private IModel _channel;

        public BoardDeletedConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: "board_events", type: ExchangeType.Fanout);

            var queueName = _channel.QueueDeclare().QueueName;
            _channel.QueueBind(queue: queueName, exchange: "board_events", routingKey: "");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var boardDeleted = JsonSerializer.Deserialize<BoardDeletedEvent>(message);

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var cardsToDelete = await db.Cards
                    .Where(c => c.BoardId == boardDeleted.BoardId)
                    .ToListAsync();

                db.Cards.RemoveRange(cardsToDelete);
                await db.SaveChangesAsync();
            };

            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
    }
}*/
