using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        OpenFileDialog opDialog = new OpenFileDialog();
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
            opDialog.Filter = "Wave File(*.wav)|*.wav";
            AlarmDateTimePicker.SelectedDateTime = DateTime.Now;
            AlarmTimePicker.SelectedDateTime = DateTime.Now;
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
                return $"{Math.Ceiling(timeSpan.TotalDays):####} days";
            }
            if (timeSpan.TotalHours > 1)
            {
                return $"{Math.Ceiling(timeSpan.TotalHours):####} hrs";
            }
            if (timeSpan.TotalMinutes > 1)
            {
                return $"{Math.Ceiling(timeSpan.TotalMinutes):####} mins";
            }

            return $"{Math.Ceiling(timeSpan.TotalSeconds):####} secs";

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
            PopulateList();
            _isInAlarmScreen = AlarmClockToggle.IsChecked.Value;
            ClockGrid.Visibility = !_isInAlarmScreen ? Visibility.Visible : Visibility.Hidden;
            AlarmGrid.Visibility = _isInAlarmScreen ? Visibility.Visible : Visibility.Hidden;
            AlarmDateTimePicker.SelectedDateTime = DateTime.Now;
            AlarmDateTimePicker.DisplayDateStart = DateTime.Now;
            AlarmClockToggle.Content = GetTextForAlarmClockToggle();
        }

        private void IsEverydayCheckBoxOnClick(object sender, RoutedEventArgs e)
        {
            IsEverydayCheckBoxChanged();
        }

        private void IsEverydayCheckBoxChanged()
        {
            _isEveryday = IsEverydayCheckBox.IsChecked.Value;
            AlarmTimePicker.Visibility = _isEveryday ? Visibility.Visible : Visibility.Collapsed;
            AlarmDateTimePicker.Visibility = _isEveryday ? Visibility.Collapsed : Visibility.Visible;
        }
        private void SetAlarmSoundButtonOnClick(object sender, RoutedEventArgs e)
        {
            var dialogResult = opDialog.ShowDialog();
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {

            }
        }
        private void SetAlarmButtonOnClick(object sender, RoutedEventArgs e)
        {
            Alarm alarm = null;
            if (_isEveryday)
            {
                if (AlarmTimePicker.SelectedDateTime.HasValue)
                {
                    var now = DateTime.Now;
                    var date = AlarmTimePicker.SelectedDateTime.Value;
                    var newDate = new DateTime(now.Year, now.Month, now.Day, date.Hour, date.Minute, date.Second);
                    alarm = new Alarm(_isEveryday, newDate);
                    AlarmManager.SetAlarm(alarm);
                }
            }
            else
            {
                if (AlarmDateTimePicker.SelectedDateTime.HasValue)
                {
                    alarm = new Alarm(_isEveryday, AlarmDateTimePicker.SelectedDateTime.Value);
                    AlarmManager.SetAlarm(alarm);
                }
            }
            if (!string.IsNullOrEmpty(opDialog.FileName))
                alarm.AlarmFile = opDialog.FileName;
            opDialog.FileName = "";
            PopulateList();
        }

        private void PopulateList()
        {
            AlarmDataGrid.ItemsSource = AlarmManager.GetAlarms();
            AlarmDataGrid.AutoGenerateColumns = true;
            AlarmDataGrid.Columns[0].Visibility = Visibility.Hidden;
            AlarmDataGrid.Columns[1].Visibility = Visibility.Hidden;
            AlarmDataGrid.Columns[2].Header = "Every Day";
            AlarmDataGrid.Columns[2].IsReadOnly = true;
            AlarmDataGrid.Columns[3].Visibility = Visibility.Hidden;
            AlarmDataGrid.Columns[4].Visibility = Visibility.Hidden;
            AlarmDataGrid.Columns[5].Header = "Date Time";
            AlarmDataGrid.Columns[5].IsReadOnly = true;
            AlarmDataGrid.Columns[6].Header = "Disabled";
            AlarmDataGrid.Columns[6].IsReadOnly = false;
            AlarmDataGrid.Columns[7].Visibility = Visibility.Hidden;
            AlarmDataGrid.Columns[8].Visibility = Visibility.Hidden;
            AlarmDataGrid.Columns[9].Visibility = Visibility.Hidden;

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
            _contextMenu.Items.Add(caption, null, OnClick);
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


        private void AlarmDataGridOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = AlarmDataGrid.SelectedItem as Alarm;
            if (selectedItem == null) return;
            IsEverydayCheckBox.IsChecked = selectedItem.IsEveryday;
            AlarmDateTimePicker.SelectedDateTime = selectedItem.DateTime;
            AlarmTimePicker.SelectedDateTime = selectedItem.DateTime;
            AlarmManager.RemoveAlarm(selectedItem);
            IsEverydayCheckBoxChanged();
            PopulateList();
        }


    }
}
