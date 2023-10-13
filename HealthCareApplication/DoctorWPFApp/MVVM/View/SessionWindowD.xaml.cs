﻿using DoctorWPFApp.MVVM.ViewModel;
using System.Windows;

namespace DoctorWPFApp.MVVM.View
{
    public partial class SessionWindowD : Window
    {
        public bool isRunning = false;
        public SessionWindowD()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void chatsBtn_Click(object sender, RoutedEventArgs e)
        {
            ChatWindowD chats = new ChatWindowD();
            Hide();
            chats.Show();
        }

        private void statsBtn_Click(object sender, RoutedEventArgs e)
        {
            StatsWindowD stats = new StatsWindowD();
            Hide();
            stats.Show();
        }

        private void stopstartBtn_Click(object sender, RoutedEventArgs e)
        {
            
            if(isRunning == true)
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