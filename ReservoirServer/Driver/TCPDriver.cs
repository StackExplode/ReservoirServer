using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace ReservoirServer.Driver
{
    public class TCPDriver : IComDriver
    {
        private bool disposedValue;

        private bool _running = false;
        //private bool _avaliable = false;

        private TcpListener _Listener;

        public bool IsRunning => _running;

        public bool IsAvaliable => _Listener == null ? false : true;

        public event OnComDataReceivedDlg OnComDataReceived;
        public event OnComClientConnectedDlg OnComClientConnected;
        public event OnComDataSentDlg OnComDataSent;
        public event OnComClientDisconnetedDlg OnComClientDisconneted;
        public event OnTransmitErrorDlg OnTransmitError;

        public IPAddress ListenIP { get; private set; }
        public ushort ListenPort { get; private set; }
        public int MaxClients { get; private set; }
        public int TransmitTimeout { get; set; } = 10000;
        public bool AsyncRecBuffer { get; set; } = true;

        internal class RecState
        {
            public BoxTCPClient Client;
            public NetworkStream Stream { get { return Client.Client.GetStream(); } }
            public TcpClient TCPClient { get { return Client.Client; } }
            public byte[] Buffer;
        }

        public virtual void SetParameter(IPAddress ip,ushort port,int maxc = 1024)
        {
            if (_running)
            {
                throw new Exception("You can't set parameter when server is running!");
            }
            ListenIP = ip;
            ListenPort = port;
            MaxClients = maxc;
        }

        public void Init()
        {
            if (!_running)
                _Listener = new TcpListener(ListenIP, ListenPort);
            
        }

        public virtual void SendData(IComClient client, byte[] data)
        {
            BoxTCPClient carclient = client as BoxTCPClient;
            NetworkStream ns = carclient.Client.GetStream();
            ns.WriteTimeout = TransmitTimeout;
            var task = ns.WriteAsync(data, 0, data.Length);
            task.ContinueWith((t) => { this.OnComDataSent?.Invoke(client, data.Length); });
        }

        public void Start()
        {
            if(!_running)
            {
                _running = true;
                _Listener.Start(MaxClients);
                _Listener.BeginAcceptTcpClient(new AsyncCallback(HandleAsyncConnection), _Listener);
            }
        }

        protected virtual void HandleAsyncConnection(IAsyncResult res)
        {
            if (_Listener == null || _Listener.Server == null || _Listener.Server.IsBound == false)
                return;
            TcpClient client = null;
            try
            {
                client = _Listener.EndAcceptTcpClient(res);
                if (_running)
                    _Listener.BeginAcceptTcpClient(HandleAsyncConnection, _Listener);
                
            }
            catch(ObjectDisposedException)
            {
                return;
            }
            catch (Exception ex)
            {
                OnTransmitError?.Invoke(null, ex);
            }
            if (client != null)
            {
                client.ReceiveTimeout = TransmitTimeout;
                byte[] buff = new byte[client.ReceiveBufferSize];
                client.GetStream().ReadTimeout = TransmitTimeout;
                BoxTCPClient carclient = new BoxTCPClient(client);
                RecState state = new RecState { Client = carclient, Buffer = buff };
                this.OnComClientConnected?.Invoke(carclient);
                state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length, HandleClientAsyncRec, state);
            }

        }

        protected virtual void HandleClientAsyncRec(IAsyncResult res)
        {
            if (_Listener == null || _Listener.Server == null || _Listener.Server.IsBound == false)
                return;
            RecState state = (RecState)res.AsyncState;
            TcpClient client = state.Client.Client;
            byte[] oldbuff = state.Buffer;
            NetworkStream ns = state.Stream;
            if (client == null)
                return;
            if(client.Connected == false)
            {
                this.OnComClientDisconneted?.Invoke(state.Client);
                state.Client.Disconnected();
            }
                

            int b2r = 0;
            try
            {
                b2r = ns.EndRead(res);
               
            }
            catch (System.IO.IOException)
            {
                b2r = 0;
            }
            catch(ObjectDisposedException)
            {
                return;
            }
            catch (Exception ex)
            {
                OnTransmitError?.Invoke(state.Client, ex);
            }

            if (client.Available > 0)
            {
                throw new Exception("Data too long!");
            }
            if (b2r == 0)
            {
                this.OnComClientDisconneted?.Invoke(state.Client);
                state.Client.Disconnected();
            }
                
            else
            {
                if(AsyncRecBuffer)
                {
                    byte[] buffer = new byte[b2r];
                    Buffer.BlockCopy(state.Buffer, 0, buffer, 0, b2r);
                    this.OnComDataReceived?.Invoke(state.Client, buffer);
                }
                else
                    this.OnComDataReceived?.Invoke(state.Client, state.Buffer);
                if (_running && (client?.Connected ?? false))
                    state.Stream.BeginRead(state.Buffer, 0, state.Buffer.Length, HandleClientAsyncRec, state);
            }
        }

        public void Stop(bool abort = false)
        {
            if(_running && (_Listener?.Server.IsBound ?? false))
            {
                _running = false;
                _Listener.Server.Close(1);
                _Listener.Stop();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                this.Stop(true);
                disposedValue = true;
            }
        }

        // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        ~TCPDriver()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
