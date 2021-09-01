using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using AlarmClock.Model;
using AlarmClock.Properties;
using MahApps.Metro.Controls;
using Application = System.Windows.Application;
using DateTime = System.DateTime;
using Timer = System.Timers.Timer;

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
        private readonly NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu = new ContextMenuStrip();
        public MainWindow()
        {
            InitializeComponent();
            DateLabel.Content = DateTime.Now.ToLongDateString();
            TimeLabel.Content = DateTime.Now.ToLongTimeString();
            _notifyIcon = new NotifyIcon();
            _clockTimer.Start();
            _clockTimer.Elapsed += ClockTimerElapsed;
            _updateSetTextTimer.Start();
            _updateSetTextTimer.Elapsed += UpdateSetTextTimerElapsed;
            AlarmClockToggle.Content = "No Alarms";
            AlarmManager.AlarmRaised += AlarmManagerAlarmRaised;
            _notifyIcon.Visible = true;
            _notifyIcon.Icon = Properties.Resources.icon;
            _notifyIcon.DoubleClick += NotifyIconDoubleClick;
            CreateIconMenuStructure("Open");
            CreateIconMenuStructure("Exit");
            _notifyIcon.ContextMenuStrip = _contextMenu;
            PopulateList();
        }

        private void AlarmManagerAlarmRaised(object sender, EventArgs e)
        {
            ExcuteRaiseAlarm((Alarm)sender);
        }

        private void ExcuteRaiseAlarm(Alarm alarm)
        {
            var alarmWindow = new AlarmWindow(alarm);
            alarmWindow.Show();
        }

        private void UpdateSetTextTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (Application.Current != null)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var textForAlarmClockToggle = GetTextForAlarmClockToggle();
                    AlarmClockToggle.Content = textForAlarmClockToggle;
                    _notifyIcon.Text = textForAlarmClockToggle;
                });
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


        private void NotifyIconDoubleClick(object sender, EventArgs e)
        {
            
            this.Show();
            this.WindowState = WindowState.Normal;
            ShowInTaskbar = true;
            this.Activate();

        }

        private void MainWindowOnStateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
                ShowInTaskbar = false;
            }

        }

        public void CreateIconMenuStructure(string caption)
        {
            _contextMenu.Items.Add(caption,null, OnClick);
        }

        private void OnClick(object sender, EventArgs e)
        {
            var menuItem = (ToolStripMenuItem)sender;
            switch (menuItem.Text)
            {
                case "Exit":
                    Application.Current.Shutdown(0);
                    break;
                case "Open":
                    this.Show();
                    this.WindowState = WindowState.Normal;
                    ShowInTaskbar = true;
                    this.Activate();
                    break;
            }
        }

        private void MainWindowOnClosing(object sender, CancelEventArgs e)
        {
            _notifyIcon.Visible = false;
        }
    }
}
