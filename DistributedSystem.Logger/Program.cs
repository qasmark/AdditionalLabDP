using System.Text;
using System.Text.Json;
using NATS.Client;
using DistributedSystem.Logger.Dataclasses;
using DistributedSystem.Logger.Repository;

namespace DistributedSystem.Logger;

internal class Program
{
    private static readonly IConnection _natsConnection = new ConnectionFactory().CreateConnection("localhost:4222");
    private static IRepository _repository = new RedisRepository("localhost:6379");
    
    static void Main()
    {
        SubscribeToEvents();
        Console.WriteLine("Logger is running. Press Ctrl+C to exit.");
        // Keep the application running
        Thread.Sleep(Timeout.Infinite);
    }
    
    private static void SubscribeToEvents()
    {
        _natsConnection.SubscribeAsync("event", (sender, args) =>
        {
            var messageObject = DeserializeMessage(args.Message.Data);
            var id = messageObject.Id;
            var processesTimeStamp = messageObject.ProcessesTimeStamp;
                
            _repository.Save(id, processesTimeStamp);  // Save in real-time

            Console.WriteLine($"Event registered: {id} from process with timestamps {string.Join(", ", processesTimeStamp)}");
        });
    }

    private static Message DeserializeMessage(byte[] messageBytes)
    {
        var messageJson = Encoding.UTF8.GetString(messageBytes);
        return JsonSerializer.Deserialize<Message>(messageJson);
    }
}