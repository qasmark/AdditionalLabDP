using StackExchange.Redis;
using System.Text.Json;

namespace DistributedSystem.Logger.Repository;

public class RedisRepository : IRepository
{
    private readonly IDatabase _db;

    public RedisRepository(string connectionString)
    {
        var connection = ConnectionMultiplexer.Connect(connectionString);
        _db = connection.GetDatabase();
    }

    public void Save(Dictionary<string, List<int>> data)
    {
        string id = ConnectionMultiplexer.Connect("localhost:6379")
            .GetServer("localhost:6379").Keys(pattern: "*").Count().ToString();
        string timesStr = JsonSerializer.Serialize(data);
        _db.StringSetAsync(id, timesStr);
    }
}