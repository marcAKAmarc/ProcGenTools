using ProcGenTools.DataProcessing;
using ProcGenTools.DataStructures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Configuration;
namespace PuzzleBuilder
{
    public class TilesetConfiguration
    {
        public List<OpinionatedItem<Bitmap>> MainDistinct;

        public List<OpinionatedItem<Bitmap>> HorizontalTraversableTiles;
        public List<OpinionatedItem<Bitmap>> HorizontalTraversablePlainTiles;
        public List<OpinionatedItem<Bitmap>> VerticalTraversableTiles;
        public List<OpinionatedItem<Bitmap>> LadderTiles;
        public List<OpinionatedItem<Bitmap>> VerticalExitTiles;
        public List<OpinionatedItem<Bitmap>> DoorTiles;
        public List<OpinionatedItem<Bitmap>> ButtonTiles;
        public List<OpinionatedItem<Bitmap>> BoxTiles;
        public List<OpinionatedItem<Bitmap>> SolidTiles;
        public List<OpinionatedItem<Bitmap>> EnemyTiles;
        public List<OpinionatedItem<Bitmap>> EmptyTiles;
        public List<OpinionatedItem<Bitmap>> RopeTiles;
        public List<OpinionatedItem<Bitmap>> ConveyorTiles;
        public List<OpinionatedItem<Bitmap>> ElevatorTiles;
        public List<OpinionatedItem<Bitmap>> ShooterTiles;
        public List<OpinionatedItem<Bitmap>> NonDynamic;
        public List<OpinionatedItem<Bitmap>> NonDynamicStrict;
        public List<OpinionatedItem<Bitmap>> Walkable;
        public List<OpinionatedItem<Bitmap>> FallTiles;
        public Bitmap CautionTile;
        public Bitmap ErrorTile;

        public string WFCdebugFolderPath;
        public string TilesetDebugFolderPath;

        public TilesetConfiguration(string main, string horizontal, string horizontalPlain, string vertical, string ladder, string verticalExit, string door, string button, string box, string rope, string conveyor, string elevator, string shooter, string solid, string enemy, string empty, string nondynamic, string nondynamicstrict, string walkable, string fall, string caution, string error, string wfcDebugFolderPath, string tilesetDebugFolderPath)
        {
            MainDistinct = ToOpinionatedList(ChopUpTilesWithMirror(main));

            HorizontalTraversableTiles = GetMatchesInMain(ChopUpTiles(horizontal));
            HorizontalTraversablePlainTiles = GetMatchesInMain(ChopUpTiles(horizontalPlain));
            VerticalTraversableTiles = GetMatchesInMain(ChopUpTiles(vertical));
            LadderTiles = GetMatchesInMain(ChopUpTiles(ladder));
            VerticalExitTiles = GetMatchesInMain(ChopUpTiles(verticalExit));
            DoorTiles = GetMatchesInMain(ChopUpTiles(door));
            ButtonTiles = GetMatchesInMain(ChopUpTiles(button));
            BoxTiles = GetMatchesInMain(ChopUpTiles(box));
            RopeTiles = GetMatchesInMain(ChopUpTiles(rope));
            ConveyorTiles = GetMatchesInMain(ChopUpTiles(conveyor));
            ElevatorTiles = GetMatchesInMain(ChopUpTiles(elevator));
            ShooterTiles = GetMatchesInMain(ChopUpTiles(shooter));
            SolidTiles = GetMatchesInMain(ChopUpTiles(solid));
            EmptyTiles = GetMatchesInMain(ChopUpTiles(empty));
            EnemyTiles = GetMatchesInMain(ChopUpTiles(enemy));
            NonDynamic = GetMatchesInMain(ChopUpTiles(nondynamic));
            NonDynamicStrict = GetMatchesInMain(ChopUpTiles(nondynamicstrict));
            Walkable = GetMatchesInMain(ChopUpTiles(walkable));
            FallTiles = GetMatchesInMain(ChopUpTiles(fall));
            ErrorTile = Image.FromFile(error) as Bitmap;
            CautionTile = Image.FromFile(caution) as Bitmap;
            WFCdebugFolderPath = wfcDebugFolderPath;
            TilesetDebugFolderPath = tilesetDebugFolderPath;

            SetAcceptableItems(MainDistinct, ChopUpTilesWithMirror2D(main));


            printLists(new List<List<OpinionatedItem<Bitmap>>>() {
                MainDistinct,
                HorizontalTraversableTiles,
                HorizontalTraversablePlainTiles,
                VerticalTraversableTiles,
                LadderTiles,
                DoorTiles,
                ButtonTiles,
                RopeTiles,
                ConveyorTiles,
                ElevatorTiles,
                ShooterTiles,
                SolidTiles,
                EmptyTiles,
                NonDynamic,
                NonDynamicStrict,
                Walkable,
                BoxTiles
            }, TilesetDebugFolderPath);
        }

        private List<Bitmap> ChopUpTilesWithMirror(string tilespath)
        {
            List<Bitmap> result = new List<Bitmap>();
            Bitmap tilesetImg = Image.FromFile(tilespath) as Bitmap;
            tilesetImg = tilesetImg.AddHorizontalMirror(true);
            var tiles = BitmapOperations.GetBitmapTiles(tilesetImg, 6, 6, true);
            //bad form
            var path = TilesetDebugFolderPath;
            BitmapOperations.SaveBitmapToFile(path+/*"../../TilesetsDebug/Current/" +*/ "mainTilesetDebug" + ".bmp", BitmapOperations.CreateBitmapFromTiles(tiles, true));
            foreach (var row in tiles)
            {
                result.AddRange(row);
            }
            return result;
        }

