using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ReservoirServer.Driver;

namespace ReservoirServer
{
    public delegate void OnAMQMessageReceivedDlg(string message);
    class AMQCommunicator
    {
        protected SimpleQueueSender _sender = null;
        protected SimpleTopicSubscriber _subscriber = null;
        protected string platform_id = "";
        private string uri;
        private string queue_name;
        private string topic_name;
        private object qsend_locker = new object();
        public string PlatformID => platform_id;

        public event OnAMQMessageReceivedDlg OnSubscribeReceived;

        public AMQCommunicator(string uri,string queue_name,string topic_name,string platformID)
        {
            this.uri = uri;
            this.queue_name = queue_name;
            this.topic_name = topic_name;
            platform_id = platformID;
        }

        //Init will start communication
        public void Initialization()
        {
            Dispose();
            _sender = new SimpleQueueSender(queue_name, uri);
            _subscriber = new SimpleTopicSubscriber(topic_name, uri, "client_"+ platform_id, "consumer_"+ platform_id, null);
            _subscriber.OnMessageReceived += _subscriber_OnMessageReceived;
        }

        public void SendQueue(string data)
        {
            //lock(qsend_locker)
            //{
                _sender.SendMessage(data, null);
            //}
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public void MultiSendTest()
        {
            Parallel.For(0, 25, (i) =>
            {
                string str = "";
                for (int j = 0; j < 30; j++)
                    str += (char)('a' + i % 26);
                _sender.SendMessage(str, null);
            });
        }

        
        public void Dispose()
        {
            if(_sender != null)
                _sender.Dispose();
            if(_subscriber != null)
                _subscriber.Dispose();
            _sender = null;
            _subscriber = null;
        }

        private void _subscriber_OnMessageReceived(string message)
        {
            OnSubscribeReceived?.Invoke(message);
        }
    }
}
