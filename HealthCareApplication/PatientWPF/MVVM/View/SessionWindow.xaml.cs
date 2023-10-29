﻿using LiveCharts.Wpf;
using LiveCharts;
using Newtonsoft.Json.Linq;
using PatientApp.DeviceConnection;
using PatientWPFApp.PatientLogic;
using System.Windows;
using System.Windows.Media;
using System;
using System.Threading;

namespace PatientWPF.MVVM.View
{
    /// <summary>
    /// Interaction logic for SessionWindow.xaml
    /// </summary>
    public partial class SessionWindow : Window
    {
        private readonly LineSeries _speedGraph;
        private readonly LineSeries _heartRateGraph;

        private bool _sessionActive = false;

        public SessionWindow(string patientName)
        {
            InitializeComponent();

            LineSeries speedGraph = new LineSeries()
            {
                Values = new ChartValues<double>()
            };
            StatChart.Series.Add(speedGraph);
            _speedGraph = StatChart.Series[0] as LineSeries;

            LineSeries heartRateGraph = new LineSeries()
            {
                Values = new ChartValues<int>()
            };
            StatChart.Series.Add(heartRateGraph);
            _heartRateGraph = StatChart.Series[1] as LineSeries;

            PatientNameText.Text = patientName;

            RequestHandler.SessionStarted += OnSessionStarted;
            RequestHandler.SessionStopped += OnSessionStopped;

            // Initialize BLE connection
            DeviceManager.Initialize().Wait();
        }

        private async void ToggleSessionButton_Click(object sender, RoutedEventArgs e)
        {
            JObject message;
            if (_sessionActive)
            {
                // Stop the session
                DeviceManager.OnReceiveData -= OnReceiveData;

                message = PatientFormat.SessionStopMessage();

                _sessionActive = false;
                ToggleSessionButton.Content = "Start session";
                ToggleSessionButton.Background = Brushes.LightGreen;

                SessionStatusText.Text = "Session stopped. Click the 'Start session' button to start a new training.";
                SessionStatusText.Background = Brushes.Azure;

                EmergencyButton.IsEnabled = false;
            }
            else
            {
                // Start a new session
                DeviceManager.OnReceiveData += OnReceiveData;

                message = PatientFormat.SessionStartMessage();

                _sessionActive = true;
                ToggleSessionButton.Content = "Stop session";
                ToggleSessionButton.Background = Brushes.Salmon;

                SessionStatusText.Text = "Training is in progress.";
                SessionStatusText.Background = Brushes.LightSalmon;

                EmergencyButton.IsEnabled = true;
            }
            await ClientConn.SendJson(message);
        }

        private void SetResistanceButton_Click(object sender, RoutedEventArgs e)
        {
            TrainerResistanceInput.Text = "";
        }

        private void StopExitButton_Click(object sender, RoutedEventArgs e)
        {
            JObject stopMessage = PatientFormat.SessionStopMessage();
            ClientConn.SendJson(stopMessage).Wait();

            // TODO fix the server side issue of throwing an exception when the connection closes.
            ClientConn.CloseConnection();
            Close();
        }

        private void OnReceiveData(object _, Statistic stat)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                double speedKmH = Math.Round(stat.Speed * 3.6, 1);
                CurrentSpeedText.Text = speedKmH.ToString();

                int oldDistance = int.Parse(CurrentDistanceText.Text);
                int newDistance = oldDistance + stat.Distance;
                CurrentDistanceText.Text = newDistance.ToString();

                CurrentHeartRateText.Text = stat.HeartRate.ToString();

                _speedGraph.Values.Add(speedKmH);
                _heartRateGraph.Values.Add((stat.HeartRate - 80) / 4);

                // Sending data in the same thread causes the UI thread to completely freeze
                // To prevent this send data through a different thread
                Thread t = new Thread(async () => await ClientConn.SendJson(PatientFormat.StatsSendMessage(stat)));
                t.Start();
            });
        }

        private async void OnSessionStarted(object _, bool __)
        {
            // Method gets called on a different thread than the current UI thread.
            // Therefore invoke this method within a lambda to make it possible

            Application.Current.Dispatcher.Invoke(() =>
            {

                if (_sessionActive) return;

                // Start a new session
                DeviceManager.OnReceiveData += OnReceiveData;

                _sessionActive = true;
                ToggleSessionButton.Content = "Stop session";
                ToggleSessionButton.Background = Brushes.Salmon;

                SessionStatusText.Text = "Training is in progress.";
                SessionStatusText.Background = Brushes.LightSalmon;

                EmergencyButton.IsEnabled = true;

                // Sending data in the same thread causes the UI thread to completely freeze
                // To prevent this send data through a different thread
                Thread t = new Thread(async() => await ClientConn.SendJson(PatientFormat.SessionStartMessage()));
                t.Start();
            });


        }

        private async void OnSessionStopped(object _, bool __)
        {
            // Method gets called on a different thread than the current UI thread.
            // Therefore invoke this method within a lambda to make it possible

            Application.Current.Dispatcher.Invoke(() =>
            {

                if (!_sessionActive) return;

                // Stop the session
                DeviceManager.OnReceiveData -= OnReceiveData;

                _sessionActive = false;
                ToggleSessionButton.Content = "Start session";
                ToggleSessionButton.Background = Brushes.LightGreen;

                SessionStatusText.Text = "Session stopped. Click the 'Start session' button to start a new training.";
                SessionStatusText.Background = Brushes.Azure;

                EmergencyButton.IsEnabled = false;

                Thread t = new Thread(async () => await ClientConn.SendJson(PatientFormat.SessionStopMessage()));
                t.Start();
            });

        }
    }
}