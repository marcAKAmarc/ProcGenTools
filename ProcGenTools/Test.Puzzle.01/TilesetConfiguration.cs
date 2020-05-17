//using ProcGenTools.DataProcessing;
//using ProcGenTools.DataStructures;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Test.Puzzle._01
//{
//    public class TilesetConfiguration
//    {
//        public List<OpinionatedItem<Bitmap>> MainDistinct;

//        public List<OpinionatedItem<Bitmap>> HorizontalTraversableTiles;
//        public List<OpinionatedItem<Bitmap>> HorizontalTraversablePlainTiles;
//        public List<OpinionatedItem<Bitmap>> VerticalTraversableTiles;
//        public List<OpinionatedItem<Bitmap>> LadderTiles;
//        public List<OpinionatedItem<Bitmap>> DoorTiles;
//        public List<OpinionatedItem<Bitmap>> ButtonTiles;
//        public List<OpinionatedItem<Bitmap>> SolidTiles;
//        public List<OpinionatedItem<Bitmap>> EmptyTiles;
//        public List<OpinionatedItem<Bitmap>> RopeTiles;
//        public List<OpinionatedItem<Bitmap>> ConveyorTiles;
//        public List<OpinionatedItem<Bitmap>> ElevatorTiles;
//        public Bitmap CautionTile;
//        public Bitmap ErrorTile;

//        public TilesetConfiguration(string main, string horizontal, string horizontalPlain, string vertical, string ladder, string door, string button, string rope, string conveyor, string elevator, string solid, string empty, string caution, string error)
//        {
//            MainDistinct = ToOpinionatedList(ChopUpTilesWithMirror(main));

//            HorizontalTraversableTiles = GetMatchesInMain(ChopUpTiles(horizontal));
//            HorizontalTraversablePlainTiles = GetMatchesInMain(ChopUpTiles(horizontalPlain));
//            VerticalTraversableTiles = GetMatchesInMain(ChopUpTiles(vertical));
//            LadderTiles = GetMatchesInMain(ChopUpTiles(ladder));
//            DoorTiles = GetMatchesInMain(ChopUpTiles(door));
//            ButtonTiles = GetMatchesInMain(ChopUpTiles(button));
//            RopeTiles = GetMatchesInMain(ChopUpTiles(rope));
//            ConveyorTiles = GetMatchesInMain(ChopUpTiles(conveyor));
//            ElevatorTiles = GetMatchesInMain(ChopUpTiles(elevator));
//            SolidTiles = GetMatchesInMain(ChopUpTiles(solid));
//            EmptyTiles = GetMatchesInMain(ChopUpTiles(empty));
//            ErrorTile = Image.FromFile(error) as Bitmap;
//            CautionTile = Image.FromFile(caution) as Bitmap;

//            SetAcceptableItems(MainDistinct, ChopUpTilesWithMirror2D(main));
//        }

//        private List<Bitmap> ChopUpTilesWithMirror(string tilespath)
//        {
//            List<Bitmap> result = new List<Bitmap>();
//            Bitmap tilesetImg = Image.FromFile(tilespath) as Bitmap;
//            tilesetImg = tilesetImg.AddHorizontalMirror(true);
//            var tiles = BitmapOperations.GetBitmapTiles(tilesetImg, 6, 6, true);
//            foreach (var row in tiles)
//            {
//                result.AddRange(row);
//            }
//            return result;
//        }

//        private List<Bitmap> ChopUpTiles(string tilespath)
//        {
//            List<Bitmap> result = new List<Bitmap>();
//            Bitmap tilesetImg = Image.FromFile(tilespath) as Bitmap;
//            var tiles = BitmapOperations.GetBitmapTiles(tilesetImg, 6, 6, true);
//            foreach (var row in tiles)
//            {
//                result.AddRange(row);
//            }
//            return result;
//        }

//        private List<List<Bitmap>> ChopUpTilesWithMirror2D(string tilespath)
//        {
            
//            List<Bitmap> result = new List<Bitmap>();
//            Bitmap tilesetImg = Image.FromFile(tilespath) as Bitmap;
//            tilesetImg = tilesetImg.AddHorizontalMirror(true);
//            var tiles = BitmapOperations.GetBitmapTiles(tilesetImg, 6, 6, true);
//            return tiles;
//        }
    

