using System;
using System.Net;
using System.Threading;

namespace Server
{
    class Program
    {
        private const ushort PORT = 7171;

        private static Protocol m_Protocol = null;
        private static Game m_Game = null;

        public static CancellationTokenSource s_MasterToken;

        public static readonly string APP_PATH = AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string DATA_PATH = APP_PATH + "Data\\";
        public static readonly string WORLDS_PATH = DATA_PATH + "Worlds\\";

        static void Main(string[] args)
        {
            Start();
        }

        private static void Start()
        {
            s_MasterToken = new CancellationTokenSource();
            m_Game = new Game();
            m_Protocol = new Protocol(PORT);

            new Thread(new ThreadStart(ConsoleLoop)).Start();
        }

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

        public static void Stop(string error = null)
        {
            s_MasterToken.Cancel();

            if (error != null)
            {
                Console.WriteLine($"\n{error}");
            }
        }
    }
}
