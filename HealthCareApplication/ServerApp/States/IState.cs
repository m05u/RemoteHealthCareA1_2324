﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ServerApp.States
{
    /// <summary>
    /// Interface to determine statemachine of server.
    /// </summary>
    internal interface IState
    {
        IState Handle(JsonObject packet);
    }
}
