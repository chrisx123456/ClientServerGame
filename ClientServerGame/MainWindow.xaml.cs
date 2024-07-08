using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClientServerGame
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(400,280));
        }
        private async void buttonServer_Click(object sender, RoutedEventArgs e)
        {
            IPAddress ip = null;
            try
            {
                ip = IPAddress.Parse(txtBoxServerIp.Text);
            }
            catch(Exception ex)
            {
                ShowMessage("Ip", "Złe ip");
                return;
            }
            int port;
            if (!int.TryParse(txtBoxServerPort.Text, out port)) 
            {
                ShowMessage("Port", "Zły port");
                return;
            }

            ServerDispatcher serverDispatcher = new ServerDispatcher(ip, port);
            serverDispatcher.Activate();
        }
        private void buttonClient_Click(object sender, RoutedEventArgs e)
        {
            IPAddress ip = null;
            try
            {
                ip = IPAddress.Parse(txtBoxClientIp.Text);
            }
            catch (Exception ex)
            {
                ShowMessage("Ip", "Złe ip");
                return;
            }
            int port;
            if (!int.TryParse(txtBoxClientPort.Text, out port))
            {
                ShowMessage("Port", "Zły port");
                return;
            }
            GameBoard board = new GameBoard(ip, port);
            board.Activate();
        }
        private async void ShowMessage(string title, string text)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = text,
                CloseButtonText = "OK"
            };
            dialog.XamlRoot = this.Content.XamlRoot;
            await dialog.ShowAsync();
        }

    }

}
