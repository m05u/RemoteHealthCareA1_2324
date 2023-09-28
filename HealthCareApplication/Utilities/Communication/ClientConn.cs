﻿using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;

namespace Utilities.Communication
{
    public class ClientConn
    {
        private readonly Encoding _encoding = Encoding.ASCII;

        private TcpClient _tcpClient;
        private NetworkStream _stream;

        private readonly string _ipAddress;
        private readonly int _port;

        private bool _isConnected;

        /// <param name="ipAddress">The IP-address of the remote server</param>
        /// <param name="port">The port of the remote server</param>
        public ClientConn(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
        }

        /// <summary>
        ///     Attempts to asynchronously connect to the VR server and establishes a NetworkStream that listens
        ///     to incoming messages.
        /// </summary>
        /// <returns>Wether the connection attempt was successful.</returns>
        public async Task<bool> ConnectToServer()
        {
            if (_isConnected) return false;

            _tcpClient = new TcpClient();

            await _tcpClient.ConnectAsync(_ipAddress, _port);

            if (_tcpClient.Connected) _stream = _tcpClient.GetStream();
            _isConnected = _tcpClient.Connected;

            return _isConnected;
        }

        /// <summary>
        ///     Closes the TcpClient and NetworkStream.
        /// </summary>
        public void CloseConnection()
        {
            if (!_isConnected) return;
            _isConnected = false;
            _tcpClient?.Close();
            _stream?.Close();
        }

        /// <summary>
        ///     Sends a JSON message to the VR server asynchronously.
        /// </summary>
        /// <param name="payload">An object whose structure will be converted to JSON and sent to the VR server.</param>
        /// <exception cref="CommunicationException">When something goes wrong while sending data to the VR server.</exception>
        public async Task SendJson(object payload)
        {
            if (!_isConnected)
                throw new CommunicationException(
                    "There is no active communication between the client application and the VR server.");


            // Turn payload object into a JSON string
            string jsonString = JsonSerializer.Serialize(payload);


            // Encode JSON string as a byte array
            byte[] payloadAsBytes = _encoding.GetBytes(jsonString);

            // Encode length of the payload as bytes
            byte[] lengthData = BitConverter.GetBytes((uint)payloadAsBytes.Length);

            // Concatenate payload to length data for the final message
            byte[] message = lengthData.Concat(payloadAsBytes).ToArray();

            await _stream.WriteAsync(message, 0, message.Length);
        }

        /// <summary>
        ///     Receives a JSON message from the VR server asynchronously
        /// </summary>
        /// <returns>The message which the server sent, as a JsonObject.</returns>
        /// <exception cref="CommunicationException">When something goes wrong while receiving data from the VR server.</exception>
        public async Task<JsonObject> ReceiveJson()
        {
            if (!_isConnected)
                throw new CommunicationException(
                    "There is no active communication between the client application and the VR server.");

            byte[] lengthArray = new byte[4];

            // Read the first four bytes and put it in lengthArray
            await _stream.ReadAsync(lengthArray, 0, lengthArray.Length);

            // Convert length to uint
            uint length = BitConverter.ToUInt32(lengthArray, 0);

            byte[] payloadBuffer = new byte[length];
            int totalBytesRead = 0;

            // Read until amount of bytes read exceeds the length of the length variable
            while (totalBytesRead < length)
            {
                // Read the bytes that have just been received
                int bytesRead = await _stream.ReadAsync(payloadBuffer, totalBytesRead, payloadBuffer.Length - totalBytesRead);
                totalBytesRead += bytesRead;
            }

            // Deserialize message
            var messageAsString = _encoding.GetString(payloadBuffer, 0, totalBytesRead);
            JsonObject deserializedMessage = JsonSerializer.Deserialize<JsonObject>(messageAsString)?.AsObject();

            if (deserializedMessage == null)
                throw new CommunicationException("Something went wrong while receiving a message from the VR server.");

            return deserializedMessage;
        }
    }
}
