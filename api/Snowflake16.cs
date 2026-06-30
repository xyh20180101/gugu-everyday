namespace GuguEveryday;

public class Snowflake16
{
    private static readonly DateTime Epoch = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private readonly object _lock = new();
    private long _lastTimestamp = -1L;
    private long _sequence = 0;

    private const int SequenceBits = 12;
    private const long SequenceMask = (1 << SequenceBits) - 1;

    public long NextId()
    {
        lock (_lock)
        {
            long timestamp = (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;

            if (timestamp == _lastTimestamp)
            {
                _sequence = (_sequence + 1) & SequenceMask;
                if (_sequence == 0)
                {
                    while (timestamp <= _lastTimestamp)
                    {
                        Thread.Sleep(1);
                        timestamp = (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;
                    }
                }
            }
            else
            {
                _sequence = 0;
            }

            _lastTimestamp = timestamp;

            long id = (timestamp << SequenceBits) | _sequence;
            return id;
        }
    }
}
