using LinkedUSB.Utils.ArpScans;
using LinkedUSB.Utils.Cursor;
using LinkedUSB.Utils.Network;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LinkedUSB
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        TCPServer server;

        public MainWindow()
        {
            Thread serverThread = new Thread(StartServerFree);
            serverThread.IsBackground = true;
            serverThread.Start();
            this.InitializeComponent();
            this.SystemBackdrop = new MicaBackdrop();
            this.ExtendsContentIntoTitleBar = true;
            this.Title = "LinkedUSB";
            this.Closed += onAppClosing;
        }

        private void StartServerFree()
        {
            server = new TCPServer();
            server.Start();
        }

        private void onAppClosing(object sender, WindowEventArgs args)
        {
            server.Stop();
            CursorHooking.RemoveMouseHook();
        }

        private async void myButton_Click(object sender, RoutedEventArgs e)
        {
            var customDialog = new ShowDevices();
            var dialog = new ContentDialog
            {
                Content = customDialog,
                PrimaryButtonText = "Add Computer",
                CloseButtonText = "Cancel",
                XamlRoot = myButton.XamlRoot,
            };
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // string inputText = customDialog.InputText;
                // Handle the input text
            }
            /*
            CursorMovement.POINT point = new CursorMovement.POINT { X = 10, Y = 10 };
            CursorMovement.MoveCursor(point);
            new Thread(() => StartClient(MyTextBox.Text)).Start();
            myButton.Content = "Clicked";
            */
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            MyListBox.Items.Clear();
            ArpUtil arpHelper = new ArpUtil();
            List<ArpItem> arpEntities = arpHelper.GetArpResult();
            List<String> scannedMac = new List<String>();
            foreach (var item in arpEntities)
            {
                if (item.Type != "dynamic") continue;
                if (scannedMac.Contains(item.MacAddress)) continue;
                scannedMac.Add(item.MacAddress);
                new Thread(() => {
                    Debug.WriteLine(item.MacAddress);
                    try
                    {
                        TcpClient client = new TcpClient();
                        var result = client.BeginConnect(item.Ip, 13000, null, null);
                        var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
                        if (!success)
                        {
                            throw new Exception("Failed to connect.");
                        }
                        NetworkStream stream = client.GetStream();
                        stream.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 }, 0, 4);
                        stream.Flush();
                        byte[] bytes = new byte[8192];
                        int bytesRead = stream.Read(bytes, 0, bytes.Length);
                        bool isLaptop = bytes[0] == 0x01;
                        string deviceName = System.Text.Encoding.ASCII.GetString(bytes, 1, bytesRead - 1);
                        bool isQueued = this.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, async () =>
                        {
                            MyListBox.Items.Add(new ListBoxItem { Content = deviceName + " - " + item.Ip });
                        });
                        /*
                        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                        {
                            MyListBox.Items.Add(new ListBoxItem { Content = item.Ip + " - " + deviceName });
                        });
                        */
                    }
                    catch (Exception ex)
                    {
                    }
                }).Start();
                // Console.WriteLine(item.Ip + "\t" + item.MacAddress + "\t" + item.Type);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void StartClient(string ip)
        {
            TCPClient.Connect(ip);
        }
    }
}
