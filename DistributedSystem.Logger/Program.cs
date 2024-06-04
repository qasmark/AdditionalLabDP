using System.Text;
using System.Text.Json;
using NATS.Client;
using DistributedSystem.Logger.Dataclasses;
using DistributedSystem.Logger.Repository;

namespace DistributedSystem.Logger;

internal class Program
{
    private static readonly IConnection _natsConnection = new ConnectionFactory().CreateConnection("localhost:4222");
    private static Dictionary<string, List<int>> _processesTimeStamp = new Dictionary<string, List<int>>();
    private static IRepository _repository = new RedisRepository("localhost:6379");
    
    static void Main()
    {
        SubscribeToEvents();
        
        while (true)
        {
            try
            {
                Console.WriteLine("Usage: <stop>\nto save data and switch off the logger");
                string keyStop = Console.ReadLine();
                if (keyStop == "stop")
                {   
                    _repository.Save(_processesTimeStamp);
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
    
    private static void SubscribeToEvents()
    {
        _natsConnection.SubscribeAsync("event", (sender, args) =>
        {
            var messageObject = DeserializeMessage(args.Message.Data);
            _processesTimeStamp.Add(messageObject.Id, messageObject.ProcessesTimeStamp);
        });
    }

    private static Message DeserializeMessage(byte[] messageBytes)
    {
        var messageJson = Encoding.UTF8.GetString(messageBytes);
        return JsonSerializer.Deserialize<Message>(messageJson);
    }
}