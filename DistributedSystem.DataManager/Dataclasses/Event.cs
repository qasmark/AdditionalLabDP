namespace DistributedSystem.DataManager.Dataclasses;

public class Event
{
    public string Id { get; set; }
    public List<int> ProcessesTimeStamp { get; set; }
    
    
    public static bool operator==(Event a, Event b)
    {
        if (a.ProcessesTimeStamp.Count != b.ProcessesTimeStamp.Count) 
            return false;

        for(int i = 0; i < a.ProcessesTimeStamp.Count;i++)
        {
            if (a.ProcessesTimeStamp[i] != b.ProcessesTimeStamp[i]) return false;
        }
        return true;
    }
    public static bool operator!=(Event a, Event b)
    {
        return !(a == b);
    }

    public static bool operator<=(Event a, Event b)
    {
        for (int i = 0; i < a.ProcessesTimeStamp.Count; i++)
        {
            if (a.ProcessesTimeStamp[i] > b.ProcessesTimeStamp[i])
                return false;
        }
        return true;
    }
    
    public static bool operator<(Event a, Event b)
    {
        bool haveLess = false;

        for (int i = 0; i < a.ProcessesTimeStamp.Count; i++)
        {
            if (a.ProcessesTimeStamp[i] < b.ProcessesTimeStamp[i])
            {
                haveLess = true;
            }
        }
        return a <= b && haveLess;
    }

    public static bool operator>=(Event a, Event b)
    {
        return b <= a;
    }

    public static bool operator>(Event a, Event b)
    {
        return b < a;

    }
}