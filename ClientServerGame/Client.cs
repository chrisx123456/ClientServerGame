using ClientServerGame.Packets;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ClientServerGame
{
    class Client : IDisposable
    {
        private GameBoard _board;
        private TcpClient _tcpClient;
        private IPAddress _ipAddress;
        private int _externalId;
        private int _internalId;
        public Client(IPAddress ip, int port, GameBoard board)
        {
            Connect(ip, port);
            _ipAddress = ip;
            _board = board;
        }
        private void Connect(IPAddress ip, int port)
        {
            try
            {
                _tcpClient = new TcpClient(ip.ToString(), port);
                
                Task.Run(() => HandleServerComs());
            }
            catch (Exception ex)
            {
                Debug.Write("Client: " + ex.Message);
            }
        }
        public void HandleServerComs()
        {
            try
            {
                while (true)
                {
                    if (_tcpClient == null || !_tcpClient.Connected || _tcpClient.GetStream() == null) return;
                    var data = TCPUtils.ReadMessage(_tcpClient.GetStream());
                    if (data != null)
                    {
                        string type = data.GetType().Name;
                        switch (type)
                        {
                            case nameof(PortPacket):
                                _tcpClient.Close();
                                _board.DispatcherQueue.TryEnqueue(() => { _board.ShowMessage("", ((PortPacket)data).port.ToString()); });
                                Connect(_ipAddress, ((PortPacket)data).port);
                                break;
                            case nameof(InitPacket):
                                if(((InitPacket)data).internalID != -1)
                                {
                                    _internalId = ((InitPacket)data).internalID;
                                }
                                _externalId = ((InitPacket)data).externalID;
                                break;
                            case nameof(ClientCloseRequestPacket):
                                _board.DispatcherQueue.TryEnqueue(async () => { 
                                     await _board.ShowMessage("", "Przeciwnik zamknął połączenie");
                                    _board.AppWindow.Closing -= _board.AppWindow_Closing;
                                    _board.Close();
                                });
                                break;
                            case nameof(PrepStartPacket):
                                _board.DispatcherQueue.TryEnqueue(() => {
                                    this._board.DisableEnableGrid(true, _board.BottomGridRef);
                                });
                                break;
                            case nameof(PrepStopPacket):
                                _board.DispatcherQueue.TryEnqueue(() => {
                                    this._board.DisableEnableGrid(false, _board.BottomGridRef);
                                });
                                break;
                            case nameof(GameStartPacket):
                                _board.DispatcherQueue.TryEnqueue(() => {
                                    this._board.DisableEnableGrid(true, _board.TopGridRef);
                                    _board.IsMyTurn = true;
                                });
                                break;
                            case nameof(HitConfirmPacket):
                                HitConfirmPacket hcp = (HitConfirmPacket)data;
                                _board.DispatcherQueue.TryEnqueue(() => {
                                    this._board.SetTextToButton(hcp.isMe, hcp.isHit, hcp.index);
                                    if (hcp.toggle) {
                                        this._board.DisableEnableGrid(true, _board.TopGridRef);
                                        _board.IsMyTurn = true;
                                    } 
                                    else this._board.DisableEnableGrid(false, _board.TopGridRef);
                                });
                                break;
                            case nameof(EndGamePacket):
                                EndGamePacket edp = (EndGamePacket)data;
                                _board.DispatcherQueue.TryEnqueue(async () => {
                                    if (edp.win)
                                    {
                                         await _board.ShowMessage("", "Wygrałeś");
                                    }
                                    else
                                    {
                                         await _board.ShowMessage("", "Przegrałeś");
                                    }
                                    this._board.DisableEnableGrid(false, _board.TopGridRef);
                                    this._board.DisableEnableGrid(false, _board.TopGridRef);
                                    ContentDialogResult res =  await _board.ShowMessageRematch();
                                    switch (res)
                                    {
                                        case ContentDialogResult.Primary:
                                                 await TCPUtils.SendData<RematchAskPacket>(new RematchAskPacket() { externalId = this._externalId, internalId = this._internalId, rematch = true },
                                                _tcpClient.GetStream());
                                            break;
                                        case ContentDialogResult.Secondary:
                                                 await TCPUtils.SendData<RematchAskPacket>(new RematchAskPacket() { externalId = this._externalId, internalId = this._internalId, rematch = false },
                                                 _tcpClient.GetStream());
                                                 _board.AppWindow.Destroy();
                                            break;
                                    }
                                });
                                break;
                            case nameof(RematchResultPacket):
                                RematchResultPacket rrp = (RematchResultPacket)data;
                                if (rrp.rematch)
                                {
                                    _board.DispatcherQueue.TryEnqueue(() =>
                                    {
                                        _board.Rematch();
                                    });
                                }
                                else
                                {
                                    _board.DispatcherQueue.TryEnqueue(async() =>
                                    {
                                        await _board.ShowMessage("", "Przeciwnik odrzucił prośbe o rewanż");
                                        _board.AppWindow.Destroy();
                                    });
                                }
                                break;
                        }

                    }
                }
            }
            catch(Exception ex)
            {
                Debug.Write("Client: " + ex.Message);
            }
        }
        public void SendSetShipPacket(int index)
        {
            TCPUtils.SendData<SetShipPacket>(new SetShipPacket { externalID = this._externalId, internalID = this._internalId, shipIndex = index }, this._tcpClient.GetStream());
        }
        public void SendHitCheckPacket(int index)
        {
            TCPUtils.SendData<HitCheckPacket>(new HitCheckPacket() { externalId = this._externalId, internalId = this._internalId, shipIndex = index }, this._tcpClient.GetStream());
        }

        public async void Dispose()
        {
            await TCPUtils.SendData<ClientClosedPacket>(new ClientClosedPacket { externalID = this._externalId, internalID = this._internalId }, this._tcpClient.GetStream());
            this._board = null;
            this._tcpClient.Close();
            this._tcpClient = null;
            this._ipAddress = null;
        }
    }
}
