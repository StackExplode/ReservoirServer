using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ReservoirServer.Driver
{
    public delegate void OnComDataReceivedDlg(IComClient client, byte[] data);
    public delegate void OnComClientConnectedDlg(IComClient client);
    public delegate void OnComDataSentDlg(IComClient client,int len);
    public delegate void OnComClientDisconnetedDlg(IComClient client);
    public delegate void OnTransmitErrorDlg(IComClient client, Exception ex);
    public interface IComDriver: IDisposable
    {
        bool IsRunning { get;  }
        bool IsAvaliable { get; }
        event OnComDataReceivedDlg OnComDataReceived;
        event OnComClientConnectedDlg OnComClientConnected;
        event OnComDataSentDlg OnComDataSent;
        event OnComClientDisconnetedDlg OnComClientDisconneted;
        event OnTransmitErrorDlg OnTransmitError;
        void Init();
        void Start();
        void Stop(bool abort = false);
        void SendData(IComClient client, byte[] data);

    }
}
