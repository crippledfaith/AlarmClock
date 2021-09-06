using System;
using System.Timers;
using System.Windows;

namespace AlarmClock.Model
{
    public class Snooze : IDisposable
    {
        private bool _isActive = false;
        public event EventHandler SnoozeRaised;
        public DateTime SnoozeDateTime { get; set; }
        public TimeSpan SnoozeTimeSpan => SnoozeDateTime - DateTime.Now;
        public bool Paused { get; set; }
        Timer _timer = new Timer(600);

        public Snooze(DateTime snoozeDateTime)
        {
            _timer.Elapsed += TimerElapsed;
            SnoozeDateTime = snoozeDateTime;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            var totalSeconds = Convert.ToInt64(SnoozeTimeSpan.TotalSeconds);
            if (totalSeconds < 1 && totalSeconds > -1)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsActive = false;
                    OnSnoozeRaised();
                });
            }
        }

        public bool IsActive
        {
            get
            {

                return _isActive;

            }
            set
            {

                _isActive = value;
                if (_isActive)
                {
                    _timer.Start();
                }
                else
                {
                    _timer.Stop();
                }
            }
        }

        protected virtual void OnSnoozeRaised()
        {
            SnoozeRaised?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}