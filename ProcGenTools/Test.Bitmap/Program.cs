using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Configuration;
using ProcGenTools.DataProcessing;
using ProcGenTools.DataStructures;
namespace Test.BitmapSplit
{
    class Program
    {
        public delegate void UpdateFrontEnd(WcfGrid grid);

        static void Main(string[] args)
        {
            Execute();
        }

        private static void Execute()
        {
            Bitmap tileset = Image.FromFile(ConfigurationManager.AppSettings["TilesetInput"]) as Bitmap;
            tileset = tileset.AddHorizontalMirror();
            List<List<Bitmap>> tiles = BitmapOperations.GetBitmapTiles(tileset, 5, 5);

            Bitmap hTraverseBmp = Image.FromFile(ConfigurationManager.AppSettings["hTraverseInput"]) as Bitmap;
            List<List<Bitmap>> hTraverseDbl = BitmapOperations.GetBitmapTiles(hTraverseBmp, 5, 5);
            Bitmap quickcheck = BitmapOperations.CreateBitmapFromTiles(hTraverseDbl, true);
            BitmapOperations.SaveBitmapToFile(ConfigurationManager.AppSettings["TilesetOutput"], quickcheck);
            List<Bitmap> hTraverse = new List<Bitmap>();
            foreach(var list in hTraverseDbl)
            {
                hTraverse.AddRange(list);
            }

            Bitmap vTraverseBmp = Image.FromFile(ConfigurationManager.AppSettings["vTraverseInput"]) as Bitmap;
            List<List<Bitmap>> vTraverseDbl = BitmapOperations.GetBitmapTiles(vTraverseBmp, 5, 5);
            List<Bitmap> vTraverse = new List<Bitmap>();
            foreach (var list in vTraverseDbl)
            {
                vTraverse.AddRange(list);
            }

            var distinctTiles = GetDistinctBitmaps(tiles);

            List<List<Bitmap>> distinctElements = new List<List<Bitmap>>();
            distinctElements.Add(distinctTiles);
            Bitmap tilemapWithSpacing = BitmapOperations.CreateBitmapFromTiles(distinctElements, true);
            BitmapOperations.SaveBitmapToFile(ConfigurationManager.AppSettings["TilesetOutput"], tilemapWithSpacing);


            var opinionatedItemsDistinct = ToOpinionatedList(distinctTiles);
            SetAcceptableItems(opinionatedItemsDistinct, tiles);

            var vTraverseOpinionated = opinionatedItemsDistinct.Where(element => vTraverse.Any(bmp => BitmapOperations.Compare(bmp, element.actualItem))).ToList();
            var hTraverseOpinionated = opinionatedItemsDistinct.Where(element => hTraverse.Any(bmp => BitmapOperations.Compare(bmp, element.actualItem))).ToList();


            for (var i = 0; i < 5; i++)
            {
                var wcf = new WcfGrid();
                wcf.Init(20, 10, 1, opinionatedItemsDistinct);
                List<WcfVector> shape = WcfVector.GetCross3dShape();
                wcf.SetInfluenceShape(shape);


                //Manual Edits
                //in and out
                AddDoor(wcf, opinionatedItemsDistinct[1], 5);
                //border
                AddBorder(wcf, opinionatedItemsDistinct[0], 5);
                //add path
                AddPath(wcf, hTraverseOpinionated, 5);


                var result = wcf.CollapseAllRecursive();
                //if (result == false)
                    //continue;

                var collapsedTiles = ToTilesList(wcf);
                wcf.PrintStatesToConsole2d();

                Bitmap tilesetRedux = BitmapOperations.CreateBitmapFromTiles(collapsedTiles);
                BitmapOperations.SaveBitmapToFile(
                    ConfigurationManager.AppSettings["BitmapOutput"].Replace(
                        ".bmp",
                        i.ToString() + ".bmp"
                    ),
                tilesetRedux);
            }
        }

