using ProcGenTools.DataProcessing;
using ProcGenTools.DataStructures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Configuration;
namespace PuzzleBuilder
{

    public static class Helpers
    {
        public static void clearFolder(string FolderName)
        {
            DirectoryInfo dir = new DirectoryInfo(FolderName);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                clearFolder(di.FullName);
                di.Delete();
            }
        }

        public static WcfGrid InitWcfGrid(Random random, IntentionGrid grid, TilesetConfiguration TilesConfig)
        {
            var WcfGrid = new WcfGrid(random);
            WcfGrid.Init(grid.Width, grid.Height, 1, TilesConfig.MainDistinct.Select(x=>(IOpinionatedItem)x).ToList());
            var shape = new List<WcfVector>().Cross3dShape();
            WcfGrid.SetInfluenceShape(shape);
            WcfGrid.handlePropagation(WcfGrid.SuperPositions[WcfGrid.Width / 2, WcfGrid.Height / 2, 0]);
            return WcfGrid;
        }

        public static void ApplyIntentionToGrid(IntentionGrid grid, WcfGrid wcfGrid, TilesetConfiguration tilesconfig, iMeaningConverter converter)
        {
            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    if(y == 8 && x == 7){
                        var breaka = "here";
                    }
                    var intentions = grid.Positions[x, y].Intentions;
                    if (intentions.Count != 0)
                    {
                        List<OpinionatedItem<Bitmap>> crossedTiles = new List<OpinionatedItem<Bitmap>>();
                        var firsttime = true;
                        foreach (var intention in intentions)
                        {
                            if (firsttime)
                            {
                                crossedTiles.AddRange(converter.MeaningToTiles(intention.Meaning, tilesconfig));
                                firsttime = false;
                            }
                            else
                            {
                                crossedTiles = crossedTiles.Where(c => converter.MeaningToTiles(intention.Meaning, tilesconfig).Any(n => n.Id == c.Id)).ToList();
                            }
                        }
                        if(crossedTiles.Count == 0)
                        {
                            Console.WriteLine("Crossing Tile Intentions yeilded no items!");
                        }
                        wcfGrid.SuperPositions[x, y, 0].CollapseToItems(crossedTiles.Select(c => c.Id).ToList(), true);
                        if (!wcfGrid.SuperPositions[x, y, 0].slots.Any(s => !s.Collapsed))
                        {
                            Console.WriteLine("Collapse To Specific Item failed for Intentions:");
                            foreach (var meaning in intentions.Select(i => i.Meaning))
                            {
                                Console.WriteLine(" -" + meaning.ToString());
                            }
                            DebugWfcPrint(crossedTiles, wcfGrid, x, y, tilesconfig.WFCdebugFolderPath);
                        }

                        var result = wcfGrid.handlePropagation(wcfGrid.SuperPositions[x, y, 0]);

                        if (!result)
                        {
                            Console.WriteLine("WcfGrid fucke dup");
                            //throw new Exception("WcfGrid fucke dup");
                            return;
                        }
                    }
                }
            }

            var recurseresult = wcfGrid.CollapseAllRecursive();
            if (!recurseresult)
            {
                Console.WriteLine("WcfGrid collapse recursive fucke dup");
                throw new Exception("WcfGrid collapse recursive fucke dup");
                return;
            }


        }

        private static List<List<Bitmap>> ToTilesList(WcfGrid grid, TilesetConfiguration TilesConfig)
        {
            var Tiles = new List<List<Bitmap>>();
            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    if (y == 0)
                    {
                        Tiles.Add(new List<Bitmap>());
                    }

                    Tiles.Last().Add(grid.SuperPositions[x, y, 0].FinalPayloadIfExists<Bitmap>(TilesConfig.CautionTile, TilesConfig.ErrorTile));
                }
            }
            return Tiles;
        }

        public static Bitmap ToBitmap(WcfGrid grid, TilesetConfiguration TilesConfig)
        {
            return BitmapOperations.CreateBitmapFromTiles(ToTilesList(grid, TilesConfig));
        }

        public static void DebugWfcPrint(List<OpinionatedItem<Bitmap>> Attempted, WcfGrid WcfGrid, int x, int y, string folderPath)
        {
            Console.WriteLine("Collapse To Specific Item failed.");
            List<Bitmap> topBmps = new List<Bitmap>();
            List<Bitmap> bottomBmps = new List<Bitmap>();
            List<Bitmap> leftBmps = new List<Bitmap>();
            List<Bitmap> rightBmps = new List<Bitmap>();
            if (y - 1 >= 0)
                topBmps = WcfGrid.SuperPositions[x, y - 1, 0].slots.Where(s => !s.Collapsed).Select(s => (Bitmap)s.item.GetItem()).ToList();
            if (y + 1 < WcfGrid.Height)
                bottomBmps = WcfGrid.SuperPositions[x, y + 1, 0].slots.Where(s => !s.Collapsed).Select(s => (Bitmap)s.item.GetItem()).ToList();
            if (x - 1 >= 0)
                leftBmps = WcfGrid.SuperPositions[x - 1, y, 0].slots.Where(s => !s.Collapsed).Select(s => (Bitmap)s.item.GetItem()).ToList();
            if (x + 1 < WcfGrid.Width)
                rightBmps = WcfGrid.SuperPositions[x + 1, y, 0].slots.Where(s => !s.Collapsed).Select(s => (Bitmap)s.item.GetItem()).ToList();

            List<Bitmap> currentBmps = Attempted.Select(s => (Bitmap)s.GetItem()).ToList();

            //string folderPath = ConfigurationManager.AppSettings["WfcDebugFolder"].ToString();//../../WfcDebug/Current/";
            var subPath = "Top/";
            Helpers.clearFolder(folderPath + subPath);
            for (var i = 0; i < topBmps.Count(); i++)
            {
                var filename = "bmp_" + i.ToString() + ".bmp";
                BitmapOperations.SaveBitmapToFile(folderPath + subPath + filename, topBmps[i]);
            }
            subPath = "Bottom/";
            Helpers.clearFolder(folderPath + subPath);
            for (var i = 0; i < bottomBmps.Count(); i++)
            {
                var filename = "bmp_" + i.ToString() + ".bmp";
                BitmapOperations.SaveBitmapToFile(folderPath + subPath + filename, bottomBmps[i]);
            }
            subPath = "Left/";
            Helpers.clearFolder(folderPath + subPath);
            for (var i = 0; i < leftBmps.Count(); i++)
            {
                var filename = "bmp_" + i.ToString() + ".bmp";
                BitmapOperations.SaveBitmapToFile(folderPath + subPath + filename, leftBmps[i]);
            }
            subPath = "Right/";
            Helpers.clearFolder(folderPath + subPath);
            for (var i = 0; i < rightBmps.Count(); i++)
            {
                var filename = "bmp_" + i.ToString() + ".bmp";
                BitmapOperations.SaveBitmapToFile(folderPath + subPath + filename, rightBmps[i]);
            }
            subPath = "CurrentOptions/";
            Helpers.clearFolder(folderPath + subPath);
            for (var i = 0; i < currentBmps.Count(); i++)
            {
                var filename = "bmp_" + i.ToString() + ".bmp";
                BitmapOperations.SaveBitmapToFile(folderPath + subPath + filename, currentBmps[i]);
            }
        }
    }
    
}
