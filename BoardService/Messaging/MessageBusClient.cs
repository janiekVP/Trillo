/*using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using Shared.Messaging.Events;

namespace BoardService.Messaging
{
    public class MessageBusClient
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "board_events", type: ExchangeType.Fanout);
        }

        public void PublishBoardDeleted(int boardId)
        {
            var message = new BoardDeletedEvent { BoardId = boardId };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            _channel.BasicPublish(
                exchange: "board_events",
                routingKey: "",
                basicProperties: null,
                body: body);
        }
    }
}
*/