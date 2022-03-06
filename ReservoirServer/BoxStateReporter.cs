using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Diagnostics;

using ReservoirServer.Enterty;
using Newtonsoft.Json;

namespace ReservoirServer
{
    class BoxStateReporter : IDisposable
    {
        private BoxList list;
        private AMQCommunicator remote;
        private int interval;
        private TimeSpan kicktime, hbtimeout;
        private Timer timer;
        private bool disposedValue;
        private BoxStateReportSubData report_data;

        public BoxStateReporter(BoxList lst, AMQCommunicator comm)
        {
            list = lst;
            remote = comm;
            interval = GlobalConfig.ReportInterval * 1000;
            kicktime = new TimeSpan(0, 0, GlobalConfig.KickTime);
            if (GlobalConfig.KickTime == 0)
                kicktime.Add(new TimeSpan(99999, 9, 9, 9));
            hbtimeout = new TimeSpan(0, 0, GlobalConfig.HBTimeout);
            timer = new Timer();
            timer.Interval = interval;
            timer.Stop();
            timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                timer.Stop();
                report_data = new BoxStateReportSubData();
                report_data.PlatformN = remote.PlatformID;
                report_data.Type = AMQDataTypeString.Type2String(AMQDataType.C0101_ReportState_HeartBeat);
                var loop_func = list.SmartTraverseMethod;
                loop_func(Report);
                report_data.DateTime = DateTime.Now;
                string json = JsonConvert.SerializeObject(
                    report_data,
                    new JsonSerializerSettings() { DateFormatString = "yyyy-MM-dd hh:mm:ss" }
                    );
                report_data = null;
                remote.SendQueue(json);
                timer.Start();
            }
            catch (System.Exception ex)
            {
                Util.ConsolePrintError(ex.ToString(), ConsoleColor.Red);
            }
        }

        private void Report(KeyValuePair<string, Box> val)
        {
            Box box = val.Value;
            try
            {
                
                DateTime now = DateTime.Now;

                box.locker.EnterWriteLock();
                if ((now - box.LastHBTime) > kicktime)
                {
                    list.Remove(box.ID);
                    box.ComClient.Disconnect();
                    box.locker.ExitWriteLock();
                    return;
                }
                if (box.State == BoxState.Online)
                {
                    if ((now - box.LastHBTime) > hbtimeout)
                    {
                        box.State = BoxState.Timeout;
                    }
                    //TODO
                }
                box.locker.ExitWriteLock();
                box.locker.EnterReadLock();
                report_data.DeviceN.Add(box.ID);
                report_data.Message.Add(new BoxStateMessageData()
                {
                    State = (byte)box.State,
                    Device = box.ID,
                    DCDY = box.Battery,
                    XHQD = box.SignalStrength,
                    LHBT = box.LastHBTime
                });
                box.locker.ExitReadLock();
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
            finally
            {
                box.locker.ExitWriteLock();
                box.locker.ExitReadLock();
            }

        }

        public void StartTimer()
        {
            timer.Start();
        }

        public void StopTimer()
        {
            timer.Stop();
        }





        [Conditional("DEBUG")]
        public void ParallelTest()
        {
            char ch = 'a';
            list = new BoxList(26, 7);
            for (int i = 0; i < 26; i++)
            {
                string name = "";
                for (int j = 0; j < 26; j++)
                {
                    name += (char)(ch + (i % 26));
                }
                list.AddNew(new Box(i.ToString(), null) { Version = name });

            }
            Stopwatch sw = new Stopwatch();
            StringBuilder sb = new StringBuilder();
            //             list.ParallelTraverse((pair) => {
            //                 Box bx = pair.Value;
            //                 sb.Append(bx.Version);
            //             });
            sw.Start();
            list.SerialTraverse((pair) =>
            {
                //Thread.Sleep(1);
                //Box bx = pair.Value;
                sb.Append(pair.Value.Version);
            });
            sw.Stop();
            Console.WriteLine(sb.ToString());
            Console.WriteLine($"Use time={sw.ElapsedMilliseconds}ms");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    timer.Stop();
                    timer.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~BoxStateReporter()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
