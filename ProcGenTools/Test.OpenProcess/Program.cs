using PuzzleBuilder;
using PuzzleBuilder.Process;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.openprocesstest
{
    class Program
    {
        //private Random random;
        static void Main(string[] args)
        {
            List<int> seeds = new List<int>() { 11 };
            var factory = new ProcessFactory<AdvancedCircuitProcess.PuzzleProcess>(11, 9, "..//..//WfcDebug//Current//", "..//..//TilesetsDebug//Current//");
            var failures = 0;
            foreach (var i in seeds)
            {
                var result = factory.GetPuzzle(i, new List<Point>() { new Point(0, 6) }, new List<Point>() { new Point(10, 4) });
                factory.SaveResult(
                    ConfigurationManager.AppSettings["Output"].ToString()
                );
                failures += factory.currentAttempt;
            }

            //int failedAttempts = 0;

            //var grid = new IntentionGrid(11, 11);//reuse
            //var random = new marcRandom(10);
            //var TilesConfig = new TilesetConfiguration(
            //                    "..//..//WfcDebug//Current//",
            //                    "..//..//TilesetsDebug//Current//"
            //                );
            //AdvancedCircuitProcess.PuzzleProcess processor = null; 
            //foreach (var i in seeds)
            //{
            //    var seed = i;
            //    PuzzleInfo output = null;
            //    //var attempt = 0;
            //    //var maxAttempt = 10;
            //    //while ((output == null || output.Success == false) && attempt < maxAttempt)
            //    //{

            //    //    random = new marcRandom(seed);
            //    //    if (processor == null)
            //    //        processor = new AdvancedCircuitProcess.PuzzleProcess(random, grid, TilesConfig);
            //    //    else //reuse
            //    //        processor.ClearForReuse(random);

            //    //    output = processor.CreateIt(new List<Point>() { new Point(0, 5) }, new List<Point>() { new Point(grid.Width-1, 5) });

            //    //    if (output == null || output.Success == false)
            //    //    {
            //    //        seed += 1000;
            //    //        failedAttempts += 1;
            //    //    }
            //    //}
            //    if (output != null)
            //        BitmapOperations.SaveBitmapToFile(
            //            ConfigurationManager.AppSettings["Output"].ToString()
            //                .Replace(".bmp", i.ToString() + ".bmp")
            //            , output.TileMap
            //        );

            //}
            Console.WriteLine("Puzzle.01 Created.  Total Failed Attampts: " + failures.ToString());
            Console.ReadKey();
        }
    }
}
