using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Commands;
using System;
using System.Collections.Generic;
using System.Text;


namespace ReservoirServer.Driver
{
    public delegate void MessageReceivedDelegate(string message);
    public class SimpleTopicPublisher : IDisposable
    {
        private readonly string topicName = null;
        private readonly IConnectionFactory connectionFactory;
        private readonly IConnection connection;
        private readonly ISession session;
        private readonly IMessageProducer producer;
        private bool isDisposed = false;

        public SimpleTopicPublisher(string topicName, string brokerUri)
        {
            this.topicName = topicName;
            this.connectionFactory = new ConnectionFactory(brokerUri);
            this.connection = this.connectionFactory.CreateConnection();
            this.connection.Start();
            this.session = connection.CreateSession();
            ActiveMQTopic topic = new ActiveMQTopic(topicName);
            this.producer = this.session.CreateProducer(topic);

        }

        public void SendMessage(string message,string filter)
        {
            if (!this.isDisposed)
            {
                ITextMessage textMessage = this.session.CreateTextMessage(message);
                textMessage.Properties.SetString("filter", filter);
                this.producer.Send(textMessage);
            }
            else
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.producer.Dispose();
                this.session.Dispose();
                this.connection.Dispose();
                this.isDisposed = true;
            }
        }

        #endregion
    }


    public class SimpleTopicSubscriber : IDisposable
    {
        private readonly string topicName = null;
        private readonly IConnectionFactory connectionFactory;
        private readonly IConnection connection;
        private readonly ISession session;
        private readonly IMessageConsumer consumer;
        private bool isDisposed = false;
        public event MessageReceivedDelegate OnMessageReceived;

        public SimpleTopicSubscriber(string topicName, string brokerUri, string clientId, string consumerId,string filter)
        {
            string ft = filter == null ? null : $"filter='{filter}'";
            this.topicName = topicName;
            this.connectionFactory = new ConnectionFactory(brokerUri);
            this.connection = this.connectionFactory.CreateConnection();
            this.connection.ClientId = clientId;
            this.connection.Start();
            this.session = connection.CreateSession();
            ActiveMQTopic topic = new ActiveMQTopic(topicName);
            this.consumer = this.session.CreateDurableConsumer(topic,consumerId, ft, false);
            this.consumer.Listener += new MessageListener(OnMessage);

        }

        public void OnMessage(IMessage message)
        {
            ITextMessage textMessage = message as ITextMessage;
            if (this.OnMessageReceived != null)
            {
                this.OnMessageReceived(textMessage.Text);
            }
        }


        #region IDisposable Members

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.consumer.Dispose();
                this.session.Dispose();
                this.connection.Dispose();
                this.isDisposed = true;
            }
        }

        #endregion
    }

    public class SimpleQueueSender : IDisposable
    {
        private bool isDisposed = false;
        private readonly IConnection connection;
        private readonly IConnectionFactory factory;
        private readonly ISession session;
        private readonly IMessageProducer producer;
        private string queue_name;

        public SimpleQueueSender(string queueName, string brokerUri, AcknowledgementMode ackmode = AcknowledgementMode.AutoAcknowledge)
        {
            this.queue_name = queueName;
            factory = new ConnectionFactory(brokerUri);
            connection = factory.CreateConnection();
            this.connection.Start();
            session = connection.CreateSession(ackmode);
            ActiveMQQueue queue = new ActiveMQQueue(queueName);
            producer = session.CreateProducer(queue);
        }

        public void SendMessage(string message, string filter)
        {
            if (!this.isDisposed)
            {
                ITextMessage textMessage = producer.CreateTextMessage(message);
                textMessage.Properties.SetString("filter", filter);
                this.producer.Send(textMessage);
            }
            else
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.producer.Dispose();
                this.session.Dispose();
                this.connection.Dispose();
                this.isDisposed = true;
            }
        }
    }

    public class SimpleQueueReciever : IDisposable
    {
        private bool isDisposed = false;
        private readonly IConnectionFactory connectionFactory;
        private readonly IConnection connection;
        private readonly ISession session;
        private readonly IMessageConsumer consumer;
        public event MessageReceivedDelegate OnMessageReceived;

        public SimpleQueueReciever(string queueName, string brokerUri, string clientId, string filter, AcknowledgementMode ackmode = AcknowledgementMode.AutoAcknowledge)
        {
            string ft = filter == null ? null : $"filter='{filter}'";
            connectionFactory = new ConnectionFactory(brokerUri);
            connection = connectionFactory.CreateConnection();
            connection.ClientId = clientId;
            connection.Start();
            session = connection.CreateSession(ackmode);
            ActiveMQQueue queue = new ActiveMQQueue(queueName);
            consumer = session.CreateConsumer(queue, ft);
            this.consumer.Listener += new MessageListener(OnMessage);
        }

        public void OnMessage(IMessage message)
        {
            ITextMessage textMessage = message as ITextMessage;
            if (this.OnMessageReceived != null)
            {
                this.OnMessageReceived(textMessage.Text);
            }
        }

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.consumer.Dispose();
                this.session.Dispose();
                this.connection.Dispose();
                this.isDisposed = true;
            }
        }
    }

    
}
