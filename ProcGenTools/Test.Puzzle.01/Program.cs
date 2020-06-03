using ProcGenTools.DataProcessing;
using ProcGenTools.DataStructures;
using ProcGenTools.Test;
using PuzzleBuilder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Test.Puzzle._01
{
    class Program
    {
        private Random random;
        static void Main(string[] args)
        {
            List<int> seeds = new List<int>() {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19 };
            foreach (var i in seeds)
            {
                var seed = i;
                PuzzleInfo output = null;
                var attempt = 0;
                var maxAttempt = 10;
                while ((output == null || output.Success == false) && attempt < maxAttempt)
                {

                    var random = new marcRandom(seed);
                    var grid = new IntentionGrid(11, 11);
                    var TilesConfig = new TilesetConfiguration(
                                ConfigurationManager.AppSettings["mainTileset"],
                                ConfigurationManager.AppSettings["hTileset"],
                                ConfigurationManager.AppSettings["hPlainTileset"],
                                ConfigurationManager.AppSettings["vTileset"],
                                ConfigurationManager.AppSettings["ladderTileset"],
                                ConfigurationManager.AppSettings["verticalExitTileset"],
                                ConfigurationManager.AppSettings["doorTileset"],
                                ConfigurationManager.AppSettings["buttonTileset"],
                                ConfigurationManager.AppSettings["boxTileset"],
                                ConfigurationManager.AppSettings["ropeTileset"],
                                ConfigurationManager.AppSettings["conveyorTileset"],
                                ConfigurationManager.AppSettings["elevatorTileset"],
                                ConfigurationManager.AppSettings["shooterTileset"],
                                ConfigurationManager.AppSettings["solidTileset"],
                                ConfigurationManager.AppSettings["enemyTileset"],
                                ConfigurationManager.AppSettings["emptyTileset"],
                                ConfigurationManager.AppSettings["nondynamic"],
                                ConfigurationManager.AppSettings["nondynamicstrict"],
                                ConfigurationManager.AppSettings["walkableTileset"],
                                ConfigurationManager.AppSettings["fallTileset"],
                                ConfigurationManager.AppSettings["cautionTileset"],
                                ConfigurationManager.AppSettings["errorTileset"],
                                "..//..//WfcDebug//Current//",
                                "..//..//TilesetsDebug//Current//"
                            );
                    var processor = new PuzzleBuilder.Creators.BasicCircuitProcess.PuzzleProcess(random, grid, TilesConfig);
                    output = processor.CreateIt(new List<Point>() { new Point(0, 9) }, new List<Point>() { new Point(10, 7) });

                    if(output == null || output.Success == false)
                        seed += 1000;
                }
                if (output != null && output.Success == true)
                    BitmapOperations.SaveBitmapToFile(
                        ConfigurationManager.AppSettings["Output"].ToString()
                            .Replace(".bmp", i.ToString() + ".bmp")
                        , output.TileMap
                    );
                
            }
            Console.WriteLine("Puzzle.01 Created.");
            Console.ReadKey();
        }
    }
}