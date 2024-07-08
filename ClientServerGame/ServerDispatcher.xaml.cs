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
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using ClientServerGame.Packets;
using GameObject = (System.Net.Sockets.TcpClient[] clients, ClientServerGame.GameData gamedata);

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClientServerGame
{

    public sealed partial class ServerDispatcher : Window
    {

        private IPAddress _ipAddress;
        //private List<TcpClient[]> _connectedClients = new List<TcpClient[]>();
        private List<GameObject> _connected = new List<GameObject>();
        int shipsNumber = 3;
        public ServerDispatcher(IPAddress ip, int port)
        {
            this.InitializeComponent();
            _ipAddress = ip;
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(500,500));
            Log("Starting");
            Task.Run(() => RunListener(ip, port));
        }
        private async Task RunListener(IPAddress ip, int port)
        {
            TcpListener listener = new TcpListener(ip, port);
            listener.Start();
            try
            {
                Log($"Serwer nasłuchuje na {ip}:{port}");
                while (true)
                {

                    TcpClient[] clientPair = new TcpClient[2];

                    for (int i = 0; i < 2; i++)
                    {
                        clientPair[i] = await listener.AcceptTcpClientAsync();
                        Log($"Klient {i + 1} połączony");
                    }

                    await TCPUtils.SendData<InitPacket>(new InitPacket { internalID = 0, externalID = _connected.Count() }, clientPair[0].GetStream());
                    await TCPUtils.SendData<InitPacket>(new InitPacket { internalID = 1, externalID = _connected.Count() }, clientPair[1].GetStream());
                    await TCPUtils.SendData<PrepStartPacket>(new PrepStartPacket(), clientPair[0].GetStream());
                    await TCPUtils.SendData<PrepStartPacket>(new PrepStartPacket(), clientPair[1].GetStream());

                    Task.Run(() => HandleClientComs(clientPair[0]));
                    Task.Run(() => HandleClientComs(clientPair[1]));

                    _connected.Add(new GameObject(clientPair, new GameData(this.shipsNumber)));

                    Log($"Para klientów dodana do listy, externalId: {_connected.Count()-1}");
                }
            }
            catch (Exception ex)
            {
                Log($"Wystąpił błąd: {ex.Message}");
            }
        }
        public void Log(string text)
        {
            this.DispatcherQueue.TryEnqueue(() => {
                ConsoleTextBox.Text += text + "\n";
            });
        }
        private async void HandleClientComs(TcpClient tcpClient)
        {
            try
            {
                while (true)
                {
                    if (tcpClient == null || !tcpClient.Connected || tcpClient.GetStream() == null) return;
                    var data = TCPUtils.ReadMessage(tcpClient.GetStream());
                    if (data != null)
                    {
                        string type = data.GetType().Name;
                        switch (type)
                        {
                            case nameof(ClientClosedPacket):
                            
                                int toSendId = ((ClientClosedPacket)data).internalID == 1 ? 0 : 1;
                                Log($"ClinetClosedPacket, InetrnalID: {toSendId}");
                                await TCPUtils.SendData<ClientCloseRequestPacket>(new ClientCloseRequestPacket(), 
                                    this._connected[((ClientClosedPacket)data).externalID].clients[toSendId].GetStream());
                                for (int i = ((ClientClosedPacket)data).externalID+1; i < _connected.Count(); i++)
                                {
                                    TcpClient[] clientPair = _connected[i].clients;
                                    await TCPUtils.SendData<InitPacket>(new InitPacket { internalID = -1, externalID = i-1 }, clientPair[0].GetStream());
                                    await TCPUtils.SendData<InitPacket>(new InitPacket { internalID = -1, externalID = i-1 }, clientPair[1].GetStream());
                                }
                                _connected[((ClientClosedPacket)data).externalID].clients[0].Close();
                                _connected[((ClientClosedPacket)data).externalID].clients[1].Close();
                                _connected.RemoveAt(((ClientClosedPacket)data).externalID);
                                return;
                            case nameof(SetShipPacket):
                                SetShipPacket ssp = (SetShipPacket)data;
                                if(_connected[ssp.externalID].gamedata.SetShip(ssp.internalID, ssp.shipIndex) == false)
                                {
                                    await TCPUtils.SendData<PrepStopPacket>(new PrepStopPacket(), _connected[ssp.externalID].clients[ssp.internalID].GetStream());
                                }
                                if (_connected[ssp.externalID].gamedata.isBothSet)
                                {
                                    Random r = new Random();
                                    int rr = r.Next(0, 101);
                                    await TCPUtils.SendData<GameStartPacket>(new GameStartPacket(), _connected[ssp.externalID].clients[rr % 2].GetStream());
                                }
                                break;
                            case nameof(HitCheckPacket):
                                HitCheckPacket hcp = (HitCheckPacket)data;
                                int enemyInternalId = hcp.internalId == 1 ? 0 : 1;
                                bool isHitCheck = _connected[hcp.externalId].gamedata.CheckForHit(enemyInternalId, hcp.shipIndex);

                                await TCPUtils.SendData<HitConfirmPacket>(new HitConfirmPacket() { index = hcp.shipIndex, isHit = isHitCheck, isMe = false, toggle = false },
                                    _connected[hcp.externalId].clients[hcp.internalId].GetStream());

                                await TCPUtils.SendData<HitConfirmPacket>(new HitConfirmPacket() { index = hcp.shipIndex, isHit = isHitCheck, isMe = true, toggle = true },
                                    _connected[hcp.externalId].clients[enemyInternalId].GetStream());

                                if (_connected[hcp.externalId].gamedata.CheckForWin(enemyInternalId))
                                {
                                    await TCPUtils.SendData<EndGamePacket>(new EndGamePacket(true),
                                        _connected[hcp.externalId].clients[hcp.internalId].GetStream());
                                    await TCPUtils.SendData<EndGamePacket>(new EndGamePacket(false),
                                        _connected[hcp.externalId].clients[enemyInternalId].GetStream());
                                }
                                break;
                            case nameof(RematchAskPacket):
                                RematchAskPacket rap = (RematchAskPacket)data;
                                if (rap.internalId == 0) _connected[rap.externalId].gamedata.Rematch0 = rap.rematch;
                                    else _connected[rap.externalId].gamedata.Rematch1 = rap.rematch;
                                if(_connected[rap.externalId].gamedata.Rematch0 != null && _connected[rap.externalId].gamedata.Rematch1 != null)
                                {
                                    if(_connected[rap.externalId].gamedata.Rematch0 == true && _connected[rap.externalId].gamedata.Rematch1 == true)
                                    {
                                        await TCPUtils.SendData<RematchResultPacket>(new RematchResultPacket(true), _connected[rap.externalId].clients[0].GetStream());
                                        await TCPUtils.SendData<RematchResultPacket>(new RematchResultPacket(true), _connected[rap.externalId].clients[1].GetStream());
                                    }
                                    if (_connected[rap.externalId].gamedata.Rematch0 == false && _connected[rap.externalId].gamedata.Rematch1 == true)
                                    {
                                        await TCPUtils.SendData<RematchResultPacket>(new RematchResultPacket(false), _connected[rap.externalId].clients[1].GetStream());
                                    }
                                    if (_connected[rap.externalId].gamedata.Rematch0 == true && _connected[rap.externalId].gamedata.Rematch1 == false)
                                    {
                                        await TCPUtils.SendData<RematchResultPacket>(new RematchResultPacket(false), _connected[rap.externalId].clients[0].GetStream());
                                    }
                                    for (int i = rap.externalId + 1; i < _connected.Count(); i++)
                                    {
                                        TcpClient[] clientPair = _connected[i].clients;
                                        await TCPUtils.SendData<InitPacket>(new InitPacket { internalID = -1, externalID = i - 1 }, clientPair[0].GetStream());
                                        await TCPUtils.SendData<InitPacket>(new InitPacket { internalID = -1, externalID = i - 1 }, clientPair[1].GetStream());
                                    }
                                    _connected[rap.externalId].clients[0].Close();
                                    _connected[rap.externalId].clients[1].Close();
                                    _connected.RemoveAt(rap.externalId);
                                    Log("Rematch: deleting old clients");

                                }

                                break;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Log("HandleClientComs: " + ex.Message);
            }
        }
        private int FindFreeTcpPort(IPAddress ip)
        {
            TcpListener l = new TcpListener(ip, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

    }
}
