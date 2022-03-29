using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ReservoirServer.Driver
{
    public class BoxTCPDriver : TCPDriver
    {

        public Task SendDataAsync(IComClient client, byte[] data)
        {
            BoxTCPClient carclient = client as BoxTCPClient;
            NetworkStream ns = carclient.Client.GetStream();
            ns.WriteTimeout = TransmitTimeout;
            Task task = null;
            try
            {
                task = ns.WriteAsync(data, 0, data.Length);
            }
            catch(System.IO.IOException)
            {
                client.Disconnected();
            }
            catch(SocketException)
            {
                client.Disconnected();
            }
            
            return task;
            //task.ContinueWith((t) => { this.OnComDataSent?.Invoke(client, data.Length); });

          
        }
    }
}
