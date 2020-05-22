using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Construction.Models;
using System.Configuration;
using PuzzleBuilder;

namespace Test.ConstructionInstructions
{
    class Program
    {
        static void Main(string[] args)
        {
            var seed = 1;
            var random = new Random(seed);
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
                        ConfigurationManager.AppSettings["emptyTileset"],
                        ConfigurationManager.AppSettings["nondynamic"],
                        ConfigurationManager.AppSettings["nondynamicstrict"],
                        ConfigurationManager.AppSettings["walkableTileset"],
                        ConfigurationManager.AppSettings["fallTileset"],
                        ConfigurationManager.AppSettings["cautionTileset"],
                        ConfigurationManager.AppSettings["errorTileset"]
                    );
            var processor = new PuzzleBuilder.Creators.BasicCircuitProcess.PuzzleProcess(random, grid, TilesConfig);
            var output = processor.CreateIt(new List<Point>() { new Point(0, 9) }, new List<Point>() { new Point(10, 7) });

            var inputImg = Image.FromFile(ConfigurationManager.AppSettings["TempInput"]) as Bitmap;
            var constructionProcessor = new Processing.Processor(6, 6, output.TileMap, output.Grid);
        }
    }
}
