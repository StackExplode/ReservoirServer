using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReservoirServer.Enterty;
using ReservoirServer.Driver;

namespace ReservoirServer
{
    class SimpleBoxAdapter
    {
        private BoxList _list;
        private AMQCommunicator _remote;
        private BoxStateReporter _reporter;
        private SimpleBoxServer _server;
        private JsonSerializerSettings jsonSettings;
        public SimpleBoxAdapter(AMQCommunicator com,SimpleBoxServer server,BoxList lst)
        {
            _remote = com;
            _server = server;
            _list = lst;

            _remote.OnSubscribeReceived += _remote_OnSubscribeReceived;

            _server.OnBoxDataRec += _server_OnBoxDataRec;
            _server.OnBoxDisconnected += _server_OnBoxDisconnected;

            jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "yyyy-MM-dd hh:mm:ss";

            _reporter = new BoxStateReporter(_list, _remote);
            
        }

        public void CleanUpJob()
        {
            _reporter.StopTimer();
            _reporter.Dispose();
            _server.Dispose();
            _remote.Dispose();


        }

        public void HeartBeatBoxDataHandler(IComClient client, string data)
        {
            BoxDataHeartBeat boxdata = JsonConvert.DeserializeObject<BoxDataHeartBeat>(data);
            string bid = boxdata.DeviceN;
            Box box = _list[bid];
            if (box == null)
            {
                box = new Box(boxdata.DeviceN, client);
                if (!_list.AddNew(box))
                    return;
                Console.WriteLine($"Box:\"{bid}\" sent heartbeat first time!");
            }
            box.locker.EnterWriteLock();
            box.ComClient = client;
            box.State = BoxState.Online;
            box.Battery = boxdata.Battery;
            box.SignalStrength = boxdata.Signal;
            box.LastHBTime = DateTime.Now;
            box.locker.ExitWriteLock();
            
        }

        public void SimpleBoxDataReportHandler(string data)
        {
            SimpleBoxData boxdata = JsonConvert.DeserializeObject<SimpleBoxData>(data);
            string bid = boxdata.DeviceN;
            Box box = _list[bid];
            if (box == null)
            {
                return;
            }
            SimpleSubscribeData subdata = new SimpleSubscribeData()
            {
                PlatformN = _server.PlatformID,
                DeviceN = new string[] { boxdata.DeviceN },
                Type = boxdata.Type,
                DateTime = DateTime.Now,
                Message = boxdata.Message
            };

            string json = JsonConvert.SerializeObject(subdata, jsonSettings);
            _remote.SendQueue(json);
        }

        public void SimpleBoxDataSubscribeHandler(string json)
        {
            SimpleSubscribeData subdata = JsonConvert.DeserializeObject<SimpleSubscribeData>(json);
            SimpleBoxData boxdata = new SimpleBoxData()
            {
                Type = subdata.Type,
                Message = subdata.Message,
                PlatformN = subdata.PlatformN,
            };

            foreach (string bid in subdata.DeviceN)
            {
                Box box = _list[bid];
                if (box != null)
                {
                    boxdata.DeviceN = bid;
                    string senddata = JsonConvert.SerializeObject(boxdata);
                    box.locker.EnterReadLock();
                    var client = box.ComClient;
                    box.locker.ExitReadLock();
                    _server.SendPack(client, senddata);
                }
            }
        }

        public void StartJob()
        {
            _server.Start();
            _remote.Initialization();
            _reporter.StartTimer();
        }

        private void _remote_OnSubscribeReceived(string message)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var jtoken = JToken.Parse(message);

                    string platid = jtoken["PlatformN"].Value<string>();
                    string type_str = jtoken["Type"].Value<string>();

                    if (!platid.Equals(_server.PlatformID))
                        return;


                    AMQDataType datatype = AMQDataTypeString.String2Type(type_str);
                    switch (datatype)
                    {
                        case AMQDataType.C0201_RTData:
                        case AMQDataType.C0202_InfoResponse:
                        case AMQDataType.C0203_ChartResponse:
                        case AMQDataType.C0301_AlertData:
                        case AMQDataType.C0302_WeatherData:
                            SimpleBoxDataSubscribeHandler(message);
                            break;
                    }
                }
                catch (System.Exception ex)
                {
                    Util.ConsolePrintError(ex.ToString(), ConsoleColor.Red);
                }
            });
        }

        private void _server_OnBoxDataRec(IComClient client, string data)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var jtoken = JToken.Parse((string)data);

                    string platid = jtoken["PlatformN"].Value<string>();
                    string type_str = jtoken["Type"].Value<string>();

                    if (!platid.Equals(_server.PlatformID))
                        return;
                    AMQDataType datatype = AMQDataTypeString.String2Type(type_str);
                    switch (datatype)
                    {
                        case AMQDataType.C0101_ReportState_HeartBeat:
                            HeartBeatBoxDataHandler(client, data);
                            break;
                        case AMQDataType.C0102_InfoRequest:
                        case AMQDataType.C0103_ChartRequest:
                            SimpleBoxDataReportHandler(data);
                            break;
                    }
                }
                catch (System.Exception ex)
                {
                    Util.ConsolePrintError(ex.ToString(), ConsoleColor.Red);
                }
            });
        }

        private void _server_OnBoxDisconnected(IComClient client)
        {
            //throw new NotImplementedException();
        }
    }
}
