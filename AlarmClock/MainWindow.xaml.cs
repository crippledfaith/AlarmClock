using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using DateTime = System.DateTime;

namespace AlarmClock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {


        private readonly Timer _clockTimer = new Timer(1000);
        private readonly Timer _updateSetTextTimer = new Timer(1000);
        private bool _isInAlarmScreen = false;
        private bool _isEveryday = false;

        public MainWindow()
        {
            InitializeComponent();
            DateLabel.Content = DateTime.Now.ToLongDateString();
            TimeLabel.Content = DateTime.Now.ToLongTimeString();
            _clockTimer.Start();
            _clockTimer.Elapsed += ClockTimerElapsed;
            _updateSetTextTimer.Start();
            _updateSetTextTimer.Elapsed += UpdateSetTextTimerElapsed;
            AlarmClockToggle.Content = "No Alarms";
            AlarmManager.AlarmRaised += AlarmManagerAlarmRaised;
            PopulateList();
        }

        private void AlarmManagerAlarmRaised(object sender, EventArgs e)
        {
            ExcuteRaiseAlarm((Alarm) sender);
        }

        private void ExcuteRaiseAlarm(Alarm alarm)
        {
            var alarmWindow = new AlarmWindow(alarm);
            alarmWindow.Show();
        }

        private void UpdateSetTextTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(() => { AlarmClockToggle.Content = GetTextForAlarmClockToggle(); });
        }

        private string GetTextForAlarmClockToggle()
        {
            if (_isInAlarmScreen)
            {
                return DateTime.Now.ToString();
            }
            else
            {
                var latestAlarm = GetLatestAlarm();
                if (latestAlarm == null)
                {
                    return $"No Alarms";
                }
                else
                {

                    return $"Next Alarm in {GetTimeText(latestAlarm.TimeLeft)}";
                }
            }

        }

        private string GetTimeText(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays > 1)
            {
                return $"{Math.Round(timeSpan.TotalDays):####} days";
            }
            if (timeSpan.TotalHours > 1)
            {
                return $"{Math.Round(timeSpan.TotalHours):####} hrs";
            }
            if (timeSpan.TotalMinutes > 1)
            {
                return $"{Math.Round(timeSpan.TotalMinutes):####} mins";
            }

            return $"{Math.Round(timeSpan.TotalSeconds):####} secs";

        }

        private Alarm GetLatestAlarm()
        {
            return AlarmManager.GetNextAlarm();
        }

        private void ClockTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (Application.Current != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    DateLabel.Content = DateTime.Now.ToLongDateString();
                    TimeLabel.Content = DateTime.Now.ToLongTimeString();
                });
            }
        }




        private void AlarmClockToggleClick(object sender, RoutedEventArgs e)
        {
            _isInAlarmScreen = AlarmClockToggle.IsChecked.Value;
            ClockGrid.Visibility = !_isInAlarmScreen ? Visibility.Visible : Visibility.Hidden;
            AlarmGrid.Visibility = _isInAlarmScreen ? Visibility.Visible : Visibility.Hidden;
            AlarmClockToggle.Content = GetTextForAlarmClockToggle();
        }

        private void IsEverydayCheckBoxOnClick(object sender, RoutedEventArgs e)
        {
            _isEveryday = IsEverydayCheckBox.IsChecked.Value;
            AlarmTimePicker.Visibility = _isEveryday ? Visibility.Visible : Visibility.Collapsed;
            AlarmDateTimePicker.Visibility = _isEveryday ? Visibility.Collapsed : Visibility.Visible;
        }

        private void SetAlarmButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (_isEveryday)
            {
                if (AlarmTimePicker.SelectedDateTime.HasValue)
                {
                    AlarmManager.SetAlarm(new Alarm(_isEveryday, AlarmTimePicker.SelectedDateTime.Value));
                }
            }
            else
            {
                if (AlarmDateTimePicker.SelectedDateTime.HasValue)
                {
                    AlarmManager.SetAlarm(new Alarm(_isEveryday, AlarmDateTimePicker.SelectedDateTime.Value));
                }
            }

            PopulateList();
        }

        private void PopulateList()
        {
            AlarmListbox.Items.Clear();
            foreach (var alarm in AlarmManager.GetAlarms())
            {
                AlarmListbox.Items.Add(alarm);
            }
        }

        private void AlarmListboxOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            AlarmManager.RemoveAlarm((Alarm)AlarmListbox.SelectedItem);
            PopulateList();
        }

        private void About(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            aboutWindow.ShowDialog();
        }
    }
}
