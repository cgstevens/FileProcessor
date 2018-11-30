using System;
using System.Threading;
using Timer = System.Timers.Timer;

namespace Shared.Helpers
{
    public class Waiter
    {
        private readonly Timer _timer;
        private readonly EventWaitHandle _waitHandle;

        public Waiter(TimeSpan? interval = null)
        {
            _waitHandle = new AutoResetEvent(false);
            _timer = new Timer();
            _timer.Elapsed += (sender, args) => _waitHandle.Set();
            SetInterval(interval);
        }

        public TimeSpan Interval
        {
            set => _timer.Interval = value.TotalMilliseconds;
        }

        public void Wait(TimeSpan? newInterval = null)
        {
            SetInterval(newInterval);
            _timer.Start();
            _waitHandle.WaitOne();
            _timer.Close();
            _waitHandle.Reset();
        }

        private void SetInterval(TimeSpan? newInterval)
        {
            if (newInterval.HasValue)
            {
                Interval = newInterval.Value;
            }
        }
    }
}
