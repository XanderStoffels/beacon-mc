namespace Beacon.API.Events;

public enum Priority
{
    Fastest = int.MinValue,
    Faster = -1000,
    Fast = -100,
    Normal = 0,
    Low = 100,
    Lower = 10000,
    Lowest = int.MaxValue
}