        private List<Bitmap> ChopUpTiles(string tilespath)
        {
            Console.WriteLine("ChoppingUpTiles at " + tilespath);
            List<Bitmap> result = new List<Bitmap>();
            Bitmap tilesetImg = Image.FromFile(tilespath) as Bitmap;
            var tiles = BitmapOperations.GetBitmapTiles(tilesetImg, 6, 6, true);
            foreach (var row in tiles)
            {
                result.AddRange(row);
            }
            return result;
        }

        private List<List<Bitmap>> ChopUpTilesWithMirror2D(string tilespath)
        {

            List<Bitmap> result = new List<Bitmap>();
            Bitmap tilesetImg = Image.FromFile(tilespath) as Bitmap;
            tilesetImg = tilesetImg.AddHorizontalMirror(true);
            var tiles = BitmapOperations.GetBitmapTiles(tilesetImg, 6, 6, true);
            return tiles;
        }


        private static List<OpinionatedItem<Bitmap>> ToOpinionatedList(List<System.Drawing.Bitmap> list)
        {
            list = GetDistinctBitmaps(list);
            List<OpinionatedItem<Bitmap>> items = new List<OpinionatedItem<Bitmap>>();
            for (var x = 0; x < list.Count; x++)
            {
                var bmp = list[x];
                items.Add(new OpinionatedItem<Bitmap>(bmp, x.ToString(), new List<WcfVector>().Cross3dShape()));
            }

            return items;
        }

        private List<OpinionatedItem<Bitmap>> GetMatchesInMain(List<Bitmap> items)
        {
            return MainDistinct.Where(element => items.Any(t => BitmapOperations.Compare(t, element.actualItem))).ToList();
        }

        private static List<Bitmap> GetDistinctBitmaps(List<Bitmap> grid)
        {
            List<Bitmap> DistinctTiles = new List<Bitmap>();

            foreach (var tile in grid)
            {
                var Distinct = true;
                foreach (var validatedTile in DistinctTiles)
                {
                    var equal = BitmapOperations.Compare(validatedTile, tile);
                    if (equal)
                    {
                        Distinct = false;
                        break;
                    }
                }
                if (Distinct == true)
                {
                    DistinctTiles.Add(tile);
                }
            }

            return DistinctTiles;
        }

        private static void SetAcceptableItems(List<OpinionatedItem<Bitmap>> Elements, List<List<Bitmap>> Tilemap)
        {
            foreach (var element in Elements)
            {
                for (var x = 0; x < Tilemap.Count; x++)
                {
                    for (var y = 0; y < Tilemap[0].Count; y++)
                    {
                        if (BitmapOperations.Compare(Tilemap[x][y], element.actualItem))
                        {
                            List<Point> neighborpoints = new List<Point>() {
                               // new Point(x - 1, y - 1),
                                new Point(x, y - 1),
                                //new Point(x + 1, y - 1),
                                new Point(x + 1, y),
                                //new Point(x + 1, y + 1),
                                new Point(x, y + 1),
                                //new Point(x - 1, y + 1),
                                new Point(x - 1, y)
                            };
                            neighborpoints = neighborpoints.Where(point =>
                                point.X >= 0 && point.X < Tilemap.Count
                                && point.Y >= 0 && point.Y < Tilemap[0].Count
                            ).ToList();

                            List<Bitmap> neighborBmps = new List<Bitmap>();
                            foreach (var neighborpoint in neighborpoints)
                            {
                                neighborBmps.Add(Tilemap[neighborpoint.X][neighborpoint.Y]);
                            }

                            for (var n = 0; n < neighborBmps.Count; n++)
                            {
                                var neighborBmp = neighborBmps[n];
                                var neighborPoint = neighborpoints[n];
                                var elementPoint = new Point(x, y);
                                var relativePoint = new Point(neighborPoint.X - elementPoint.X, neighborPoint.Y - elementPoint.Y);


                                OpinionatedItem<Bitmap> neighborElement = null;
                                foreach (var elementN in Elements)
                                {
                                    if (BitmapOperations.Compare(elementN.actualItem, neighborBmp))
                                    {
                                        neighborElement = elementN;
                                        break;
                                    }
                                }

                                //if neighbor in element has neighbor at pos, bail (make distinct - makes faster in long run.. no natural weight though
                                /*if (element.GetAcceptableInDirection(relativePoint.X, relativePoint.Y, 0).Any(acc => acc.Id == neighborElement.Id))
                                    continue;*/

                                if (neighborElement == null)
                                {
                                    throw new Exception("Couldn't find neighbor in distinct elements!");
                                }
                                element.SetAcceptableInDirection(neighborElement, relativePoint.X, relativePoint.Y, 0);
                            }
                        }
                    }
                }
            }
        }

        private static void printLists(List<List<OpinionatedItem<Bitmap>>> ElementsList, string debugFolderPath)
        {
            List<Bitmap> Bitmaps = new List<Bitmap>();
            var path = debugFolderPath;
            foreach(var Elements in ElementsList)
            {
                var list = new List<List<Bitmap>>();
                list.Add(new List<Bitmap>());
                list[0].AddRange(Elements.Select(x => x.actualItem).ToList());
                var bmp = BitmapOperations.CreateBitmapFromTiles(list);
                Bitmaps.Add(bmp);
            }

            for(var i = 0; i < Bitmaps.Count; i++)
            {
                BitmapOperations.SaveBitmapToFile(path+"/"+/*"../../TilesetsDebug/Current/" +*/ i.ToString() + ".bmp", Bitmaps[i]);
            }
        }
    }
}

