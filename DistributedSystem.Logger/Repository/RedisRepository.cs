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

    public void Save(string id, List<int> processesTimeStamp)
    {
        string timesStr = JsonSerializer.Serialize(processesTimeStamp);
        _db.StringSet(id, timesStr);
    }
}