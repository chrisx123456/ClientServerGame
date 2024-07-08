using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Drawing;
using Microsoft.UI;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using ClientServerGame.Packets;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics.Metrics;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClientServerGame
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GameBoard : Window
    {
        private IPAddress _ipAddress;
        private int _port;
        private Client _client;
        public Grid TopGridRef;
        public Grid BottomGridRef;
        public bool IsMyTurn = false; 
        public GameBoard(IPAddress ip, int port)
        {
            this.InitializeComponent();
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(600,600));
            this.AppWindow.Closing += AppWindow_Closing;
            CreateButtonGrids();
            DisableEnableGrid(false, TopGrid);
            DisableEnableGrid(false, BottomGrid);
            this.TopGridRef = this.TopGrid;
            this.BottomGridRef = this.BottomGrid;
            this._client = new Client(ip, port, this);

            this._ipAddress = ip;
            this._port = port;

        }


        private void CreateButtonGrids()
        {
            CreateButtonGrid(TopGrid, TopGridButtonClick);
            CreateButtonGrid(BottomGrid, BottomGridButtonClick);
        }
        private void CreateButtonGrid(Grid parentGrid, RoutedEventHandler handler)
        {
            // Definiuj 8 kolumn i 8 wierszy
            for (int i = 0; i < 8; i++)
            {
                parentGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                parentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Dodaj 64 przyciski
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Button button = new Button
                    {
                        //Content = (row * 8 + col).ToString(),
                        Tag = Convert.ToInt32(row * 8 + col),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(1, 1, 1, 1),
                        FontFamily = (FontFamily)Application.Current.Resources["Poppins"],
                        FontSize = 16d,                      
                    };
                    button.Click += handler;
                    Grid.SetRow(button, row);
                    Grid.SetColumn(button, col);
                    parentGrid.Children.Add(button);
                }
            }
        }



        private void BottomGridButtonClick(object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)sender);
            btn.Resources["ButtonBackgroundDisabled"] = new SolidColorBrush(Colors.Green);
            btn.IsEnabled = false;
            btn.Content = "Set";
            _client.SendSetShipPacket((int)btn.Tag);
            
        }
        private void TopGridButtonClick(object sender, RoutedEventArgs e)
        {
            if (IsMyTurn)
            {
                int index = (int)((Button)sender).Tag;
                _client.SendHitCheckPacket(index);
                ((Button)sender).Click -= TopGridButtonClick;
                IsMyTurn = false;
            }
        }
        public void SetTextToButton(bool isMe, bool isHit, int index)
        {

            Grid parentGrid = isMe ? BottomGrid : TopGrid;
            Button button = (Button)parentGrid.Children[index];

            string text = isHit ? "Hit" : "Miss";
            Windows.UI.Color color = isHit ? Colors.Red : Colors.Blue;

            button.Content = text;
            var brush = new SolidColorBrush(color);

            button.Resources["ButtonBackgroundDisabled"] = brush;
            button.Background = brush;

        }

        public void DisableEnableGrid(bool lever, Grid parentGrid)
        {
            foreach (Button button in parentGrid.Children) { button.IsEnabled = lever; }
        }
        public async Task<ContentDialogResult> ShowMessage(string title, string text)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = title,
                Content = text,
                CloseButtonText = "OK",                
            };
            dialog.XamlRoot = this.Content.XamlRoot;
            return await dialog.ShowAsync();
        }
        public async Task<ContentDialogResult> ShowMessageRematch()
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Ponowna gra",
                Content = "Czy chcesz zagrać ponownie?",
                PrimaryButtonText = "Tak",
                SecondaryButtonText = "Nie"
            };
            dialog.XamlRoot = this.Content.XamlRoot;
            return await dialog.ShowAsync();
        }
        public void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
        {
            _client.Dispose();
            Debug.WriteLine("Disposing client");
        }
        public void Rematch()
        {
            GameBoard gb = new GameBoard(this._ipAddress, this._port);
            this.Close();
            gb.Activate();
        }
    }
}
