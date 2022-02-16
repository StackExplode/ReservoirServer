using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace ReservoirServer.Driver
{
    public class UDPDriver : IComDriver 
    {
        private bool disposedValue;

        private bool _running = false;
        //private bool _avaliable = false;
        private UdpClient _udpserver;
        public bool IsRunning => _running;

        public bool IsAvaliable => _udpserver == null ? false : true;

        public event OnComDataReceivedDlg OnComDataReceived;
        [Obsolete("This event is not implemented!", true)]
        public event OnComClientConnectedDlg OnComClientConnected;
        public event OnComDataSentDlg OnComDataSent;
        [Obsolete("This event is not implemented!", true)]
        public event OnComClientDisconnetedDlg OnComClientDisconneted;
        public event OnTransmitErrorDlg OnTransmitError;

        public IPAddress ListenIP { get; private set; }
        public ushort ListenPort { get; private set; }
        //public int MaxClients { get; private set; }

        public void SetParameter(IPAddress ip,ushort port)
        {
            if (_running)
            {
                throw new Exception("You can't set parameter when server is running!");
            }
                
            ListenIP = ip;
            ListenPort = port;
            //MaxClients = maxclient;
        }


        public void Init()
        {
            if (!_running)
                _udpserver = new UdpClient(new IPEndPoint(ListenIP, ListenPort));
        }

        public void SendData(IComClient client, byte[] data)
        {
            if(_running)
            {
                var task = _udpserver.SendAsync(data, data.Length, (client as BoxUDPClient).Client);
                task.ContinueWith((rst) => { OnComDataSent?.Invoke(client, rst.Result); });
            }
            
        }

        public void Start()
        {
            if(!_running)
            {
                _running = true;
                _udpserver.BeginReceive(new AsyncCallback(ReceiveDataAsync), null);
            }
        }

        private void ReceiveDataAsync(IAsyncResult ar)
        {
            IPEndPoint ip = null;
            byte[] arr;
            BoxUDPClient cl = null;
            try
            {
                arr = _udpserver.EndReceive(ar, ref ip);
                if (arr != null && arr.Length > 0)
                {
                    cl = new BoxUDPClient(ip);
                    OnComDataReceived?.Invoke(cl, arr);
                }
            }
            catch(System.IO.IOException)
            {
                
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch(Exception ex)
            {
                OnTransmitError?.Invoke(cl ?? new BoxUDPClient(ip), ex);
            }
            finally
            {
                if(_running && _udpserver != null)
                    _udpserver.BeginReceive(new AsyncCallback(ReceiveDataAsync), null);
            }
        }

        public void Stop(bool abort = false)
        {
            if(_running)
            {
                _running = false;
                _udpserver.Close();  
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
        ~UDPDriver()
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
