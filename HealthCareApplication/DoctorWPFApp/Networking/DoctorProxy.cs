﻿using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Utilities.Logging;

namespace DoctorWPFApp.Networking
{
    public class DoctorProxy
    {
        private static readonly List<Request> _pendingRequests = new List<Request>();
        private static readonly ClientConn _clientConn = new ClientConn("127.0.0.1", 8888);



        // add events
        #region Events
        public static event EventHandler<EventArgs> LoggedIn;
        public static event EventHandler<string> OnReceiveMessage;
        public static event EventHandler<EventArgs> OnReceiveData;
        #endregion



        public static async Task Listen()
        {
            if (!await _clientConn.ConnectToServer())
                throw new CommunicationException("Could not connect to the server.");

            while (await _clientConn.ReceiveJson() is var message) // listening for message
            {
                Logger.Log($"Received: {message}", LogType.Debug);

                // Get command and data field from message
                (string command, JsonObject dataObject) = GetCommandAndData(message);

                // Handle the command


                //// Thread safe access to _pendingRequests, because another thread may use this variable too.
                //// TODO see if this is still necessary in the final product with UI and stuff.
                //lock (_pendingRequests)
                //{
                //    // Check if this message was a response to a request we sent earlier
                //    Request possibleRequest = GetRequestWithCommand(command);
                //    if (possibleRequest != null)
                //    {
                //        // Handle the message as a response to something sent earlier
                //        possibleRequest.SetResponse(dataObject);
                //        if (!_pendingRequests.Remove(possibleRequest))
                //            throw new CommunicationException("Could not remove request from list.");
                //        continue;
                //    }
                //}
                // TODO here goes code for anything that was not a response, e.g. the chat listener
                Logger.Log($"Was not response, but was {command}", LogType.GeneralInfo);

                switch (command)
                {
                    case "chats/send":
                       
                    // If any session/... has been sent and the code reaches this place,
                    // then assume that the patient send the command.


                    default:
                        Logger.Log($"Cannot process command '{command}'.", LogType.Warning);
                        break;
                }
            }

        }




        #region JSON messages
        /// <summary>
        /// Retrieves the command field (string) and the data field (JsonObject) from
        /// a message.
        /// </summary>
        /// <exception cref="CommunicationException">If the JSON message did not have the required fields</exception>
        public static (string, JsonObject) GetCommandAndData(JsonObject message)
        {
            // Check if the message has a command field
            if (!message.ContainsKey("command"))
                throw new CommunicationException("The message did not contain the JSON key 'command'");

            // Check if the message has a data field
            if (!message.ContainsKey("data"))
                throw new CommunicationException("The message did not contain the JSON key 'data'");

            // Get command and data field
            string command = message["command"].ToString();
            JsonObject dataObject = message["data"].AsObject();

            // Return a TjOePEl
            return (command, dataObject);
        }


        /// <summary>
        /// Sends the request to the server and waits for the response
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The data field of the response sent by the server</returns>
        public static async Task<JsonObject> GetResponse(Request request)
        {
            await _clientConn.SendJson(request.Message);

            lock (_pendingRequests)
                _pendingRequests.Add(request);

            return await request.AwaitResponse();
        }


        /// <summary>
        /// Checks if there is a request with a given command inside the _pendingRequests list.
        /// </summary>
        /// <returns>The request with the specified command value, or else null.</returns>
        private static Request? GetRequestWithCommand(string command)
        {
            foreach (var request in _pendingRequests)
            {
                if (command.Equals(request.Command))
                    return request;
            }
            return null;
        }

        #endregion
    }
}