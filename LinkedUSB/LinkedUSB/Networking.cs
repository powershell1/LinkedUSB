using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Network
{
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