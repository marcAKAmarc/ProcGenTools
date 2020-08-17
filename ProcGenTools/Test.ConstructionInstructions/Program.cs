using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Construction.Models;
using System.Configuration;
using PuzzleBuilder;
using PuzzleBuilder.Process;
using RoomEditor.Models;

namespace Test.ConstructionInstructions
{
    class Program
    {
        static DateTime time, endtime;
        static void Main(string[] args)
        {
            var seed = -3;
            // var random = new Random(seed);
            // var grid = new IntentionGrid(11, 11);
            // var TilesConfig = new TilesetConfiguration(/*
            //     ConfigurationManager.AppSettings["mainTileset"],
            //     ConfigurationManager.AppSettings["hTileset"],
            //     ConfigurationManager.AppSettings["hPlainTileset"],
            //     ConfigurationManager.AppSettings["vTileset"],
            //     ConfigurationManager.AppSettings["ladderTileset"],
            //     ConfigurationManager.AppSettings["verticalExitTileset"],
            //     ConfigurationManager.AppSettings["doorTileset"],
            //     ConfigurationManager.AppSettings["buttonTileset"],
            //     ConfigurationManager.AppSettings["boxTileset"],
            //     ConfigurationManager.AppSettings["ropeTileset"],
            //     ConfigurationManager.AppSettings["conveyorTileset"],
            //     ConfigurationManager.AppSettings["elevatorTileset"],
            //     ConfigurationManager.AppSettings["shooterTileset"],
            //     ConfigurationManager.AppSettings["solidTileset"],
            //     ConfigurationManager.AppSettings["enemyTileset"],
            //     ConfigurationManager.AppSettings["emptyTileset"],
            //     ConfigurationManager.AppSettings["nondynamic"],
            //     ConfigurationManager.AppSettings["nondynamicstrict"],
            //     ConfigurationManager.AppSettings["walkableTileset"],
            //     ConfigurationManager.AppSettings["fallTileset"],
            //     ConfigurationManager.AppSettings["cautionTileset"],
            //     ConfigurationManager.AppSettings["errorTileset"],*/
            //      "..//..//PuzzleBuilder//WfcDebug//Current//",
            //      "..//..//PuzzleBuilder//TilesetsDebug//Current//"
            //);
            //var processor = new PuzzleBuilder.Creators.BasicCircuitProcess.PuzzleProcess(random, grid, TilesConfig);
            //var output = processor.CreateIt(new List<Point>() { new Point(0, 9) }, new List<Point>() { new Point(10, 7) });
            time = DateTime.Now;

            var factory = new ProcessFactory<AdvancedCircuitProcess.PuzzleProcess>(10,9, "..//..//PuzzleBuilder//WfcDebug//Current//", "..//..//PuzzleBuilder//TilesetsDebug//Current//");
            var puzzleResult = factory.GetPuzzle(seed, Portal.GetTestPortals(10,9));
            factory.SaveResult(
                ConfigurationManager.AppSettings["Output"].ToString()
                        .Replace("output.bmp", "")
            );

            var conFactory = new Construction.Factory("..//..//DebugGetTypeGrid//", 6, 6);
            var result = conFactory.GetGameElements(puzzleResult.TileMap, puzzleResult.Grid);
            endtime = DateTime.Now;
            Console.WriteLine("Total result time:  " + TimeSpan.FromTicks(endtime.Ticks - time.Ticks).ToString());

            time = DateTime.Now;
            var puzzleReseult2 = factory.GetPuzzle(seed + 1, Portal.GetTestPortals(10,9));
            var result2 = conFactory.GetGameElements(puzzleReseult2.TileMap, puzzleResult.Grid);
            endtime = DateTime.Now;
            Console.WriteLine("Total result2 time:  " + TimeSpan.FromTicks(endtime.Ticks - time.Ticks).ToString());

        }
    }
}
