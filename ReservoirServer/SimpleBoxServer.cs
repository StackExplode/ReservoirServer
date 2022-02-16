using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

using ReservoirServer.Driver;
using ReservoirServer.Enterty;



namespace ReservoirServer
{
    public delegate void OnBoxConnectedDlg(IComClient client);
    public delegate void OnBoxDisconnectedDlg(IComClient client);
    public delegate void OnBoxDataRecDlg(IComClient client, string data);

    class SimpleBoxServer
    {
        BoxTCPDriver tcpserver = new BoxTCPDriver();
        public string PlatformID { get; set; }
        public BoxTCPDriver Server => tcpserver;
        public virtual event OnBoxConnectedDlg OnBoxConnected;
        public virtual event OnBoxDisconnectedDlg OnBoxDisconnected;
        public virtual event OnBoxDataRecDlg OnBoxDataRec;

        public SimpleBoxServer(string serverID, IPAddress ip, ushort port)
        {
            this.PlatformID = serverID;
            tcpserver.SetParameter(ip, port, GlobalConfig.MaxClients);
            tcpserver.Init();
            tcpserver.OnComClientConnected += Tcpserver_OnComClientConnected;
            tcpserver.OnComClientDisconneted += Tcpserver_OnComClientDisconneted;
            tcpserver.OnComDataReceived += Tcpserver_OnComDataReceived;
            tcpserver.OnTransmitError += Tcpserver_OnTransmitError;
        }

        public virtual void SendPack(IComClient client,string data)
        {
            
            tcpserver.SendDataAsync(client, Encoding.UTF8.GetBytes(data));
        }

        protected virtual void Tcpserver_OnTransmitError(IComClient client, Exception ex)
        {
            //Log error
        }

        protected virtual void Tcpserver_OnComDataReceived(IComClient client, byte[] data)
        {
            string s = Encoding.UTF8.GetString(data);
            OnBoxDataRec?.Invoke(client, s);
        }

        protected virtual void Tcpserver_OnComClientDisconneted(IComClient client)
        {
            OnBoxDisconnected?.Invoke(client);
        }

        protected virtual void Tcpserver_OnComClientConnected(IComClient client)
        {
            OnBoxConnected?.Invoke(client);
        }

        public void Start() => tcpserver.Start();
        public void Stop() => tcpserver.Stop();
        public void Dispose() => tcpserver.Dispose();

    }
}
