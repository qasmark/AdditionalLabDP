namespace DistributedSystem.Logger.Repository;

public interface IRepository
{
    void Save(string id, List<int> processesTimeStamp);
}