namespace EmailService.Worker.Health;

public sealed class ConsumerHealthState
{
    private int _started;     // 0/1
    private int _consuming;   // 0/1
    private long _lastMessageUtcTicks; // DateTime.UtcNow.Ticks
    private string? _lastError;

    public bool Started => Volatile.Read(ref _started) == 1;
    public bool Consuming => Volatile.Read(ref _consuming) == 1;

    public DateTime LastMessageUtc
    {
        get
        {
            long ticks = Interlocked.Read(ref _lastMessageUtcTicks);
            return ticks <= 0 ? DateTime.MinValue : new DateTime(ticks, DateTimeKind.Utc);
        }
    }

    public string? LastError => Volatile.Read(ref _lastError);

    public void MarkStarted()
    {
        Interlocked.Exchange(ref _started, 1);
    }

    public void MarkConsuming(bool consuming)
    {
        Interlocked.Exchange(ref _consuming, consuming ? 1 : 0);
    }

    public void MarkMessage()
    {
        Interlocked.Exchange(ref _lastMessageUtcTicks, DateTime.UtcNow.Ticks);
    }

    public void MarkError(string? error)
    {
        Volatile.Write(ref _lastError, error);
    }
}
