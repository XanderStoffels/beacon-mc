namespace Beacon.API.Events;

public interface ICancellable
{
    public bool IsCanceled { get; set; }
}