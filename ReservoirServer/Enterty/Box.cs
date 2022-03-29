using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ReservoirServer.Driver;

namespace ReservoirServer.Enterty
{
    public enum BoxState
    {
        Unknown = 0,
        Online = 1,
        Offline = 2,
        Timeout = 3
    }
    class Box
    {
        public string Version { get; set; }
        public string ID { get;  set; }
        public byte SignalStrength { get;  set; }
        public byte Battery { get;  set; }
        public BoxState State { get;  set; } = BoxState.Unknown;
        public DateTime LastHBTime { get;  set; } 
        public IComClient ComClient { get { return comClient; } set { comClient = value; } }

        private IComClient comClient;
        public ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        
        public Box(string id,IComClient client)
        {
            ID = id;
            comClient = client;
            LastHBTime = DateTime.Now;
            if (comClient != null)
            {
                comClient.OnDisconnected += () =>
                {
                    //locker.EnterWriteLock();
                    State = BoxState.Offline;
                    //locker.ExitWriteLock();
                };
            }
        }
     
    }
}
