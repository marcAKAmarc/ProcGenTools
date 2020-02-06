using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ProcGenTools.DataProcessing;
using ProcGenTools.DataStructures;

namespace Test.LevelGeneration
{
    public class LevelEditor
    {
        private int _tileWidth;
        private int _tileHeight;
        private int _levelWidth;
        private int _levelHeight;
        private List<List<Bitmap>> _tileset;
        private Bitmap _errorTile;
        private Bitmap _cautionTile;
        private List<OpinionatedItem<Bitmap>> _wfcTiles;
        private List<OpinionatedItem<Bitmap>> _vTraverseWfcTiles;
        private List<OpinionatedItem<Bitmap>> _hTraverseWcfTiles;
        private List<OpinionatedItem<Bitmap>> _entranceTiles;
        private List<OpinionatedItem<Bitmap>> _borderTiles;
        private List<OpinionatedItem<Bitmap>> _cornerIntersectionTiles;
        private WcfGrid _grid;
        private Random _random;

        public LevelEditor(int tileWidth, int tileHeight, int levelWidth, int levelHeight, string errorTilePath, string cautionTilePath )
        {
            _tileHeight = tileHeight;
            _tileWidth = tileWidth;
            _levelHeight = levelHeight;
            _levelWidth = levelWidth;

            _errorTile = Image.FromFile(errorTilePath) as Bitmap;
            _cautionTile = Image.FromFile(cautionTilePath) as Bitmap;
        }

        public void LoadTileset(string path, bool horizontalMirror = true)
        {
            //Chop up tileset
            Bitmap tilesetImg = Image.FromFile(path) as Bitmap;
            if(horizontalMirror)
                tilesetImg = tilesetImg.AddHorizontalMirror();
            _tileset = BitmapOperations.GetBitmapTiles(tilesetImg, _tileWidth, _tileHeight);


            //setup wfcTiles (and neighbors)
            var distinctTiles = GetDistinctBitmaps(_tileset);
            //List<List<Bitmap>> distinctElements = new List<List<Bitmap>>();
            //distinctElements.Add(distinctTiles);
            _wfcTiles = ToOpinionatedList(distinctTiles);
            SetAcceptableItems(_wfcTiles, _tileset);
        }

        public void SetupTraversableTiles(string hPath, string vPath)
        {
            _vTraverseWfcTiles = getWcfTilesInBmp(vPath);
            _hTraverseWcfTiles = getWcfTilesInBmp(hPath);
        }

        public void SetupDoorTiles(string path)
        {
            _entranceTiles = getWcfTilesInBmp(path);
        }
        public void SetupBorderTiles(string path)
        {
            _borderTiles = getWcfTilesInBmp(path);
        }
        public void SetupCornersAndIntersectionTiles(string path)
        {
            _cornerIntersectionTiles = getWcfTilesInBmp(path);
        }
        private List<OpinionatedItem<Bitmap>> getWcfTilesInBmp(string path)
        {
            Bitmap bmp = Image.FromFile(path) as Bitmap;
            List<List<Bitmap>> tileset = BitmapOperations.GetBitmapTiles(bmp, _tileWidth, _tileHeight);
            List<Bitmap> tiles = new List<Bitmap>();
            foreach (var list in tileset)
            {
                tiles.AddRange(list);
            }
            return _wfcTiles.Where(element => tiles.Any(t => BitmapOperations.Compare(t, element.actualItem))).ToList();
        }

        public void InitWcfGrid(int? seed = null)
        {
            if(seed != null)
            {
                _random = new Random(seed.Value);
            }
            else
            {
                _random = null;
            }
            if (_random==null)
                _grid = new WcfGrid();
            else
                _grid = new WcfGrid(_random);
            _grid.Init(_levelWidth, _levelHeight, 1, _wfcTiles);
            var shape = new List<WcfVector>().Cross3dShape();
            _grid.SetInfluenceShape(shape);
        }
        public bool ManualChanges(int entranceY, int exitY)
        {
            bool result = true;
            if(_entranceTiles.Count != 0)
            {
                result = result && AddDoor(entranceY, exitY);
            }
            if(_borderTiles.Count != 0)
            {
                result = result && AddBorder(entranceY, exitY);
            }
            if(_hTraverseWcfTiles.Count != 0 && _vTraverseWfcTiles.Count != 0)
            {
                result = result && AddPath(entranceY, exitY,2);
            }
            return result;
        }
        public bool CollapseWcf()
        {
            return _grid.CollapseAllRecursive();
        }
        public bool OutputWfc(string outPath)
        {
            var collapsedTiles = ToTilesList(_grid);

            Bitmap tilesetRedux = BitmapOperations.CreateBitmapFromTiles(collapsedTiles);
            BitmapOperations.SaveBitmapToFile(outPath,tilesetRedux);

            return true;
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
            foreach (var verticalRow in grid)
            {
                foreach (var tile in verticalRow)
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
            }
            return DistinctTiles;
        }

