using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AlarmClock.Models;
using Newtonsoft.Json;

namespace AlarmClock
{
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