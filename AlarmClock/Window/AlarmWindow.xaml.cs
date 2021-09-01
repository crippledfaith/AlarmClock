using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AlarmClock.Model;

namespace AlarmClock
{
    /// <summary>
    /// Interaction logic for AlarmWindow.xaml
    /// </summary>
    public partial class AlarmWindow
    {
        private SoundPlayer player;
        private Alarm alarm;
        private Timer timer = new Timer(5000);
        public AlarmWindow(Alarm alarm)
        {
            InitializeComponent();
            this.alarm = alarm;
            TimeLabel.Content = DateTime.Now.ToString();
            alarm.Snooze.SnoozeRaised += SnoozeSnoozeRaised;
            player = new SoundPlayer(alarm.AlarmFile);
            player.PlayLooping();
            timer.Start();
            timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                timer.Interval = 1000;
                TimeLabel.Content = DateTime.Now.ToString();
            });

        }

        private void SnoozeSnoozeRaised(object sender, EventArgs e)
        {
            player.PlayLooping();
            this.Show();
        }

        private void StopButtonOnClick(object sender, RoutedEventArgs e)
        {
            player.Stop();
            this.Close();
        }

        private void SnoozeButtonOnClick(object sender, RoutedEventArgs e)
        {
            alarm.Snooze.SnoozeDateTime = DateTime.Now.AddMinutes(5);
            alarm.Snooze.IsActive = true;
            player.Stop();
            this.Hide();
        }
    }
}
