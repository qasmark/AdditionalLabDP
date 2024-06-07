using NATS.Client;
using System.Text;
using System.Text.Json;
using DistributedSystem.Process.Dataclasses;
using StackExchange.Redis;

namespace DistributedSystem.Process;

 internal class Program
{
    private static readonly IConnection _natsConnection = new ConnectionFactory().CreateConnection("localhost:4222");
    private static readonly ConnectionMultiplexer _redisConnection = ConnectionMultiplexer.Connect("localhost:6379");
    private static readonly IDatabase _redisDb = _redisConnection.GetDatabase();
    private static readonly List<int> _processesTimeStamp = new List<int> { 0, 0, 0, 0 };
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
        string messageJson = Encoding.UTF8.GetString(messageBytes);
        return JsonSerializer.Deserialize<Message>(messageJson);
    }

    private static void DisplayReceivedMessage(Message message)
    {
        Console.WriteLine($"Received message: {message.Msg} from process with id {message.Id}");
    }

    private static void UpdateProcessTimes(List<int> times)
    {
        for (int i = 0; i < _processesTimeStamp.Count; i++)
        {
            _processesTimeStamp[i] = Math.Max(_processesTimeStamp[i], times[i]);
        }
        _processesTimeStamp[_id]++;
    }

    private static void PublishEvent()
    {
        var e = CreateEvent();
        var eventMessage = SerializeEvent(e);
        var messageBytes = Encoding.UTF8.GetBytes(eventMessage);
        _natsConnection.Publish("event", messageBytes);
    }

    private static Event CreateEvent()
    {
        return new Event
        {
            Id = $"e{_id}_{_processesTimeStamp[_id]}",
            ProcessesTimeStamp = _processesTimeStamp
        };
    }

    private static string SerializeEvent(Event e)
    {
        return JsonSerializer.Serialize(e);
    }

    private static void ProcessUserInput()
    {
        while (true)
        {
            Console.WriteLine("Usage: <process number> [out <message>] or '<process number> in' to receive messages");
            string input = Console.ReadLine();
            var (id, message, command) = ParseInput(input);

            if (command == "in")
            {
                ProcessInCommand(id);
            }
            else if (command == "out")
            {
                if (id != _id)
                {
                    if (string.IsNullOrEmpty(message))
                    {
                        Console.WriteLine("Message cannot be empty");
                    }
                    else
                    {
                        SendMessage(id, message);
                        PublishEvent();
                    }
                }
                else
                {
                    Console.WriteLine("Cannot send message to self. For internal events, just provide the process number.");
                }
            }
            else if (string.IsNullOrEmpty(command))
            {
                if (id == _id)
                {
                    ProcessInternalEvent();
                }
                else
                {
                    Console.WriteLine("Invalid input format. Use '<process number> out <message>' to send messages or '<process number> in' to receive messages.");
                }
            }
            else
            {
                Console.WriteLine("Invalid command. Use 'out' to send messages to other processes or 'in' to receive messages.");
            }
        }
    }
    
    private static void ProcessInternalEvent()
    {
        Console.WriteLine($"Internal event at process {_id}");
        _processesTimeStamp[_id]++;
        PublishEvent();
    }
    
    private static (int id, string message, string command) ParseInput(string input)
    {
        string[] parts = input.Split(' ');

        if (parts.Length == 1 && int.TryParse(parts[0], out int id))
        {
            return (id, string.Empty, string.Empty);
        }

        if (parts.Length < 2 || !int.TryParse(parts[0], out id))
        {
            Console.WriteLine("Invalid input format");
            return (0, string.Empty, string.Empty);
        }

        string command = parts[1];
        string message = parts.Length > 2 ? string.Join(" ", parts, 2, parts.Length - 2) : string.Empty;

        return (id, message, command);
    }
    private static void ProcessInCommand(int senderId)
    {
        string key = $"message_{senderId}_to_{_id}";

        string messageJson = _redisDb.ListRightPop(key);
        if (string.IsNullOrEmpty(messageJson))
        {
            Console.WriteLine($"No messages from process {senderId}");
            return;
        }

        var message = JsonSerializer.Deserialize<Message>(messageJson);
        if (message == null)
        {
            Console.WriteLine("Failed to deserialize message");
            return;
        }

        DisplayReceivedMessage(message);
        UpdateProcessTimes(message.ProcessesTimeStamp);
        PublishEvent();
    }

    private static void SendMessage(int receiverId, string messageContent)
    {
        _processesTimeStamp[_id]++;
        var message = CreateMessage(receiverId, messageContent);
        var messageJson = SerializeMessage(message);
        _redisDb.ListLeftPush($"message_{_id}_to_{receiverId}", messageJson);
    }

    private static Message CreateMessage(int receiverId, string messageContent)
    {
        return new Message
        {
            Id = _id.ToString(),
            Msg = messageContent,
            ProcessesTimeStamp = new List<int>(_processesTimeStamp)
        };
    }

    private static string SerializeMessage(Message message)
    {
        return JsonSerializer.Serialize(message);
    }
}