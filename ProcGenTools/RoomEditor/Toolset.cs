﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ProcGenTools.DataProcessing;
using ProcGenTools.DataStructures;
using RoomEditor.Models;
using System.Configuration;
/// <summary>
/// THIS HAS BEEN MIGRATED TO THE ROOM EDITOR PROJECT
/// </summary>
namespace RoomEditor
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
        private List<OpinionatedItem<Bitmap>> _hEntranceTiles;
        private List<OpinionatedItem<Bitmap>> _vEntranceTiles;
        private List<OpinionatedItem<Bitmap>> _borderTiles;
        private List<OpinionatedItem<Bitmap>> _cornerIntersectionTiles;
        private WcfGrid _grid;
        private Random _random;

        public LevelEditor(int tileWidth, int tileHeight, int levelWidth, int levelHeight, string errorTilePath, string cautionTilePath)
        {
            _tileHeight = tileHeight;
            _tileWidth = tileWidth;
            _levelHeight = levelHeight;
            _levelWidth = levelWidth;

            _errorTile = Image.FromFile(errorTilePath) as Bitmap;
            _cautionTile = Image.FromFile(cautionTilePath) as Bitmap;
        }

        public void SetDimensions(int levelWidth, int levelHeight)
        {
            _levelHeight = levelHeight;
            _levelWidth = levelWidth;
        }

        public void LoadTileset(string path, bool horizontalMirror = true)
        {
            //Chop up tileset
            Bitmap tilesetImg = Image.FromFile(path) as Bitmap;
            if (horizontalMirror)
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

        public void SetupHEntranceTiles(string path)
        {
            _hEntranceTiles = getWcfTilesInBmp(path);
        }
        public void SetupVEntranceTiles(string path)
        {
            _vEntranceTiles = getWcfTilesInBmp(path);
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
            if (seed != null)
            {
                _random = new Random(seed.Value);
            }
            else
            {
                _random = null;
            }
            if (_random == null)
                _grid = new WcfGrid();
            else
                _grid = new WcfGrid(_random);
            _grid.Init(_levelWidth, _levelHeight, 1, _wfcTiles);
            var shape = new List<WcfVector>().Cross3dShape();
            _grid.SetInfluenceShape(shape);
        }

        public bool CollapseWcf()
        {
            return _grid.CollapseAllRecursive();
        }
        public bool OutputWfc(string outPath)
        {
            var collapsedTiles = ToTilesList(_grid);

            Bitmap tilesetRedux = BitmapOperations.CreateBitmapFromTiles(collapsedTiles);
            BitmapOperations.SaveBitmapToFile(outPath, tilesetRedux);

            return true;
        }
        public Bitmap GetBitmap()
        {
            var collapsedTiles = ToTilesList(_grid);
            return BitmapOperations.CreateBitmapFromTiles(collapsedTiles);
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

        public bool AddBorder(Room room)
        {
            //NOT AFFECTED BY BORDER WIDTH!
            bool result = true;
            for (var x = 0; x < _grid.Width; x++)
            {
                for (var y = 0; y < _grid.Height; y++)
                {
                    if(x==13 && y == 0)
                    {
                        //bad form... just temporary
                        BitmapOperations.SaveBitmapToFile(ConfigurationManager.AppSettings["BitmapOutput"], GetBitmap());
                    }
                    if (!(x == 0 || y == 0 || x == _grid.Width - 1 || y == _grid.Height - 1))
                        continue;
                    var thisPoint = new Point(x, y);

                    //should technically be moved to add path
                    if (room.portals.Any(p => p.point == thisPoint)) {
                        var portals = room.portals.Where(p => p.point == thisPoint);
                        var directions = portals.Select(p => p.direction).Distinct();
                        if (directions.Where(d => d.X == 0).Count() == 0)
                        {
                            //horizontal
                            _grid.SuperPositions[x,y,0].CollapseToItems(_hEntranceTiles.Select(i => i.Id).ToList(), true);
                            result = result && _grid.handlePropagation(_grid.SuperPositions[x, y, 0]);
                        }
                        else if (directions.Where(d => d.Y == 0).Count() == 0)
                        {
                            //vertical
                            _grid.SuperPositions[x, y, 0].CollapseToItems(_vEntranceTiles.Select(i => i.Id).ToList(), true);
                            result = result && _grid.handlePropagation(_grid.SuperPositions[x, y, 0]);
                        }
                        else
                        {
                            //intersectional
                            _grid.SuperPositions[x, y, 0].CollapseToItems(_cornerIntersectionTiles.Select(i => i.Id).ToList(), true);
                            result = result && _grid.handlePropagation(_grid.SuperPositions[x, y, 0]);
                            
                        }
                        continue;
                    }
                    if (x == 13 && y == 0)
                    {
                        //bad form... just temporary
                        BitmapOperations.SaveBitmapToFile(ConfigurationManager.AppSettings["BitmapOutput"], GetBitmap());
                    }

                    _grid.SuperPositions[x, y, 0].CollapseToItems(_borderTiles.Select(i => i.Id).ToList(), true);
                    if (x == 13 && y == 0)
                    {
                        //bad form... just temporary
                        BitmapOperations.SaveBitmapToFile(ConfigurationManager.AppSettings["BitmapOutput"], GetBitmap());
                    }
                    result = _grid.handlePropagation(_grid.SuperPositions[x, y, 0]);
                    if (!result)
                        return false;
                }
            }
            return true;
        }

        public bool AddPortal(Portal portal)
        {
            List<OpinionatedItem<Bitmap>> tiles = _hEntranceTiles;
            if (portal.direction.Y != 0)
                tiles = _vEntranceTiles;

            _grid.SuperPositions[portal.point.X, portal.point.Y, 0].CollapseToItems(tiles.Select(i => i.Id).ToList(), true);
            return _grid.handlePropagation(_grid.SuperPositions[portal.point.X, portal.point.Y, 0]);
        }

        public bool AddPaths(Room room)
        {
            Console.WriteLine("Room Paths:");
            bool result = true;
            for(var i = 0; i < room.portals.Count; i++)
            {
                //dont do the last one if there is more than one total
                if (i > 0 && room.portals.Count - 1 == i)
                    break;
                //foreach (var entrance in room.portals.Where(x => x.IsEntrance(room)||true)) //oof
                //{

                //this is what is messing up!  it is relying on entrance and exit and diretion, which is not good for bidirection...
                //for now - each portal simply connects to the next.
                //shift points into box of borderwidth
                var entrance = room.portals[i];
                var exits = new List<Portal>();
                if (room.portals.Count > 1 && i < room.portals.Count -1)
                    exits.Add(room.portals[i + 1]);

                Point entrancePoint, entranceDirection, exitPoint, exitDirection;
                entrancePoint = entrance.point;
                if (entrance.point.X < room.DistanceBetweenPathAndEdge)
                {
                    entrancePoint.X = room.DistanceBetweenPathAndEdge;
                }
                if (entrance.point.X > (room.Width - 1) - (room.DistanceBetweenPathAndEdge))
                {
                    entrancePoint.X = (room.Width - 1) - (room.DistanceBetweenPathAndEdge);
                }

                if (entrance.point.Y < room.DistanceBetweenPathAndEdge)
                {
                    entrancePoint.Y = room.DistanceBetweenPathAndEdge;
                }
                if (entrance.point.Y > (room.Height - 1) - (room.DistanceBetweenPathAndEdge))
                {
                    entrancePoint.Y = (room.Height - 1) - (room.DistanceBetweenPathAndEdge);
                }

                //in portals, direction is always direction out... so we flip entrance direction
                //but keep exit direction as is
                entranceDirection = new Point(entrance.direction.X * -1, entrance.direction.Y * -1);
                //var exits = room.portals.Where(x => !x.IsEntrance(room)).ToList();
                if (exits.Count == 0)
                {
                    exitPoint = entrancePoint;
                    exitDirection = new Point();
                }
                else
                {
                    exitPoint = exits.First().point;
                    exitDirection = exits.First().direction;
                }

                if (exitPoint.X < room.DistanceBetweenPathAndEdge)
                {
                    exitPoint.X = room.DistanceBetweenPathAndEdge;
                }
                if (exitPoint.X > (room.Width - 1) - (room.DistanceBetweenPathAndEdge))
                {
                    exitPoint.X = (room.Width - 1) - (room.DistanceBetweenPathAndEdge);
                }

                if (exitPoint.Y < room.DistanceBetweenPathAndEdge)
                {
                    exitPoint.Y = room.DistanceBetweenPathAndEdge;
                }
                if (exitPoint.Y > (room.Height - 1) - (room.DistanceBetweenPathAndEdge))
                {
                    exitPoint.Y = (room.Height - 1) - (room.DistanceBetweenPathAndEdge);
                }
                //shift to 0
                exitPoint.X = exitPoint.X - room.DistanceBetweenPathAndEdge;
                exitPoint.Y = exitPoint.Y - room.DistanceBetweenPathAndEdge;
                entrancePoint.X -= room.DistanceBetweenPathAndEdge;
                entrancePoint.Y -= room.DistanceBetweenPathAndEdge;

                result = result && AddPath(entrancePoint, entranceDirection, exitPoint, exitDirection, room.Width - (room.DistanceBetweenPathAndEdge * 2), room.Height - (room.DistanceBetweenPathAndEdge * 2), room.DistanceBetweenPathAndEdge);
                if(result == false)
                {
                    return false;
                }
            }

            return true;
        }
        private bool AddPath(Point start, Point startDirection, Point end, Point endDirection, int pathWidth, int pathHeight, int borderPadding)
        {
            bool result;
            Path path = new Path(
                new PathPoint(new Point(start.X, start.Y), startDirection),
                new PathPoint(new Point(end.X, end.Y), endDirection),
                pathWidth, pathHeight,
                null, _random
            );
            path.printToConsole();
            for (var i = 0; i < path._pathPoints.Count; i++)
            {
                Point? previousDir = new Point(1, 0);
                if (i == 0)
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
                else if (path._pathPoints[i].direction == null || path._pathPoints[i].direction.Y == 0)
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
                result = _grid.handlePropagation(
                    _grid.SuperPositions[
                        path._pathPoints[i].point.X + borderPadding,
                        path._pathPoints[i].point.Y + borderPadding,
                        0
                    ]
                );
                if (result == false)
                {
                    return result;
                }
            }
            return true;
        }
    }
}
