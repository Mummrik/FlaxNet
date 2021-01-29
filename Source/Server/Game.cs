using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Server
{
    internal class Game
    {
        public const int WORLD_CHUNKSIZE = 32;
        public const int WORLD_CHUNKSIZE_HALF = (int)(WORLD_CHUNKSIZE * 0.5f);
        public const int WORLD_TILESIZE = 100;
        public static List<World> s_WorldInstances;

        public Game()
        {
            Console.WriteLine(">> Initialize Game...");
            Start();
        }

        private void Start()
        {
            InitializeWorld();
        }
        private void Stop()
        {
        }

        private void InitializeWorld()
        {
            Console.Write("\t>> Initializing GameWorld...");
            if (Directory.Exists(Program.WORLDS_PATH))
            {
                int instanceAmount = Directory.GetFiles(Program.WORLDS_PATH).Length;
                if (instanceAmount > 0)
                {
                    DateTime start = DateTime.Now;
                    s_WorldInstances = new List<World>();

                    for (int i = 0; i < instanceAmount; i++)
                    {
                        using (StreamReader sr = new StreamReader($"{Program.WORLDS_PATH + i}.json"))
                        {
                            s_WorldInstances.Add(JsonConvert.DeserializeObject<World>(sr.ReadToEnd()));
                        }
                    }
                    Console.WriteLine($"\tDone in '{String.Format("{0:N3}", (DateTime.Now - start).TotalSeconds)}' sec");
                }
                else
                {
                    Program.Stop($"\nERROR: Couldn't find any world data @ {Program.WORLDS_PATH}");
                }
            }
            else
            {
                Program.Stop($"\nERROR: Couldn't find path '{Program.WORLDS_PATH}'");
            }
        }

    }
}