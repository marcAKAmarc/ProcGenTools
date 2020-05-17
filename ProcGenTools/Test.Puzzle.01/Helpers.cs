﻿//using ProcGenTools.DataProcessing;
//using ProcGenTools.DataStructures;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Test.Puzzle._01
//{
//    public static class Helpers
//    {
//        public static void clearFolder(string FolderName)
//        {
//            DirectoryInfo dir = new DirectoryInfo(FolderName);

//            foreach (FileInfo fi in dir.GetFiles())
//            {
//                fi.Delete();
//            }

//            foreach (DirectoryInfo di in dir.GetDirectories())
//            {
//                clearFolder(di.FullName);
//                di.Delete();
//            }
//        }

//        public static List<OpinionatedItem<Bitmap>> MeaningToTiles(Meaning meaning, TilesetConfiguration config)
//        {
//            var result = new List<OpinionatedItem<Bitmap>>();
//            switch (meaning)
//            {
//                case Meaning.HTraversablePlain:
//                    result.AddRange(config.HorizontalTraversablePlainTiles);
//                    break;
//                case Meaning.Circuit:
//                    result.AddRange(config.HorizontalTraversablePlainTiles);
//                    result.AddRange(config.VerticalTraversableTiles);
//                    break;
//                case Meaning.HTraversable:
//                case Meaning.GroundLevel:
//                    result.AddRange(config.HorizontalTraversableTiles);
//                    break;
//                case Meaning.VTraversable:
//                    result.AddRange(config.VerticalTraversableTiles);
//                    break;
//                case Meaning.Ladder:
//                    result.AddRange(config.LadderTiles);
//                    break;
//                case Meaning.ToggleDoor:
//                    result.AddRange(config.DoorTiles);
//                    break;
//                case Meaning.Button:
//                    result.AddRange(config.ButtonTiles);
//                    break;
//                case Meaning.Rope:
//                    result.AddRange(config.RopeTiles);
//                    break;
//                case Meaning.Elevator:
//                    result.AddRange(config.ElevatorTiles);
//                    break;
//                case Meaning.Conveyor:
//                    result.AddRange(config.ConveyorTiles);
//                    break;
//                case Meaning.ExitPath:
//                case Meaning.EntrancePath:
//                    result.AddRange(config.HorizontalTraversablePlainTiles);
//                    result.AddRange(config.VerticalTraversableTiles);
//                    result.AddRange(config.DoorTiles);
//                    break;
//                case Meaning.Solid:
//                    result.AddRange(config.SolidTiles);
//                    break;
//                case Meaning.Empty:
//                    result.AddRange(config.EmptyTiles);
//                    break;
//            }
//            return result;
//        }

//        public static WcfGrid InitWcfGrid(Random random, IntentionGrid grid, TilesetConfiguration TilesConfig)
//        {
//           var WcfGrid = new WcfGrid(random);
//            WcfGrid.Init(grid.Width, grid.Height, 1, TilesConfig.MainDistinct);
//            var shape = new List<WcfVector>().Cross3dShape();
//            WcfGrid.SetInfluenceShape(shape);
//            WcfGrid.handlePropagation(WcfGrid.SuperPositions[WcfGrid.Width / 2, WcfGrid.Height / 2, 0]);
//            return WcfGrid;
//        }

//        public static void ApplyIntentionToGrid(IntentionGrid grid, WcfGrid wcfGrid, TilesetConfiguration tilesconfig)
//        {
//            for (var x = 0; x < grid.Width; x++)
//            {
//                for (var y = 0; y < grid.Height; y++)
//                {
//                    var intentions = grid.Positions[x, y].Intentions;
//                    if (intentions.Count != 0)
//                    {
//                        List<OpinionatedItem<Bitmap>> crossedTiles = new List<OpinionatedItem<Bitmap>>();
//                        var firsttime = true;
//                        foreach (var intention in intentions)
//                        {
//                            if (firsttime)
//                            {
//                                crossedTiles.AddRange(MeaningToTiles(intention.Meaning, tilesconfig));
//                                firsttime = false;
//                            }
//                            else
//                            {
//                                crossedTiles = crossedTiles.Where(c => MeaningToTiles(intention.Meaning, tilesconfig).Any(n => n.Id == c.Id)).ToList();
//                            }
//                        }

//                        wcfGrid.SuperPositions[x, y, 0].CollapseToItems(crossedTiles.Select(c => c.Id).ToList(), true);
//                        if (!wcfGrid.SuperPositions[x, y, 0].slots.Any(s => !s.Collapsed))
//                        {
//                            Console.WriteLine("Collapse To Specific Item failed for Intentions:");
//                            foreach (var meaning in intentions.Select(i => i.Meaning))
//                            {
//                                Console.WriteLine(" -" + meaning.ToString());
//                            }
//                            DebugWfcPrint(crossedTiles, wcfGrid, x, y);
//                        }

