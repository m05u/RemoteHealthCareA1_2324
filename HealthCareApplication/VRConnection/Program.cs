﻿using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;

namespace VRConnection
{

    public class Program
    {

        public static void Main(string[] args)
        {
            // Application to test connection to VR server

            // Initialize Server connection
            TcpClient tcpClient = new TcpClient("145.48.6.10", 6666);
            NetworkStream networkStream = tcpClient.GetStream();

            TunnelHandler.CreateTunnel(networkStream);
            //
            //
            // bool running = true;
            //
            // while (running)
            // {
            //     Console.Write("Write message: ");
            //     string message = Console.ReadLine();
            //     if (message.Equals("quit"))
            //     {
            //         running = false;
            //     }
            //
            //     TunnelHandler.SendMessage(networkStream, message);
            //
            //     string response = TunnelHandler.ReadMessage(networkStream);
            //     Console.WriteLine("Response:" + JsonObject.Parse(response));
            //
            // }
            tcpClient.Close();
            networkStream.Close();



        }

    }


}
