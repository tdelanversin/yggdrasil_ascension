using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using System.Data;
using SharpFont.PostScript;
using System.IO;
using YRG;

namespace YRG {
    public class Csv_reader
    {
        static public int [] Csvread()
        {
            var path = @"C:\Users\dcolo\source\repos\gamelab2023-rapture-team3\src\LevelData\Collisions.csv";
            using (var sr = new StreamReader(path))
            {
                 var myArray = sr.ReadToEnd()
                    .Split('\n')
                    .SelectMany(s => s.Split(',')
                        .Select(x => int.Parse(x)))
                    .ToArray<int>();

                foreach (var x in myArray)
                    Console.WriteLine(x);
                return myArray;
            }
            
        }
        static void Main()
        {
            Csvread();
        }
        
    }
}
