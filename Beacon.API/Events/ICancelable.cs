namespace Beacon.API.Events;

public interface ICancelable
{
    public bool IsCancelled { get; set; }
}
