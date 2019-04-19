using System;

namespace SerialHandler.Demo.NETCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter serial port name:");
            var portName = Console.ReadLine();
            Console.WriteLine("Enter serial port baud rate:");
            var baudRate = Console.ReadLine();
            MyCommandService _commandService = new MyCommandService(portName, Convert.ToInt32(baudRate));
            _commandService.HeartReport += _commandService_HeartReport;
            _commandService.Open();
            while (true)
            {
                Console.WriteLine("Enter command");
                Console.WriteLine("QueryState: Send query-state command to serinal port.");
                Console.WriteLine("Exit: Exit the application.");
                var key = Console.ReadLine();
                if (key.Equals("QueryState", StringComparison.OrdinalIgnoreCase))
                {
                    bool isError;
                    var reault = _commandService.QueryState(out isError);
                    if (reault == SendResult.Success)
                    {
                        Console.WriteLine($"The terminal state is {isError}.");
                        return;
                    }
                    else
                    {
                        Console.WriteLine($"Command failed:{reault}.");
                    }
                }
                else if (key.Equals("Exit", StringComparison.OrdinalIgnoreCase))
                {
                    _commandService.Close();
                    Environment.Exit(0);
                }
            }
        }

        private static void _commandService_HeartReport()
        {
            Console.WriteLine($"Terminal Report State:{DateTime.Now}");
        }
    }
}
