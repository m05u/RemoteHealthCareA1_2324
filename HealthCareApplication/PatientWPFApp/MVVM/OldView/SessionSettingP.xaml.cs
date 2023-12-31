﻿using System.Windows;

namespace PatientWPFApp.View
{
    
    public partial class SessionSettingP : Window
    {
        public bool isRunning = false;
        public SessionSettingP()
        {
            InitializeComponent();
        }

        private void chatsBtn_Click(object sender, RoutedEventArgs e)
        {
            ChatP chats = new ChatP();
            Close();
            chats.Show();
        }

        //todo data required
        private void summaryBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Averege speed: " + "\n" + "Distance: " + "\n" + "Average heartrate: ", "Summary so far!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void stopstartBtn_Click(object sender, RoutedEventArgs e)
        {

            if (isRunning == true)
            {
                isRunning = false;
                stopstartBtn.Content = "Start";
                stopstartBtn.Background = System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
                isRunning = true;
                stopstartBtn.Content = "Stop";
                stopstartBtn.Background = System.Windows.Media.Brushes.Red;
            }

        }
    }
}
