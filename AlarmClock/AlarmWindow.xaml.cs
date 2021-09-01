using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AlarmClock
{
    /// <summary>
    /// Interaction logic for AlarmWindow.xaml
    /// </summary>
    public partial class AlarmWindow
    {
        private SoundPlayer player;
        private Alarm alarm;
        public AlarmWindow(Alarm alarm)
        {
            InitializeComponent();
            this.alarm = alarm;
            TimeLabel.Content = DateTime.Now.ToString();
            alarm.Snooze.SnoozeRaised += SnoozeSnoozeRaised;
            player = new SoundPlayer(alarm.AlarmFile);
            player.PlayLooping();
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
