namespace Beacon.Net;

public class KeepAliver(TimeSpan interval) : IDisposable
{
    /// <summary>
    /// The keep alive started waiting for a call to <see cref="KeepAlive"/>.
    /// If no call is made before the deadline, the <see cref="TimerExpired"/> event is raised.
    /// </summary>
    public event EventHandler? IntervalStarted;
    
    /// <summary>
    /// The keep alive timer was expired.
    /// </summary>
    public event EventHandler? TimerExpired;

    public bool IsRunning { get; private set; }
    public long Key { get; private set; } = 0;
    
    private DateTimeOffset? _lastKeepAlive;
    private  CancellationTokenSource _cancellationTokenSource = new();

    public void Start()
    {
        if (IsRunning)
            throw new InvalidOperationException("Keep alive is already running.");
        
        IsRunning = true;
        
        var token = _cancellationTokenSource.Token;

        Task.Run(KeepAliveLoop, token);
        return;

        async Task KeepAliveLoop()
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Generate a new key.
                    Key = DateTimeOffset.UtcNow.Ticks;

                    Console.WriteLine("Waiting for keep alive...");
                    IntervalStarted?.Invoke(this, EventArgs.Empty);
                    await Task.Delay(interval, token);

                    if (token.IsCancellationRequested)
                        return;

                    if (_lastKeepAlive is null)
                    {
                        TimerExpired?.Invoke(this, EventArgs.Empty);
                        break;
                    }

                    if (_lastKeepAlive.Value >= DateTimeOffset.UtcNow - interval) continue;
                    TimerExpired?.Invoke(this, EventArgs.Empty);
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                // We don't care about this exception, it's expected.
            } 
            finally
            {
                IsRunning = false;
            } 
        }
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        _lastKeepAlive = null;
        if (_cancellationTokenSource.TryReset()) return;
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public bool KeepAlive(long key)
    {
        if (!IsRunning)
            throw new InvalidOperationException("Keep alive is not running.");

        if (key != Key)
            return false;

        Console.WriteLine("Keeping alive!");
        _lastKeepAlive = DateTimeOffset.UtcNow;
        return true;
    }
    
    
    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
    
}