using System;
using CommandCentral.Utilities;

namespace CommandCentral.Test
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                "1. Test Watchbill".ToConsole();

                if (Int32.TryParse(Console.ReadLine(), out var choice))
                {
                    Console.Clear();
                    
                    switch (choice)
                    {
                        case 1:
                        {
                            "Beginning watchbill test...".ToConsole();
                            WatchbillTester.StartTest();
                            break;
                        }
                    }
                }
            }
        }
    }
}