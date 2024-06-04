namespace DistributedSystem.Process.Dataclasses;

public class Message
{
    public string Id { get; set; }
    public string Msg { get; set; }
    public List<int> ProcessesTimeStamp { get; set; }
}