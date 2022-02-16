using System;
using System.Collections.Generic;
using System.Text;

namespace ReservoirServer
{
    class CommandParser
    {
        string[] args;
        public CommandParser(string[] arg)
        {
            args = arg;
        }

        public void Parse(out string inipath,out string logpath,out bool ver)
        {
            inipath = logpath = null;
            ver = false;
            for (int i = 0; i < args.Length; i++)
            {
                switch(args[i])
                {
                    case "-l":logpath = args[++i];break;
                    case "-c":inipath = args[++i];break;
                    case "-v":ver = true;break;
                    case "-h":
                        ver = true;
                        Console.WriteLine("Parameters:");
                        Console.WriteLine("-l <path>: Set log file path, by default is \"./config.ini\".");
                        Console.WriteLine("-c <path>: Set config file path, by default is \"./server.log\".");
                        Console.WriteLine("-v: See program version.");
                        Console.WriteLine("-h: See this help text.");
                        Console.WriteLine();
                        break;
                }

            }
        }
    }
}
