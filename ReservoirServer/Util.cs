using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReservoirServer
{
    class Util
    {
        public static void ConsolePrintLine(string text,ConsoleColor? forecolor,ConsoleColor? backcolor=null)
        {
            var fc = Console.ForegroundColor;
            var bc = Console.BackgroundColor;
            if (forecolor != null)
                Console.ForegroundColor = (ConsoleColor)forecolor;
            if (backcolor != null)
                Console.BackgroundColor = (ConsoleColor)backcolor;
            Console.Write(text);
            Console.ForegroundColor = fc;
            Console.BackgroundColor = bc;
            Console.WriteLine();
        }

        public static void ConsolePrintError(string text, ConsoleColor? forecolor, ConsoleColor? backcolor = null)
        {
            var fc = Console.ForegroundColor;
            var bc = Console.BackgroundColor;
            if (forecolor != null)
                Console.ForegroundColor = (ConsoleColor)forecolor;
            if (backcolor != null)
                Console.BackgroundColor = (ConsoleColor)backcolor;
            Console.Error.Write(text);
            Console.ForegroundColor = fc;
            Console.BackgroundColor = bc;
            Console.Error.WriteLine();
        }
    }
    public class ConsoleErrorWriterDecorator : TextWriter
    {
        private TextWriter m_OriginalConsoleStream;

        public ConsoleErrorWriterDecorator(TextWriter consoleTextWriter)
        {
            m_OriginalConsoleStream = consoleTextWriter;
        }

        public override void WriteLine(string value)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            m_OriginalConsoleStream.WriteLine(value);

            Console.ForegroundColor = originalColor;
        }

        public override Encoding Encoding
        {
            get { return Encoding.Default; }
        }

        public static void SetToConsole()
        {
            Console.SetError(new ConsoleErrorWriterDecorator(Console.Error));
        }
    }
}
