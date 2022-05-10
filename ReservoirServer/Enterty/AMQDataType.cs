using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReservoirServer.Enterty
{
    enum AMQDataType
    {
        C0101_ReportState_HeartBeat,
        C0102_InfoRequest,
        C0103_ChartRequest,
        C0201_RTData,
        C0202_InfoResponse,
        C0203_ChartResponse,
        C0301_AlertData,
        C0302_WeatherData,
        C0401_PreReleaseData,
    }

    static class AMQDataTypeString
    {
        private static Dictionary<string, AMQDataType> _dic = new Dictionary<string, AMQDataType>
        {
            {"0101", AMQDataType.C0101_ReportState_HeartBeat },
            {"0102", AMQDataType.C0102_InfoRequest },
            {"0103", AMQDataType.C0103_ChartRequest },
            {"0201", AMQDataType.C0201_RTData },
            {"0202", AMQDataType.C0202_InfoResponse},
            {"0203", AMQDataType.C0203_ChartResponse},
            {"0301", AMQDataType.C0301_AlertData},
            {"0302", AMQDataType.C0302_WeatherData},
            {"0401", AMQDataType.C0401_PreReleaseData},
        };

        public static AMQDataType String2Type(string s)
        {
            return _dic[s];
        }

        [Obsolete("This function take low performance!")]
        public static string Type2String(AMQDataType t)
        {
            return _dic.FirstOrDefault(x => x.Value == t).Key;
        }
    }

}
