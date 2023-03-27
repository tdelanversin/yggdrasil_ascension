using System;
using System.IO;
using System.Text.Json;
namespace YRG
{
    class Level
    {
        public int x { get; set; }
        public int y { get; set; }

        public int width { get; set; }

        public int height { get; set; }

    }
    class Json_Reader
    {
         static void deserialize()
        {
            //var path = @".\LevelData\data.json";

            //string text = File.ReadAllText(path);
            //var level = JsonSerializer.Deserialize<Level>(text);

           // Console.WriteLine($"First name: {level.x}");

        }
    }
}