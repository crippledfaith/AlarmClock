using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Documents;
using AlarmClock.Properties;
using Newtonsoft.Json;

namespace AlarmClock
{
    public class Snooze : IDisposable
    {
        private bool _isActive = false;
        public event EventHandler SnoozeRaised;
        public DateTime SnoozeDateTime { get; set; }
        public TimeSpan SnoozeTimeSpan => SnoozeDateTime - DateTime.Now;
        public bool Paused { get; set; }
        Timer _timer = new Timer(1000);

        public Snooze(DateTime snoozeDateTime)
        {
            _timer.Elapsed += TimerElapsed;
            SnoozeDateTime = snoozeDateTime;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (Convert.ToInt64(SnoozeTimeSpan.TotalSeconds) == 0)
            {
                Application.Current.Dispatcher.Invoke(()=>
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

    public static class AlarmManager
    {
        public static event EventHandler AlarmRaised;
        private static List<Alarm> Alarms = new List<Alarm>();
        public static List<Alarm> ActiveAlarms => Alarms.FindAll(q => !q.IsDisabled && q.IsActive);

        static AlarmManager()
        {
            var alarmsJson = "Alarms.json";
            if (File.Exists(alarmsJson))
            {
                var json = File.ReadAllText(alarmsJson);
                Alarms = JsonConvert.DeserializeObject<List<Alarm>>(json);
            }
        }
        public static void RemoveAlarm(Alarm alarm)
        {
            Alarms.Remove(alarm);
            alarm.AlarmRaised -= AlarmAlarmRaised;
            alarm.Dispose();
            SaveAlarm();
        }
        public static void SetAlarm(Alarm alarm)
        {
            Alarms.Add(alarm);
            alarm.AlarmRaised += AlarmAlarmRaised;
            Alarms = Alarms.OrderBy(q => q.TimeLeft).ToList();
            SaveAlarm();

        }

        private static void AlarmAlarmRaised(object sender, EventArgs e)
        {
            OnAlarmRaised((Alarm)sender);
        }

        private static void SaveAlarm()
        {
            var serializeObject = JsonConvert.SerializeObject(Alarms, Formatting.Indented);
            var alarmsJson = "Alarms.json";
            if (File.Exists(alarmsJson))
                File.Delete(alarmsJson);
            File.WriteAllText(alarmsJson, serializeObject);
        }

        public static List<Alarm> GetAlarms()
        {
            return Alarms.ToList();
        }

        public static Alarm GetNextAlarm()
        {
            if (!ActiveAlarms.Any()) return null;
            return ActiveAlarms.FirstOrDefault();
        }


        private static void OnAlarmRaised(Alarm alarm)
        {
            AlarmRaised?.Invoke(alarm, EventArgs.Empty);
        }

    }


}