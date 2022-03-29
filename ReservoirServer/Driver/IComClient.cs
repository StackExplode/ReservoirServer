using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ReservoirServer.Driver
{
    public interface IComClient
    {
        bool IsAlive { get; set; }

        void Disconnect();
        void Disconnected();
        public event Action OnDisconnected;
    }

    public class BoxUDPClient : IComClient
    {
        public BoxUDPClient(IPEndPoint cl)
        {
            Client = cl;
        }

        public bool IsAlive { get; set; } = false;

        public IPEndPoint Client { get; }

        public event Action OnDisconnected;

        public virtual void Disconnect()
        {
            //throw new NotImplementedException();
        }

        public void Disconnected()
        {
            IsAlive = false;
            OnDisconnected?.Invoke();
        }
    }

    public class BoxTCPClient : IComClient
    {
        public BoxTCPClient(TcpClient cl)
        {
            Client = cl;
        }
        public TcpClient Client { get; }
        public bool IsAlive { get; set; } = false;
        public event Action OnDisconnected;

        public void Disconnect()
        {
            Client.Close();
            IsAlive = false;
            OnDisconnected?.Invoke();
        }

        public void Disconnected()
        {
            IsAlive = false;
            OnDisconnected?.Invoke();
        }
        //public bool IsAlive => Client.Connected;
    }
}
