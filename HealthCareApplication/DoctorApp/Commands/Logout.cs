﻿using DoctorApp.Helpers;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Utilities.Communication;

namespace DoctorApp.Commands
{
    internal class Logout : IDoctorCommand
    {
        public Task<bool> Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}
