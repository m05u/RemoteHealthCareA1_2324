﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRConnection.Communication
{
    [Serializable]
    internal class CommunicationException : Exception
    {
        public CommunicationException(string message) : base(message) { }
    }
}
