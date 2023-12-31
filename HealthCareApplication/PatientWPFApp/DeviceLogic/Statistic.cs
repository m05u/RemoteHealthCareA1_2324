﻿using System;

namespace PatientApp.DeviceConnection
{
    public class Statistic
    {
        public double Speed;
        public int Distance;
        public int HeartRate;
        public int[] RrIntervals;

        public Statistic()
        {
            Speed = -1;
            Distance = -1;
            HeartRate = -1;
            RrIntervals = new int[0];
        }

        public bool IsComplete()
        {
            return Speed != -1 && Distance != -1 &&
                HeartRate != -1 && RrIntervals != new int[0];
        }
    }
}
