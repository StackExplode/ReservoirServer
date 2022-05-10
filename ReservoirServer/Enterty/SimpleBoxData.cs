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
        public DateTime @DateTime { get; set; }
    }

    class BoxAlertData : BoxDataWithDate
    {
        public DateTime DateEnd { get; set; }
        public BoxAlertData() { }
        public BoxAlertData(BoxDataWithDate data)
        {
            this.PlatformN = data.PlatformN;
            this.DeviceN = data.DeviceN;
            this.Type = data.Type;
            this.Message = data.Message;
            this.DateTime = data.DateTime;

        }
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
