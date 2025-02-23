using System.Timers;

namespace Atoz;

internal class ExponentialBackoffTimer : IDisposable
{
    private readonly System.Timers.Timer _timer;
    private readonly TimeSpan _initInterval;
    private readonly int _power;
    private TimeSpan _prevInterval;

    public event ElapsedEventHandler OnTick;

    public ExponentialBackoffTimer(TimeSpan initInterval, int power)
    {
        _power = power;
        _initInterval = initInterval;
        _prevInterval = initInterval;

        _timer = new System.Timers.Timer(_initInterval);
        _timer.Enabled = false;

        _timer.Elapsed += (s, e) =>
        {
            _prevInterval = _prevInterval.Multiply(_power);

            _timer.Enabled = false;
            _timer.Interval = _prevInterval.Milliseconds;
            _timer.Enabled = true;

            OnTick?.Invoke(s, e);
        };
    }

    public void Start()
    {
        _timer.Enabled = true;
    }

    public void Stop()
    {
        _timer.Enabled = false;
    }

    public void Reset()
    {
        Stop();
        _prevInterval = TimeSpan.Zero;
        Start();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
