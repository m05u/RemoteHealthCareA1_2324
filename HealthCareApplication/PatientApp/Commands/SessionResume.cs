﻿using PatientApp.DeviceConnection;
using System;

namespace PatientApp.Commands
{
    internal class SessionResume : ISessionCommand
    {
        private EventHandler<Statistic> _onReceiveDataDevMgr;
        private readonly EventHandler<Statistic> _onReceiveData;

        public SessionResume(EventHandler<Statistic> onReceiveDataDevMgr, EventHandler<Statistic> onReceiveData)
        {
            _onReceiveDataDevMgr = onReceiveDataDevMgr;
            _onReceiveData = onReceiveData;
        }

        /// <summary>
        /// Subscribes to data receive event
        /// </summary>
        public void Execute()
        {
            _onReceiveDataDevMgr += _onReceiveData;
            Console.WriteLine("======= Session resumed");
        }
    }
}
