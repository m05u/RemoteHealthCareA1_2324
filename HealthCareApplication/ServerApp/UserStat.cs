﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp
{
    [Serializable] // Make class serializable so stats can be send easily to the doctor 
    public class UserStat
    {
        public float speed { get; set; }
        public int distance { get; set; }
        public byte heartrate { get; set; }
        public UserStat(float speed, int distance, byte heartrate)
        {
            this.distance = distance;
            this.speed = speed;
            this.heartrate = heartrate;
        }

        //public string ToString()
        //{
        //    return $"Speed: {speed}, --- Distance: {distance}, --- Heartrate: {heartrate}";
   
        //}

    }
}
