using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using AlarmClock.Models;
using AlarmClock.Properties;

namespace AlarmClock.Model
{
    public class Alarm : IDisposable
    {
        public event EventHandler AlarmRaised;
        Timer _timer = new Timer(1000);
        public Alarm(bool isEveryday, DateTime dateTime)
        {
            IsEveryday = isEveryday;
            DateTime = dateTime;
            Snooze = new Snooze(dateTime.AddMinutes(5));
            Id = Guid.NewGuid().ToString();
            AlarmFile = Settings.Default.DefaultAlarmFile;
            _timer.Start();
            _timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (IsDisabled || !IsActive)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _timer.Stop();
                });
            }
            if (Convert.ToInt64(TimeLeft.TotalSeconds) == 0)
            {
                Application.Current.Dispatcher.Invoke(() => { AlarmRaised?.Invoke(this, EventArgs.Empty); });

            }
        }

        public TimeSpan TimeLeft
        {
            get
            {
                if (IsEveryday)
                {

                    var dateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Hour,
                        DateTime.Minute, DateTime.Second);
                    var timeLeft = dateTime - DateTime.Now;
                    if (timeLeft.TotalSeconds > 0)
                        return timeLeft;
                    dateTime = dateTime.AddDays(1);
                    timeLeft = dateTime - DateTime.Now;
                    return timeLeft;
                }
                return DateTime - DateTime.Now;
            }
        }

        public string Id { get; set; }
        public bool IsEveryday { get; set; }
        public DateTime DateTime { get; set; }
        public TimeSpan SnoozeTime { get; set; }
        public Snooze Snooze { get; set; }
        public string DisplayDateTime => !IsEveryday ? DateTime.ToString() : DateTime.ToShortTimeString();
        public bool IsDisabled { get; set; }
        public bool IsActive => TimeLeft.TotalSeconds > 0;
        public string AlarmFile { get; set; }
        public List<string> Days { get; set; }

        protected virtual void OnAlarmRaised()
        {
            AlarmRaised?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Snooze.Dispose();
            _timer?.Dispose();
        }


    }
}