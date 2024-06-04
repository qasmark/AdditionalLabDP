using NATS.Client;
using System.Text;
using System.Text.Json;
using DistributedSystem.Process.Dataclasses;

namespace DistributedSystem.Process;

internal class Program
{
    private static readonly IConnection _natsConnection = new ConnectionFactory().CreateConnection("localhost:4222");
    private static List<int> _processesTimesStamp = new List<int> { 0, 0, 0 };
    private static int _id;
    
    static void Main(string[] args)
    {
        _id = int.Parse(args[0]);
        SubscribeToMessages();
        Console.WriteLine($"Process {_id} is ready for listening");
        ProcessUserInput();
    }
    
     private static void SubscribeToMessages()
     {
        _natsConnection.SubscribeAsync("message" + _id, (sender, args) =>
        {
            var messageObject = DeserializeMessage(args.Message.Data);
            DisplayReceivedMessage(messageObject);
            UpdateProcessTimes(messageObject.ProcessesTimeStamp);
            PublishEvent();
        });
     }
     private static Message DeserializeMessage(byte[] messageBytes)
    {
        string? messageJson = Encoding.UTF8.GetString(messageBytes);
        return JsonSerializer.Deserialize<Message>(messageJson);
    }
     
     private static void DisplayReceivedMessage(Message message)
    {
        Console.WriteLine($"Received message {message.Msg} from process with id {message.Id}");
    }

    private static void UpdateProcessTimes(List<int> times)
    {
        for (int i = 0; i < _processesTimesStamp.Count; i++)
        {
            _processesTimesStamp[i] = Math.Max(_processesTimesStamp[i], times[i]);
        }
        _processesTimesStamp[_id]++;
    }

    private static void PublishEvent()
    {
        var e = CreateEvent();
        var actionMessage = SerializeAction(e);
        var messageBytes = Encoding.UTF8.GetBytes(actionMessage);
        _natsConnection.Publish("event", messageBytes);
    }

    private static Event CreateEvent()
    {
        return new Event()
        {
            Id = $"e{_id}_{_processesTimesStamp[_id]}",
            ProcessesTimeStamp = _processesTimesStamp
        };
    }

    private static string SerializeAction(Event action)
    {
        return JsonSerializer.Serialize(action);
    }

    private static void ProcessUserInput()
    {
        while (true)
        {
            Console.WriteLine("Usage: <process number> <message>");
            string input = Console.ReadLine();
            var (id, message) = ParseInput(input);

            if (id == _id)
            {
                ProcessSelfMessage(message);
                continue;
            }

            SendMessage(id, message);
            PublishEvent();
        }
    }

    private static (int id, string message) ParseInput(string input)
    {
        string[] parts = input.Split(' ');

        if (parts.Length < 2 || !int.TryParse(parts[0], out int id))
        {
            Console.WriteLine("Invalid input format");
            return (0, string.Empty);
        }

        string message = string.Join(" ", parts, 1, parts.Length - 1);
        return (id, message);
    }

    private static void ProcessSelfMessage(string message)
    {
        Console.WriteLine("Message to self: " + message);
        _processesTimesStamp[_id]++;
        PublishEvent();
    }

    private static void SendMessage(int id, string message)
    {
        _processesTimesStamp[_id]++;
        var mess = CreateMessage(id, message);
        var messMessage = SerializeMessage(mess);
        var messageBytes = Encoding.UTF8.GetBytes(messMessage);
        _natsConnection.Publish("message" + id, messageBytes);
    }

    private static Message CreateMessage(int id, string message)
    {
        return new Message
        {
            Id = _id.ToString(),
            Msg = message,
            ProcessesTimeStamp = _processesTimesStamp
        };
    }

    private static string SerializeMessage(Message message)
    {
        return JsonSerializer.Serialize(message);
    }
}