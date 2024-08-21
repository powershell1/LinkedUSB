using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;

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

        private void StartClient(string ip)
        {
            TCPClient.Connect(ip);
        }
    }
}
