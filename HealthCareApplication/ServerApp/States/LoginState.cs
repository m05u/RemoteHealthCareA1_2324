﻿using Microsoft.SqlServer.Server;
using ServerApp.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ServerApp.States
{
    /// <summary>
    /// State in when a client has connected with the server. 
    /// </summary>
    internal class LoginState : IState
    {
        //TODO current username and password are for 

        private ServerContext context;
        public LoginState(ServerContext context)
        {
            this.context = context;
        }

        public IState Handle(JsonObject packet)
        {
            //extracting the needed values from packet
            string username = JsonUtil.GetValueFromPacket(packet, "data", "username").ToString();
            string password = JsonUtil.GetValueFromPacket(packet, "data", "password").ToString();

            Console.WriteLine("Login recieved data: " + username + "    " + password);


            if(Server.users.Any())
            {
    
                foreach (UserAccount account in Server.users)
                {
                    if (account.GetUserName() == username && account.GetPassword() == password)
                    {
                        Console.WriteLine("We are actually logging in!");
                        context.ResponseToClient = ResponseClientData.GenerateResponse("login", null, "ok");
                        return new SessionIdle(context);
                    }
                    else
                    {
                        Console.WriteLine("Currently going into account creation state.");
                        context.ResponseToClient = ResponseClientData.GenerateResponse("login", null, "ok");
                        return new CreateAccountState(context);
                    }
                }
            }
            else
            {
                context.ResponseToClient = ResponseClientData.GenerateResponse("login", null, "ok");
                return new CreateAccountState(context);
            }
         
            //Login Failed so it stays in LoginState
            return this;
        }

    }
}
