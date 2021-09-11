using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using AlarmClock.Model;
using Newtonsoft.Json;

namespace AlarmClock
{
    public static class AlarmManager
    {
        public static event EventHandler AlarmRaised;
        private static List<Alarm> Alarms = new List<Alarm>();
        public static List<Alarm> ActiveAlarms => Alarms.FindAll(q => !q.IsDisabled && q.IsActive);
        private static string _alarmsJsonPath = "";
        static AlarmManager()
        {
            var applicationName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
            var alarmsJson = "Alarms.json";
            var applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                applicationName);
            if (!Directory.Exists(applicationDataPath))
            {
                Directory.CreateDirectory(applicationDataPath);
            }

            _alarmsJsonPath = Path.Combine(applicationDataPath, alarmsJson);
            if (File.Exists(_alarmsJsonPath))
            {
                var json = File.ReadAllText(_alarmsJsonPath);
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
            if (File.Exists(_alarmsJsonPath))
                File.Delete(_alarmsJsonPath);
            File.WriteAllText(_alarmsJsonPath, serializeObject);
        }

        public static List<Alarm> GetAlarms()
        {
            return Alarms.ToList();
        }

        public static Alarm GetNextAlarm()
        {
            if (!ActiveAlarms.Any()) return null;
            return ActiveAlarms.OrderBy(q=>q.TimeLeft).FirstOrDefault();
        }


        private static void OnAlarmRaised(Alarm alarm)
        {
            AlarmRaised?.Invoke(alarm, EventArgs.Empty);
        }

    }
}