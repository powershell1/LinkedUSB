using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Management;
using LinkedUSB.Utils.Cursor;

namespace LinkedUSB.Utils.Network
{
    public class TCPServer
    {
        public static int PORT = 13000;

        TcpListener server;

        public void Start()
        {
            server = new TcpListener(IPAddress.Any, PORT);
            server.Start();
            Debug.WriteLine("Server started...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                Thread clientThread = new Thread(HandleClient);
                clientThread.IsBackground = true;
                clientThread.Start(client);
            }
        }

        public void Stop()
        {
            server.Stop();
        }
        private static bool isLaptop()
        {
            string query = "SELECT * FROM Win32_Battery";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                using (ManagementObjectCollection results = searcher.Get())
                {
                    return results.Count > 0;
                }
            }
        }

        private static async void HandleClient(object? obj)
        {
            if (obj is TcpClient client)
            {
                NetworkStream stream = client.GetStream();
                byte[] bytesFrom = new byte[4];
                await stream.ReadAsync(bytesFrom, 0, 4);
                if (bytesFrom[0] == 0)
                {
                    string machineName = Environment.MachineName;
                    byte[] data = new byte[8192];
                    byte[] messageBytes = Encoding.ASCII.GetBytes(machineName);
                    data[0] = isLaptop() ? (byte)1 : (byte)0;
                    Array.Copy(messageBytes, 0, data, 1, messageBytes.Length);
                    await stream.WriteAsync(data, 0, 8192);
                    await stream.FlushAsync();
                    stream.Close();
                    client.Close();
                }
                else if (bytesFrom[0] == 1)
                {
                    try
                    {
                        while (client.Connected)
                        {
                            /*
                            ushort x = 500;
                            ushort y = 600;
                            byte[] sendBuffer = new byte[4];
                            sendBuffer[0] = (byte)(x & 0xFF);
                            sendBuffer[1] = (byte)((x >> 8) & 0xFF);
                            sendBuffer[2] = (byte)(y & 0xFF);
                            sendBuffer[3] = (byte)((y >> 8) & 0xFF);
                            */
                            await stream.WriteAsync(CursorHooking.broadcastByte, 0, CursorHooking.broadcastByte.Length);
                            await stream.FlushAsync();
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
    }

    public class TCPClient
    {
        public static void Connect(string ip)
        {
            Debug.WriteLine("Client started...");
            TcpClient client = new TcpClient(ip, TCPServer.PORT);
            NetworkStream stream = client.GetStream();
            stream.Write(new byte[] { 0x01, 0x00, 0x00, 0x00 }, 0, 4);
            stream.Flush();
            while (true)
            {
                byte[] buffer = new byte[4];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                ushort decodeX = (ushort)(buffer[0] | (buffer[1] << 8));
                ushort decodeY = (ushort)(buffer[2] | (buffer[3] << 8));
                CursorMovement.SetPosition(new CursorMovement.POINT { X = decodeX, Y = decodeY });
            }
        }
    }
}