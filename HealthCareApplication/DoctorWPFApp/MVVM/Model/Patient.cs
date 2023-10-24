﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorWPFApp.MVVM.Model
{
    internal class Patient
    {
        public string Name { get; set; }

        public double Speed { get; set; }

        public double Distance { get; set; }
        public double HeartRate {  get; set; }
        public double RrIntervals { get; set; }
        public double Time { get; set; }

        public List<double> Speedlist { get; set; }
        public List<double> Distancelist { get; set; }
        public List<double> HeartRatelist { get; set; }
        public List<double> RrIntervalslist { get; set; }
        public List<double> Timelist { get; set; }

        public List<String> ChatMessages { get; set; }
    }
}
