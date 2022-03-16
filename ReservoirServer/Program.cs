using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
//using System.Text.Json;

using ReservoirServer.Driver;
using ReservoirServer.Enterty;

namespace ReservoirServer
{
    class Program
    {
        //TODO: Log file, Safety(Encryption), JSON error catch, system service
        public static readonly string version = "1.07";

#if DEBUG
        public static string VERSION => version + "a";
#elif RELEASE
        public static string VERSION => version + "b";
#endif

        static AMQCommunicator communicator;
        static SimpleBoxServer server;
        static BoxList boxlist;
        static SimpleBoxAdapter adapter;
         
        static string logpath=null;
        static void Main(string[] args)
        {
            CommandParser cmd = new CommandParser(args);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string inipath = null;
            bool ver = false;
            cmd.Parse(out inipath, out logpath, out ver);
            if(ver)
            {
                Console.WriteLine("ReservoirServer v" + VERSION + " author: Jennings(aka NeNe)" + Environment.NewLine + "Copyleft with GPLv3 2021-2022 All Rights Reversed!");
                return;
            }
            AttatchTester();

            Console.WriteLine("ReservoirServer v" + VERSION + " author: Jennings(aka NeNe)");
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Warnning: This version is for testing, no system service, data encryption, log file or exception handler be implemented!");
            Console.ForegroundColor = color;

            Console.WriteLine("Start to initialization...");

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //ConsoleErrorWriterDecorator.SetToConsole();
            GlobalConfig.ParseConfigFile(inipath == null ? "./config.ini" : inipath);

            boxlist = new BoxList(GlobalConfig.MaxClients, GlobalConfig.ReporterMaxCoreUsage);

            communicator = new AMQCommunicator(
                GlobalConfig.AMQBrokeURI,
                GlobalConfig.QueueName,
                GlobalConfig.TopicName,
                GlobalConfig.PlatformID
            );
            //communicator.Initialization();

            server = new SimpleBoxServer(
                GlobalConfig.PlatformID,
                GlobalConfig.BoxServerIP,
                GlobalConfig.BoxServerPort
            );
            

            adapter = new SimpleBoxAdapter(communicator, server, boxlist);
            Console.WriteLine("All moduels initialized finished! Starting listener...");
            
            adapter.StartJob();

            //communicator.MultiSendTest();
            
            Console.Write("Server Listening...");
            Util.ConsolePrintLine("Press <Enter> key to exit", ConsoleColor.White, ConsoleColor.Green);
            Console.ReadLine();
           
        }

        private static void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Util.ConsolePrintError(e.Exception.ToString(), ConsoleColor.Red);
            e.SetObserved();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            //var color = Console.ForegroundColor;
            //Console.ForegroundColor = ConsoleColor.Red;
            Util.ConsolePrintError(ex.ToString(), ConsoleColor.Red);
            if(e.IsTerminating)
            {
                Console.WriteLine();
                Util.ConsolePrintLine("Fatal error occured! Press Any Key to Exit!", ConsoleColor.White, ConsoleColor.Red);
                Console.ReadKey();
                Environment.Exit(1);
            }
            
            //Console.ForegroundColor = color;

        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            //Do clean up!
            adapter.CleanUpJob();
            
            Console.WriteLine("Clean up finished! Bye!");
            

        }

        [Conditional("DEBUG")]
        static void AttatchTester()
        {
            bool isRunning = Process.GetProcessesByName("ActiveMQTester").Length > 0;
            string fname = @"D:\QianZong\2021Fuyu\Reservoir\Server\ActiveMQTester\bin\Debug\netcoreapp3.1\ActiveMQTester.exe";
            if (!isRunning && File.Exists(fname))
                Process.Start(fname);
        }
    }
}