        private static void AddBorder(WcfGrid grid, IOpinionatedItem borderElement, int doorwayY)
        {
            for(var x = 0; x < grid.Width; x++)
            {
                for(var y = 0; y < grid.Height; y++)
                {
                    if (x > 0 && x < grid.Width - 1 && y > 0 && y < grid.Height - 1)
                        continue;
                    if (y == doorwayY)
                        continue;
                    grid.SuperPositions[x, y, 0].CollapseToItem(borderElement.Id);
                    var result = grid.handlePropagation(grid.SuperPositions[x, y, 0]);
                }
            }
        }
        private static void AddDoor(WcfGrid grid, IOpinionatedItem doorItem, int doorwayY)
        {
            //grid.SuperPositions[0, 5, 0].Uncollapse();
            //grid.SuperPositions[grid.Width-1, 5, 0].Uncollapse();
            grid.SuperPositions[0, doorwayY, 0].CollapseToItem(doorItem.Id);
            grid.handlePropagation(grid.SuperPositions[0, doorwayY, 0]);
            grid.SuperPositions[grid.Width - 1, doorwayY, 0].CollapseToItem(doorItem.Id);
            grid.handlePropagation(grid.SuperPositions[grid.Width - 1, doorwayY, 0]);
        }

        private static void AddPath( WcfGrid grid, List<OpinionatedItem<Bitmap>> hTraverse, int doorwayY)
        {
            for(var x = 2; x < grid.Width - 2; x++)
            {
                grid.SuperPositions[x, doorwayY, 0].CollapseToItems(hTraverse.Select(i => i.Id).ToList(), true);
                grid.handlePropagation(grid.SuperPositions[x, doorwayY, 0]);
            }
        }

        private static void SetAcceptableItems(List<OpinionatedItem<Bitmap>> Elements, List<List<Bitmap>> Tilemap)
        {
            foreach(var element in Elements)
            {
                for(var x = 0; x < Tilemap.Count; x++)
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

                                //if neighbor in element has neighbor at pos, bail
                                OpinionatedItem<Bitmap> neighborElement = null;
                                foreach (var elementN in Elements)
                                {
                                    if (BitmapOperations.Compare(elementN.actualItem, neighborBmp))
                                    {
                                        neighborElement = elementN;
                                        break;
                                    }
                                }

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

        private static List<Bitmap> GetDistinctBitmaps(List<List<Bitmap>> grid)
        {
            List<Bitmap> DistinctTiles = new List<Bitmap>();
            foreach(var verticalRow in grid)
            {
                foreach(var tile in verticalRow)
                {
                    var Distinct = true;
                    foreach(var validatedTile in DistinctTiles)
                    {
                        var equal = BitmapOperations.Compare(validatedTile, tile);
                        if (equal)
                        {
                            Distinct = false;
                            break;
                        }
                    }
                    if(Distinct == true)
                    {
                        DistinctTiles.Add(tile);
                    }
                }
            }
            return DistinctTiles;
        }

        private static List<OpinionatedItem<T>> ToOpinionatedList<T>(List<T> list)
        {
            List<OpinionatedItem<T>> items = new List<OpinionatedItem<T>>();
            for(var x = 0; x < list.Count; x++)
            {
                var bmp = list[x];
                items.Add(new OpinionatedItem<T>(bmp, x.ToString(), WcfVector.GetCross3dShape()));
            }

            return items;
        }

        private static List<List<Bitmap>> ToTilesList(WcfGrid grid)
        {
            var Tiles = new List<List<Bitmap>>();
            for(var x = 0; x < grid.Width; x++)
            {
                for(var y = 0; y < grid.Height; y++)
                {
                    if (y == 0)
                    {
                        Tiles.Add(new List<Bitmap>());
                    }

                    Tiles.Last().Add(grid.SuperPositions[x, y, 0].FirstRemainingPayload<Bitmap>());
                }
            }
            return Tiles;
        }
    }
}
