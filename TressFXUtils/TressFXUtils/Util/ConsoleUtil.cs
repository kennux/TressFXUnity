using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TressFXUtils.Util
{
    public class ConsoleUtil
    {
        public static void LogToConsole(string message, ConsoleColor color)
        {
            ConsoleColor c = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.WriteLine(message);

            Console.ForegroundColor = c;
        }
    }
}