//        private static List<OpinionatedItem<Bitmap>> ToOpinionatedList(List<System.Drawing.Bitmap> list)
//        {
//            list = GetDistinctBitmaps(list);
//            List<OpinionatedItem<Bitmap>> items = new List<OpinionatedItem<Bitmap>>();
//            for (var x = 0; x < list.Count; x++)
//            {
//                var bmp = list[x];
//                items.Add(new OpinionatedItem<Bitmap>(bmp, x.ToString(), new List<WcfVector>().Cross3dShape()));
//            }

//            return items;
//        }

//        private List<OpinionatedItem<Bitmap>> GetMatchesInMain(List<Bitmap> items)
//        {
//            return MainDistinct.Where(element => items.Any(t => BitmapOperations.Compare(t, element.actualItem))).ToList();
//        }

//        private static List<Bitmap> GetDistinctBitmaps(List<Bitmap> grid)
//        {
//            List<Bitmap> DistinctTiles = new List<Bitmap>();

//            foreach (var tile in grid)
//            {
//                var Distinct = true;
//                foreach (var validatedTile in DistinctTiles)
//                {
//                    var equal = BitmapOperations.Compare(validatedTile, tile);
//                    if (equal)
//                    {
//                        Distinct = false;
//                        break;
//                    }
//                }
//                if (Distinct == true)
//                {
//                    DistinctTiles.Add(tile);
//                }
//            }
            
//            return DistinctTiles;
//        }

//        private static void SetAcceptableItems(List<OpinionatedItem<Bitmap>> Elements, List<List<Bitmap>> Tilemap)
//        {
//            foreach (var element in Elements)
//            {
//                for (var x = 0; x < Tilemap.Count; x++)
//                {
//                    for (var y = 0; y < Tilemap[0].Count; y++)
//                    {
//                        if (BitmapOperations.Compare(Tilemap[x][y], element.actualItem))
//                        {
//                            List<Point> neighborpoints = new List<Point>() {
//                               // new Point(x - 1, y - 1),
//                                new Point(x, y - 1),
//                                //new Point(x + 1, y - 1),
//                                new Point(x + 1, y),
//                                //new Point(x + 1, y + 1),
//                                new Point(x, y + 1),
//                                //new Point(x - 1, y + 1),
//                                new Point(x - 1, y)
//                            };
//                            neighborpoints = neighborpoints.Where(point =>
//                                point.X >= 0 && point.X < Tilemap.Count
//                                && point.Y >= 0 && point.Y < Tilemap[0].Count
//                            ).ToList();

//                            List<Bitmap> neighborBmps = new List<Bitmap>();
//                            foreach (var neighborpoint in neighborpoints)
//                            {
//                                neighborBmps.Add(Tilemap[neighborpoint.X][neighborpoint.Y]);
//                            }

//                            for (var n = 0; n < neighborBmps.Count; n++)
//                            {
//                                var neighborBmp = neighborBmps[n];
//                                var neighborPoint = neighborpoints[n];
//                                var elementPoint = new Point(x, y);
//                                var relativePoint = new Point(neighborPoint.X - elementPoint.X, neighborPoint.Y - elementPoint.Y);


//                                OpinionatedItem<Bitmap> neighborElement = null;
//                                foreach (var elementN in Elements)
//                                {
//                                    if (BitmapOperations.Compare(elementN.actualItem, neighborBmp))
//                                    {
//                                        neighborElement = elementN;
//                                        break;
//                                    }
//                                }

//                                //if neighbor in element has neighbor at pos, bail (make distinct - makes faster in long run.. no natural weight though
//                                /*if (element.GetAcceptableInDirection(relativePoint.X, relativePoint.Y, 0).Any(acc => acc.Id == neighborElement.Id))
//                                    continue;*/

//                                if (neighborElement == null)
//                                {
//                                    throw new Exception("Couldn't find neighbor in distinct elements!");
//                                }
//                                element.SetAcceptableInDirection(neighborElement, relativePoint.X, relativePoint.Y, 0);
//                            }
//                        }
//                    }
//                }
//            }
//        }
//    }
//}

