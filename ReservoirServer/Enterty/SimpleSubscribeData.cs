using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace ReservoirServer.Enterty
{
    class SimpleSubscribeData : ISubscribeData
    {
        public string PlatformN { get; set; }
        public string[] DeviceN { get; set; }
        public string Type { get; set; }

        public DateTime @DateTime { get; set; }
        public object Message { get; set; }
    }

    public interface ISubscribeData {
        //public string Type { get; set; }
    }

    public class BoxStateMessageData
    {
        public string Device { get; set; }
        public byte DCDY { get; set; }
        public byte XHQD { get; set; }
        public byte State { get; set; }
        public DateTime LHBT { get; set; }
    }
    public class BoxStateReportSubData : ISubscribeData
    {
        public string PlatformN { get; set; }
        public ConcurrentBag<string> DeviceN { get; set; } = new ConcurrentBag<string>();
        public string Type { get; set; }

        public DateTime @DateTime { get; set; }
        public ConcurrentBag<BoxStateMessageData> Message { get; set; } = new ConcurrentBag<BoxStateMessageData>();
    }
}
