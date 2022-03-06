using System;
using System.Collections.Generic;
using System.Text;

namespace ReservoirServer.Enterty
{
    class SimpleBoxData : IBoxData
    {
        public string PlatformN { get; set; }
        public string DeviceN { get; set; }
        public string Type { get; set; }
        

        public object Message { get; set; }

    }

    class BoxDataWithDate : SimpleBoxData
    {
        public DateTime @DateTime;
    }

    class BoxDataHeartBeat : IBoxData
    {
        public string PlatformN { get; set; }
        public string DeviceN { get; set; }
        public string Type { get; set; }

        public byte Battery { get; set; }
        public byte Signal { get; set; }
    }

    public interface IBoxData {
        //public string Type { get; set; }
    }
}
