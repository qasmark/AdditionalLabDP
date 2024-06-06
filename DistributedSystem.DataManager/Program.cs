using StackExchange.Redis;
using System.Text.Json;
using DistributedSystem.DataManager.Dataclasses;

namespace DistributedSystem.DataManager;

internal class Program
{
    private static readonly ConnectionMultiplexer _connection = ConnectionMultiplexer.Connect("localhost:6379");
    private static readonly IDatabase _db = _connection.GetDatabase();

    static void Main() 
    {
        while (true)
        {
            try
            {
                ProcessInput();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
            }
        }
    }

    private static void ProcessInput()
    {
        List<string> past = new List<string>();
        List<string> future = new List<string>();
        List<string> parallel = new List<string>();
    
        Console.WriteLine("Usage: <process number> <event number>");
        string input = Console.ReadLine();

        string[] parts = input.Split(' ');

        if (parts.Length != 2 
            || !int.TryParse(parts[0], out int tempProcessId) 
            || !int.TryParse(parts[1], out int tempEventId))
        {
            throw new FormatException("Incorrect input format");
        }

        string currentEventKey = $"e{tempProcessId}_{tempEventId}";
        string timesJson = _db.StringGet(currentEventKey);
        if (string.IsNullOrEmpty(timesJson))
        {
            throw new KeyNotFoundException("Event with the specified id not found");
        }

        List<int> currentEventTimeStamp = JsonSerializer.Deserialize<List<int>>(timesJson);
        Event currE = new Event()
        {
            Id = currentEventKey,
            ProcessesTimeStamp = currentEventTimeStamp
        };

        var server = _connection.GetServer("localhost:6379");
        foreach (var key in server.Keys())
        {
            string eventJson = _db.StringGet(key);
            List<int> eventTimeStamp = JsonSerializer.Deserialize<List<int>>(eventJson);
            Event e = new Event()
            {
                Id = key,
                ProcessesTimeStamp = eventTimeStamp
            };

            if (e == currE) continue;

            if (e < currE)
            {
                past.Add(e.Id);
                continue;
            }

            if (currE < e)
            {
                future.Add(e.Id);
                continue;
            }
            parallel.Add(e.Id);
        }

        Console.WriteLine("past: " + string.Join(", ", past));
        Console.WriteLine("future: " + string.Join(", ", future));
        Console.WriteLine("parallel: " + string.Join(", ", parallel));
    }
}