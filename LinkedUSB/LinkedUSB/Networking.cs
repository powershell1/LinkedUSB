using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.IO;
using System.Threading.Tasks;
using System;

namespace Network
{
    public class TCPServer
    {
        public static int PORT = 13000;

        static void Start()
        {
            TcpListener server = new TcpListener(IPAddress.Any, PORT);
            server.Start();
            Debug.WriteLine("Server started...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(client);
            }
        }

        static async void HandleClient(object? obj)
        {
            if (obj is TcpClient client)
            {
                NetworkStream stream = client.GetStream();
                try
                {
                    while (client.Connected)
                    {
                        int x = 500;
                        int y = 500;
                        byte[] sendBuffer = new byte[4];
                        sendBuffer[0] = (byte)(x & 0xFF);
                        sendBuffer[1] = (byte)((x >> 8) & 0xFF);
                        sendBuffer[2] = (byte)(y & 0xFF);
                        sendBuffer[3] = (byte)((y >> 8) & 0xFF);
                        await stream.WriteAsync(sendBuffer, 0, sendBuffer.Length);
                        await stream.FlushAsync();
                        // Debug.WriteLine("Data sent...");
                        // Simulate some delay
                        await Task.Delay(10);
                    }
                }
                catch (IOException ioEx)
                {
                    Console.WriteLine($"IOException: {ioEx.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
                finally
                {
                    stream.Close();
                    client.Close();
                    Console.WriteLine("Client disconnected...");
                }
            }
        }
    }

    public class TCPClient
    {
        public static void Connect(string ip)
        {
            Debug.WriteLine("Client started...");
            TcpClient client = new TcpClient(ip, TCPServer.PORT);
            NetworkStream stream = client.GetStream();

            while (true)
            {
                byte[] buffer = new byte[8];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                int decodeX = (buffer[1] << 8) | buffer[0];
                int decodeY = (buffer[3] << 8) | buffer[2];
                Debug.WriteLine("Received: " + decodeX + ", " + decodeY);
            }
        }
    }



    public class Services : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            // Debug.WriteLine("Received message: " + e.Data);
        }
    }

    public class WSService
    {
        public static int PORT = 14850;

        public static WebSocketServer wssv = new WebSocketServer(PORT);

        public static void StartServer()
        {
            // Iterate through all WebSocket sessions and send a message to each one
            wssv.AddWebSocketService<Services>("/receiver");
            wssv.Start();
            Debug.WriteLine("Server started on port " + PORT);
        }

        public static void StopServer()
        {
            wssv.Stop();
            Debug.WriteLine("Server stopped");
        }
    }

    public class WSReceiver
    {
        public WebSocket ws;

        public WSReceiver(string ip)
        {
            string networkAddress = "ws://" + ip + ":" + WSService.PORT + "/receiver";
            Debug.WriteLine(networkAddress);
            ws = new WebSocket(networkAddress);
            ws.OnMessage += onMessage;
            ws.OnError += Ws_OnError;
        }

        private void Ws_OnError(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine(e.Exception);
        }

        public void Connect()
        {
            ws.Connect();
        }

        private void onMessage(object sender, MessageEventArgs e)
        {
            byte[] receiveBuffer = e.RawData;
            int decodeX = (receiveBuffer[1] << 8) | receiveBuffer[0];
            int decodeY = (receiveBuffer[3] << 8) | receiveBuffer[2];
            CursorMovement.SetPosition(new CursorMovement.POINT { X = decodeX, Y = decodeY });
            Debug.WriteLine("X: " + decodeX + " Y: " + decodeY);

        }

        public void Close()
        {
            ws.Close();
        }
    }
}