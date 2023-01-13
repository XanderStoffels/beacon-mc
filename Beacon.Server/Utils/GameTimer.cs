using System.Diagnostics;

namespace Beacon.Server.Utils;


/**
 *<summary>
 * A utility class that can be used to (somewhat) accurately wait for timer ticks in an asynchronous way.
 * Due to the nature of <see cref="!:Task.Delay"/>, a timer tick may be delayed by up to 15ms.
 * </summary>
 * <remarks>Credit to the Obsidian team! This is a modified version of their BalancedTimer class.</remarks>
 */
public class GameTimer
{
    private readonly CancellationToken _cancellationToken;
    private readonly Stopwatch _stopwatch;
    private readonly long _ticksInterval; // Number of Stopwatch ticks equal to the interval

    private long _delay;  // Measured in ticks

    public GameTimer(int intervalInMilliseconds) : this(intervalInMilliseconds, CancellationToken.None)
    {
    }

    public GameTimer(int intervalInMilliseconds, CancellationToken cancellationToken)
    {
        if (intervalInMilliseconds < 1)
            throw new ArgumentOutOfRangeException(nameof(intervalInMilliseconds));
        
        _stopwatch = new();
        _cancellationToken = cancellationToken;
        _ticksInterval = intervalInMilliseconds * Stopwatch.Frequency / 1000L;
    }

    /**
     <summary>
         Wait for the next tick.
         Returns the amount of delay in milliseconds the timer had to wait for the next tick.
         If this number approaches 0, it could mean the server is overworked.
     </summary>
     */
    public async ValueTask<int> WaitForNextTickAsync()
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var delta = _stopwatch.ElapsedTicks;
        _stopwatch.Restart();

        // Measure delay
        _delay += delta - _ticksInterval;

        if (_delay >= 0) return 0;

        // Wait for the extra time
        var extraTimeInMilliseconds = (int)(-_delay * 1000L / Stopwatch.Frequency);
        await Task.Delay(extraTimeInMilliseconds, _cancellationToken);

        return extraTimeInMilliseconds;
    }
}