        private static List<OpinionatedItem<T>> ToOpinionatedList<T>(List<T> list)
        {
            List<OpinionatedItem<T>> items = new List<OpinionatedItem<T>>();
            for (var x = 0; x < list.Count; x++)
            {
                var bmp = list[x];
                items.Add(new OpinionatedItem<T>(bmp, x.ToString(), new List<WcfVector>().Cross3dShape()));
            }

            return items;
        }

        private List<List<Bitmap>> ToTilesList(WcfGrid grid)
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

                    Tiles.Last().Add(grid.SuperPositions[x, y, 0].FinalPayloadIfExists<Bitmap>(_cautionTile, _errorTile));
                }
            }
            return Tiles;
        }


        private bool AddBorder(int entranceY, int exitY)
        {
            bool result;
            for (var x = 0; x < _grid.Width; x++)
            {
                for (var y = 0; y < _grid.Height; y++)
                {
                    if (x > 0 && x < _grid.Width - 1 && y > 0 && y < _grid.Height - 1)
                        continue;
                    if ((y == entranceY && x == 0) || (y == exitY && x == _grid.Width - 1))
                        continue;
                    _grid.SuperPositions[x, y, 0].CollapseToItems(_borderTiles.Select(i=>i.Id).ToList(), true);
                    result = _grid.handlePropagation(_grid.SuperPositions[x, y, 0]);
                    if (!result)
                        return false;
                }
            }
            return true;
        }
        private bool AddDoor(int entranceY, int exitY)
        {
            //grid.SuperPositions[0, 5, 0].Uncollapse();
            //grid.SuperPositions[grid.Width-1, 5, 0].Uncollapse();
            _grid.SuperPositions[0, entranceY, 0].CollapseToItems(_entranceTiles.Select(i=>i.Id).ToList(), true);
            var result1 = _grid.handlePropagation(_grid.SuperPositions[0, entranceY, 0]);
            _grid.SuperPositions[_grid.Width - 1, exitY, 0].CollapseToItems(_entranceTiles.Select(i=>i.Id).ToList(), true);
            var result2 = _grid.handlePropagation(_grid.SuperPositions[_grid.Width - 1, exitY, 0]);
            return result1 && result2;
        }

        private bool AddPath(int entranceY, int exitY, int borderPadding)
        {
            bool result;
            Path path = new Path(
                new Point(0, entranceY-borderPadding), 
                new Point(_levelWidth-(1 + (borderPadding*2)), exitY-borderPadding), 
                _levelWidth-(borderPadding*2), _levelHeight - borderPadding, null, _random);
            path.printToConsole();
            for(var i = 0; i < path._pathPoints.Count; i++)
            {
                Point? previousDir = new Point(1, 0);
                if(i == 0)
                {
                    previousDir = new Point(1, 0);
                }
                else
                {
                    previousDir = path._pathPoints[i - 1].direction;
                }

                if (previousDir != path._pathPoints[i].direction)
                {
                    //corners and intersections
                    _grid.SuperPositions[
                        path._pathPoints[i].point.X + borderPadding, 
                        path._pathPoints[i].point.Y + borderPadding, 
                        0
                    ].CollapseToItems(_cornerIntersectionTiles.Select(tile => tile.Id).ToList(), true); 
                }
                else if (path._pathPoints[i].direction == null || path._pathPoints[i].direction.Value.Y == 0)
                {
                    //horizontal
                    _grid.SuperPositions[
                        path._pathPoints[i].point.X + borderPadding, 
                        path._pathPoints[i].point.Y + borderPadding, 
                        0
                    ].CollapseToItems(_hTraverseWcfTiles.Select(tile => tile.Id).ToList(), true);
                }
                else
                {
                    //vertical
                    _grid.SuperPositions[
                        path._pathPoints[i].point.X + borderPadding, 
                        path._pathPoints[i].point.Y + borderPadding, 
                        0
                    ].CollapseToItems(_vTraverseWfcTiles.Select(tile => tile.Id).ToList(), true);
                }
                result =_grid.handlePropagation(
                    _grid.SuperPositions[
                        path._pathPoints[i].point.X + borderPadding, 
                        path._pathPoints[i].point.Y + borderPadding, 
                        0
                    ]
                );
                if(result == false)
                {
                    return result;
                }
            }
            return true;
            //for (var x = 2; x < _grid.Width - 2; x++)
            //{
            //    _grid.SuperPositions[x, doorwayY, 0].CollapseToItems(_hTraverseWfcTiles.Select(i => i.Id).ToList(), true);
            //    _grid.handlePropagation(_grid.SuperPositions[x, doorwayY, 0]);
            //}
        }

    }
}
