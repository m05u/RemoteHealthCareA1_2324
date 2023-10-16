﻿using DoctorWPFApp.MVVM.ViewModel;
using System.Windows;

namespace DoctorWPFApp.MVVM.View
{
    /// <summary>
    /// Create a 
    /// </summary>
    public partial class LoginWindowD : Window
    {
        public LoginWindowD()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            DataContext = new MainWindowViewModel();
        }

        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (gbBox.Text.ToString() == "super" && wwBox.Password.ToString() == "sexy")
            {
                Navigator.navToSessionWindow(this);
            }
            else
            {
                MessageBox.Show("Wrong username or password.", "ERROR!", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }


    }
}