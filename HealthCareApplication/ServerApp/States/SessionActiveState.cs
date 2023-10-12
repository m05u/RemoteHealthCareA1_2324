﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ServerApp.States
{
    internal class SessionActiveState : IState
    {
        private ServerContext _context;
        public SessionActiveState(ServerContext context)
        {
            this._context = context;
        }

        public IState Handle(JsonObject packet)
        {
            string command = JsonUtil.GetValueFromPacket(packet, "command").ToString();
         
            if (command == "stats/send")
            {
                double speed = Double.Parse(JsonUtil.GetValueFromPacket(packet, "data", "speed").ToString());
                int distance = Int32.Parse(JsonUtil.GetValueFromPacket(packet, "data", "distance").ToString());
                byte heartRate = Byte.Parse(JsonUtil.GetValueFromPacket(packet, "data", "heartrate").ToString());


                // Save data in server
                BufferUserData(speed, distance, heartRate);
                return this; // Stay in this state to recieve more data

            }
            else if (packet.ContainsKey("session/stop"))
            {

                _context.ResponseToClient = ResponseDataForClient.GenerateResponse("session/stop",null,"ok");
                return new SessionStoppedState(this._context);

            }
            //Login Failed so it stays in LoginState
            return this;
        }

        private void BufferUserData(double speed, int distance, byte heartrate)
        {

            this._context.userStatsBuffer.Add(new UserStat(speed, distance, heartrate));

            foreach (UserStat stat in this._context.userStatsBuffer)
            {
                Console.WriteLine(stat.ToString());
            }
        }
    }
}
