using System;
using System.Net;
using System.Threading;

namespace Server
{
    class Server
    {
        private static Protocol protocol = null;
        private const uint port = 7171;

        public static CancellationTokenSource s_MasterToken;

        static void Main(string[] args)
        {
            Start();
        }
        private static void Start()
        {
            s_MasterToken = new CancellationTokenSource();

            protocol = new Protocol(IPAddress.Any, port);

            new Thread(new ThreadStart(ConsoleLoop)).Start();
        }

        private static void Stop() => s_MasterToken.Cancel();

        private static void ConsoleLoop()
        {
            while (!s_MasterToken.IsCancellationRequested)
            {
                switch (Console.ReadLine().ToLower())
                {
                    case "/commands":
                    case "/help":
                        Console.WriteLine("\nValid Commands:\n/shutdown\n");
                        break;
                    case "/shutdown":
                        Stop();
                        break;
                    default:
                        Console.WriteLine("Invalid Command... Type '/help' to see valid commands.");
                        break;
                }
            }
        }
    }
}
