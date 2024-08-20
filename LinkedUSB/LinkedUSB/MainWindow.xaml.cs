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
using System.Runtime.InteropServices.WindowsRuntime;
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
        public static WSReceiver wsReceiver;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Closed += onAppClosing;
        }

        private void onAppClosing(object sender, WindowEventArgs args)
        {
            WSService.StopServer();
            CursorHooking.RemoveMouseHook();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            CursorMovement.POINT point = new CursorMovement.POINT { X = 10, Y = 10 };
            CursorMovement.MoveCursor(point);
            if (wsReceiver == null)
            {
                wsReceiver = new WSReceiver(MyTextBox.Text);
                wsReceiver.Connect();
            }
            myButton.Content = "Clicked";
        }
    }
}
