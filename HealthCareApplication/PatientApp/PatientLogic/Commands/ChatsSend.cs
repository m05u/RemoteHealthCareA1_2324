﻿using PatientApp.PatientLogic.Helpers;
using System.Threading.Tasks;
using Utilities.Communication;

namespace PatientApp.PatientLogic.Commands
{
    internal class ChatsSend
    {
        private readonly string _chatMessage;
        private readonly ClientConn _clientConn;

        public ChatsSend(string chatMessage, ClientConn clientConn)
        {
            _chatMessage = chatMessage;
            _clientConn = clientConn;
        }

        public async Task<bool> Execute()
        {
            await _clientConn.SendJson(PatientFormat.ChatsSendMessage(_chatMessage));

            // Expect no response from the server

            return true;
        }
    }
}
