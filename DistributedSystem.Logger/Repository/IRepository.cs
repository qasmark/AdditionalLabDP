namespace DistributedSystem.Logger.Repository;

public interface IRepository
{
    void Save(Dictionary<string, List<int>> data);
}