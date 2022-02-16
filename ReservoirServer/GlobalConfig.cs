using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ReservoirServer
{
    static class GlobalConfig
    {
        private static IniReader ini = null;

        public static IPAddress BoxServerIP => IPAddress.Parse(ini.GetValue("boxserver", "listen"));
        public static ushort BoxServerPort => (ushort)ini.GetInteger("boxserver", "port");
        public static string PlatformID => ini.GetValue("boxserver", "serverid");
        public static int MaxClients => ini.GetInteger("boxserver", "maxclients");
        public static int HBTimeout => ini.GetInteger("boxserver", "hbtimeout");
        public static int KickTime => ini.GetInteger("boxserver", "kicktime");
        public static string AMQBrokeURI => ini.GetValue("activemq", "brokeuri");
       
        public static string QueueName => ini.GetValue("activemq", "queuename");
        public static string TopicName => ini.GetValue("activemq", "topicname");
        public static byte ReporterMaxCoreUsage => (byte)ini.GetInteger("reporter", "maxcoreusage");
        public static int ReportInterval => ini.GetInteger("reporter", "reportinterval");

        public static void ParseConfigFile(string fname)
        {
            ini = new IniReader(fname);
        }
    }

}
