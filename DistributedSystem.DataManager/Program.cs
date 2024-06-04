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
                Console.WriteLine($"Error occured: {ex.Message}");
            }
        }
    }
    private static void ProcessInput()
    {
        List<string> past = new List<string>();
        List<string> future = new List<string>();
        List<string> parallel = new List<string>();
        
        Console.WriteLine("Usage: <session number> <process number> <event number>");
        string input = Console.ReadLine();

        string[] parts = input.Split(' ');

        if (parts.Length != 3 
            || !int.TryParse(parts[0], out int tempSessionId) 
            || !int.TryParse(parts[1], out int tempProcessId) 
            || !int.TryParse(parts[2], out int tempEventId))
        {
            throw new FormatException("Incorrect input format");
        }

        string timesJson = _db.StringGet(tempSessionId.ToString());
        if (string.IsNullOrEmpty(timesJson))
        {
            throw new KeyNotFoundException("Session with the specified id not found");
        }

        Dictionary<string, List<int>> times = JsonSerializer.Deserialize<Dictionary<string, List<int>>>(timesJson);

        Event currE = new Event()
        {
            ProcessesTimeStamp = times["e" + tempProcessId.ToString() + "_" + tempEventId.ToString()]
        };

        foreach (var key in times)
        {
            Event e = new()
            {
                ProcessesTimeStamp = key.Value
            };

            if (e == currE) continue;

            if (e < currE) 
            {
                past.Add(key.Key);
                continue;
            }

            if(currE <  e)
            {
                future.Add(key.Key);
                continue;
            }
            parallel.Add(key.Key);
        }

        Console.WriteLine("past: " + string.Join(", ", past));
        Console.WriteLine("future: " + string.Join(", ", future));
        Console.WriteLine("parallel: " + string.Join(", ", parallel));

    }
}