//                        var result = wcfGrid.handlePropagation(wcfGrid.SuperPositions[x, y, 0]);

//                        if (!result)
//                        {
//                            Console.WriteLine("WcfGrid fucke dup");
//                            return;
//                        }
//                    }
//                }
//            }

//            var recurseresult = wcfGrid.CollapseAllRecursive();
//            if (!recurseresult)
//            {
//                Console.WriteLine("WcfGrid collapse recursive fucke dup");
//                return;
//            }


//        }

//        private static List<List<Bitmap>> ToTilesList(WcfGrid grid, TilesetConfiguration TilesConfig)
//        {
//            var Tiles = new List<List<Bitmap>>();
//            for (var x = 0; x < grid.Width; x++)
//            {
//                for (var y = 0; y < grid.Height; y++)
//                {
//                    if (y == 0)
//                    {
//                        Tiles.Add(new List<Bitmap>());
//                    }

//                    Tiles.Last().Add(grid.SuperPositions[x, y, 0].FinalPayloadIfExists<Bitmap>(TilesConfig.CautionTile, TilesConfig.ErrorTile));
//                }
//            }
//            return Tiles;
//        }

//        public static Bitmap ToBitmap(WcfGrid grid, TilesetConfiguration TilesConfig)
//        {
//            return BitmapOperations.CreateBitmapFromTiles(ToTilesList(grid, TilesConfig));
//        }

//        public static void DebugWfcPrint(List<OpinionatedItem<Bitmap>> Attempted, WcfGrid WcfGrid, int x, int y)
//        {
//            Console.WriteLine("Collapse To Specific Item failed.");
//            List<Bitmap> topBmps = new List<Bitmap>();
//            List<Bitmap> bottomBmps = new List<Bitmap>();
//            List<Bitmap> leftBmps = new List<Bitmap>();
//            List<Bitmap> rightBmps = new List<Bitmap>();
//            if (y - 1 >= 0)
//                topBmps = WcfGrid.SuperPositions[x, y - 1, 0].slots.Where(s => !s.Collapsed).Select(s => (Bitmap)s.item.GetItem()).ToList();
//            if (y + 1 < WcfGrid.Height)
//                bottomBmps = WcfGrid.SuperPositions[x, y + 1, 0].slots.Where(s => !s.Collapsed).Select(s => (Bitmap)s.item.GetItem()).ToList();
//            if (x - 1 >= 0)
//                leftBmps = WcfGrid.SuperPositions[x - 1, y, 0].slots.Where(s => !s.Collapsed).Select(s => (Bitmap)s.item.GetItem()).ToList();
//            if (x + 1 < WcfGrid.Width)
//                rightBmps = WcfGrid.SuperPositions[x - 1, y, 0].slots.Where(s => !s.Collapsed).Select(s => (Bitmap)s.item.GetItem()).ToList();

//            List<Bitmap> currentBmps = Attempted.Select(s => (Bitmap)s.GetItem()).ToList();

//            string path = "../../WfcDebug/Current/";
//            var subPath = "Top/";
//            Helpers.clearFolder(path + subPath);
//            for (var i = 0; i < topBmps.Count(); i++)
//            {
//                var filename = "bmp_" + i.ToString() + ".bmp";
//                BitmapOperations.SaveBitmapToFile(path + subPath + filename, topBmps[i]);
//            }
//            subPath = "Bottom/";
//            Helpers.clearFolder(path + subPath);
//            for (var i = 0; i < bottomBmps.Count(); i++)
//            {
//                var filename = "bmp_" + i.ToString() + ".bmp";
//                BitmapOperations.SaveBitmapToFile(path + subPath + filename, bottomBmps[i]);
//            }
//            subPath = "Left/";
//            Helpers.clearFolder(path + subPath);
//            for (var i = 0; i < leftBmps.Count(); i++)
//            {
//                var filename = "bmp_" + i.ToString() + ".bmp";
//                BitmapOperations.SaveBitmapToFile(path + subPath + filename, leftBmps[i]);
//            }
//            subPath = "Right/";
//            Helpers.clearFolder(path + subPath);
//            for (var i = 0; i < rightBmps.Count(); i++)
//            {
//                var filename = "bmp_" + i.ToString() + ".bmp";
//                BitmapOperations.SaveBitmapToFile(path + subPath + filename, rightBmps[i]);
//            }
//            subPath = "CurrentOptions/";
//            Helpers.clearFolder(path + subPath);
//            for (var i = 0; i < currentBmps.Count(); i++)
//            {
//                var filename = "bmp_" + i.ToString() + ".bmp";
//                BitmapOperations.SaveBitmapToFile(path + subPath + filename, currentBmps[i]);
//            }
//        }
//    }
//}
