using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ProcGenTools.DataProcessing;
namespace ProcGenTools.DataStructures
{
    public class Zone
    {
        public Guid id;
        public int Level;
        public static int totalLevels = 0;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int AbsoluteX;
        public int AbsoluteY;
        public HierarchicalMap SubMap;
        public HierarchicalMap FromMap;
        public List<ZonePortal> ZonePortals;
        public Zone()
        {
            id = Guid.NewGuid();
            ZonePortals = new List<ZonePortal>();
        }

        public void CreateSubMap(Random random)
        {


            if (totalLevels < Level + 1)
                totalLevels = Level + 1;
            var scale = 1;
            if (HierarchicalMap.RelativeScales.Count() - 1 >= Level)
                scale = HierarchicalMap.RelativeScales[Level];
            SubMap = new HierarchicalMap(Width * scale, Height * scale, random, Level + 1);
            SubMap.fromZone = this;

            if (HierarchicalMap.RelativeScales.Count() - 1 < Level)
                return;

            foreach (var portal in ZonePortals)
            {
                var newX = 0;
                var newY = 0;
                if (portal.ZoneRelativePosition.X + 1 == Width)
                    newX = (Width * scale) - 1;
                else if (portal.ZoneRelativePosition.X == 0)
                    newX = 0;
                else
                    newX = portal.ZoneRelativePosition.X * scale;
                if (portal.ZoneRelativePosition.Y + 1 == Height)
                    newY = (Height * scale) - 1;
                else if (portal.ZoneRelativePosition.Y == 0)
                    newY = 0;
                else
                    newY = portal.ZoneRelativePosition.Y * scale;

                SubMap.AddPortal(new HierarchicalMapPortal() {
                    direction = new Point(1, 0),
                    point = new Point(
                       newX,
                       newY
                    )
                });
            }
            SubMap.CreatePaths();
            SubMap.CoverPathsWithZones(3, 2);

            SubMap.CreateSubMaps();
        }
    }
    public class ZonePortal
    {
        public Point ZoneRelativePosition;
        public Point MapRelativePosition;
        public Zone Destination;
        public ZonePortalDirection direction;
    }
    public enum ZonePortalDirection { In, Out, Bidirectional};
    public class HierarchicalMapPortal
    {
        public Guid id;
        public Point point;
        public Point direction;

        public HierarchicalMapPortal()
        {
            id = Guid.NewGuid();
        }
    }

    public class HierarchicalMap
    {
        public int _Level = 0;
        public static int _TotalLevels = 0;
        public int _MapWidth;
        public int _MapHeight;
        public int _AbsWidth;
        public int _AbsHeight;
        public double _MapHypotenuse;
        public Zone[,] _Grid;
        public List<Zone> flatZones;
        public List<HierarchicalMapPortal> _Portals;
        public List<Path> _Paths;
        public Random _Random;
        public Zone fromZone = null;
        public static int[] RelativeScales;

        private ConsoleColor dbColorConsole;
        private Color dbColor;

        private static int consoleColorIndex = 0;


        public static int ScaleAtLevel(int level)
        {
            var scaleAtLevel = 1;
            for (var i = level; i < RelativeScales.Count(); i++)
            {
                scaleAtLevel = scaleAtLevel * RelativeScales[i];
            }
            return scaleAtLevel;
        }

        public HierarchicalMap(int width, int height, Random random, int Level = 0)
        {
            _MapHeight = height;
            _MapWidth = width;
            
            _Random = random;

            _Grid = new Zone[width, height];
            flatZones = new List<Zone>();
            _MapHypotenuse = Math.Sqrt((Math.Pow(_MapWidth, 2) + Math.Pow(_MapHeight, 2)));
            _Portals = new List<HierarchicalMapPortal>();
            _Paths = new List<Path>();
            _Level = Level;
            Array values = Enum.GetValues(typeof(ConsoleColor));
            dbColorConsole = (ConsoleColor)values.GetValue(_Random.Next(values.Length));
            //Console.WriteLine(dbColor.ToString());
            //Console.ReadKey();

            dbColorConsole = (Enum.GetValues(typeof(ConsoleColor)) as ConsoleColor[])[consoleColorIndex];
            consoleColorIndex += 1;
            consoleColorIndex = consoleColorIndex % Enum.GetValues(typeof(ConsoleColor)).Length;

            dbColor = Color.FromArgb(_Random.Next(255), _Random.Next(255), _Random.Next(255));
        }

        public List<HierarchicalMap>[,] GetMasterMap(List<HierarchicalMap>[,] _masterMap = null, int depth = 0)
        {
            if(depth == 3)
            {
                var breaka = "here";
            }
            var scaleAtLevel = HierarchicalMap.ScaleAtLevel(_Level);

            if (_masterMap == null)
                _masterMap = new List<HierarchicalMap>[scaleAtLevel*_MapWidth, scaleAtLevel * _MapHeight];

            var myWidth = scaleAtLevel * _MapWidth;
            var myHeight = scaleAtLevel * _MapHeight;


            var currentZone = this.fromZone;
            int absX = 0;
            int absY = 0;
            while (currentZone != null)
            {
                absX += currentZone.X * ScaleAtLevel(currentZone.Level);
                absY += currentZone.Y * ScaleAtLevel(currentZone.Level);
                currentZone = currentZone.FromMap.fromZone;
            }

            for(var y = absY; y < absY + myHeight; y++)
            {
                for(var x = absX; x < absX + myWidth; x++)
                {
                    if (_masterMap[x, y] == null)
                        _masterMap[x, y] = new List<HierarchicalMap>();
                    _masterMap[x, y].Add(this);
                }
            }

            foreach(var zone in flatZones)
            {
                if(zone.SubMap != null)
                    zone.SubMap.GetMasterMap(_masterMap, depth + 1);
            }

            return _masterMap;
        }

        public void CreateSubMaps()
        {
            var subbed = new List<Guid>();
            for(var y = 0; y < _MapHeight; y++)
            {
                for(var x = 0; x < _MapWidth; x++)
                {
                    if(_Grid[x,y] != null && !subbed.Any(s=>s == _Grid[x, y].id))
                    {
                        _Grid[x, y].CreateSubMap(_Random);
                        _TotalLevels = Zone.totalLevels;
                        subbed.Add(_Grid[x, y].id);
                    }
                }
            }   
        }
        public void AddPortal(HierarchicalMapPortal portal)
        {
            _Portals.Add(portal);
        }

        public void SetTestPortals()
        {
            _Portals.AddRange(new List<HierarchicalMapPortal>() {
                new HierarchicalMapPortal()
                {
                    direction= new Point(1,0),
                    point = new Point(
                        0,
                        (int)Math.Floor((double)_MapHeight/2)
                    )
                },
                new HierarchicalMapPortal()
                {
                    direction= new Point(-1,0),
                    point = new Point(
                        _MapWidth - 1,
                        (int)Math.Floor((double)_MapHeight/2)+1
                    )
                },
                new HierarchicalMapPortal()
                {
                    direction= new Point(-1,0),
                    point = new Point(
                        (int)Math.Floor((double)_MapWidth/2),
                        0
                    )
                },
            });
        }

        internal bool IsPortalEntrance(HierarchicalMapPortal portal)
        {
            return (portal.point.X + portal.direction.X > 0 && portal.point.X + portal.direction.X < _MapWidth)
                &&
                (portal.point.Y + portal.direction.Y > 0 && portal.point.Y + portal.direction.Y < _MapHeight);
        }

        public bool SpawnZone(int maxSide, int minSide)
        {
            int SideX_o = _Random.Next(minSide, maxSide+1);
            int SideY_o = _Random.Next(minSide, maxSide+1);
            int SideX = SideX_o;
            int SideY = SideY_o;
            SearchCache search = SearchForFreePosition();
            if (search.result == null)
                return false;

            var foundPosition = false;
            Point? adjustedOrigin = null;
            while(search.result != null)
            {
                //int currentGoalSideX = SideX;
                //int currentGoalSideY = SideY;

                var failedToFindFit = false;
                adjustedOrigin = CanFitZoneOverPoint(new Point(SideX, SideY), search.result.Value);
                while (adjustedOrigin == null)
                {
                    if (SideY < SideX)
                        SideX -= 1;
                    else
                        SideY -= 1;

                    if(SideX < minSide || SideY < minSide)
                    {
                        failedToFindFit = true;
                        break;
                    } 
                }
                if (failedToFindFit)
                {
                    SearchForFreePosition(search);
                    SideX = SideX_o;
                    SideY = SideY_o;
                }
                else
                {
                    foundPosition = true;
                    break;
                }
            }

            if (!foundPosition)
                return false;

            var zone = new Zone()
            {
                Level = _Level,
                Height = SideY,
                Width = SideX,
                X = adjustedOrigin.Value.X,
                Y = adjustedOrigin.Value.Y,
                FromMap = this
            };

            AssignZoneToGrid(zone);
            AssignPortalsToZone(zone);
            PrintToConsole();
            return true;
        }

        public bool SpawnZone(int maxSide, int minSide, Point position)
        {
            int SideX_o = _Random.Next(minSide, maxSide + 1);
            int SideY_o = _Random.Next(minSide, maxSide + 1);
            int SideX = SideX_o;
            int SideY = SideY_o;
            var adjustedOrigin = CanFitZoneOverPoint(new Point(SideX, SideY), position);
            while (adjustedOrigin == null)
            {
                if (SideY < SideX)
                    SideX -= 1;
                else
                    SideY -= 1;

                if (SideX < minSide || SideY < minSide)
                {
                    return false;
                }

                adjustedOrigin = CanFitZoneOverPoint(new Point(SideX, SideY), position);
            }
            var zone = new Zone()
            {
                Level = _Level,
                Height = SideY,
                Width = SideX,
                X = adjustedOrigin.Value.X,
                Y = adjustedOrigin.Value.Y,
                FromMap = this
            };

            AssignZoneToGrid(zone);
            AssignPortalsToZone(zone);
            PrintToConsole();
            return true;
        }

        public bool CoverPathsWithZones(int maxSide, int minSide)
        {
            var createdZones = new List<Zone>();
            var failures = 0;
            while (true)
            {
                Console.WriteLine("Failures: " + failures.ToString());

                if(failures >= 10)
                {
                    failures = 0;

                    //reset grid completely
                    for (var x = 0; x < _MapWidth; x++)
                    {
                        for (var y = 0; y < _MapHeight; y++)
                        {
                            if (_Grid[x, y] != null && createdZones.Any(cz => cz.id == _Grid[x, y].id))
                            {
                                _Grid[x, y] = null;
                            }
                        }
                    }
                    flatZones.Clear();
                    createdZones.Clear();
                }

                //delete last one from created zones
                var deleteAmount = failures;
                for (var i = 0; i < deleteAmount; i++)
                {
                    var deletable = createdZones.LastOrDefault();
                    if (deletable != null)
                    {
                        createdZones.RemoveAt(createdZones.Count - 1);
                        flatZones = flatZones.Where(fz => fz.id != deletable.id).ToList();
                        for (var x = 0; x < _MapWidth; x++)
                        {
                            for (var y = 0; y < _MapHeight; y++)
                            {
                                if (_Grid[x, y] != null && _Grid[x, y].id == deletable.id)
                                {
                                    _Grid[x, y] = null;
                                }
                            }
                        }
                    }
                }

                var succeeded = true;
                foreach (var path in _Paths)
                {
                    foreach (var pathpoint in path._pathPoints)
                    {
                        if (_Grid[pathpoint.point.X, pathpoint.point.Y] == null)
                        {
                            var result = SpawnZone(maxSide, minSide, pathpoint.point);
                            if (result)
                                createdZones.Add(_Grid[pathpoint.point.X, pathpoint.point.Y]);
                            else
                            {
                                succeeded = false;
                                break;
                            }
                        }
                    }
                    if (succeeded == false)
                        break;
                }

                if (succeeded)
                    break;
                failures += 1;
            }

            return true;
        }

        private void AssignZoneToGrid(Zone zone)
        {
            for(var x = 0; x < zone.Width; x++)
            {
                for(var y = 0; y < zone.Height; y++)
                {
                    _Grid[x + zone.X, y + zone.Y] = zone;
                }
            }
            flatZones.Add(zone);
        }

        private class NeighborZone
        {
            public Zone Neighbor;
            public Point MyAbsPoint;
            public Point NeighborAbsPoint;
        }
        private void AssignPortalsToZone(Zone zone)
        {
            var neighborZones = new List<NeighborZone>();
            //top
            for(var x = zone.X; x < zone.X + zone.Width; x++)
            {
                var y = zone.Y - 1;
                if (!(x >= 0 && x < _MapWidth && y >= 0 && y < _MapHeight))
                    continue;
                var potentialNeighbor = _Grid[x, y];
                PrintToConsole(new Point(x, y));
                if (potentialNeighbor != null && zone.id != potentialNeighbor.id)
                {
                    neighborZones.Add(new NeighborZone()
                    {
                        MyAbsPoint = new Point(x, y + 1),
                        NeighborAbsPoint = new Point(x, y),
                        Neighbor = potentialNeighbor
                    });
                }
            }
            //left
            for (var y = zone.Y; y < zone.Y + zone.Height; y++)
            {
                var x = zone.X - 1;
                if (!(x >= 0 && x < _MapWidth && y >= 0 && y < _MapHeight))
                    continue;
                var potentialNeighbor = _Grid[x, y];
                PrintToConsole(new Point(x, y));
                if (potentialNeighbor != null && zone.id != potentialNeighbor.id)
                {
                    neighborZones.Add(new NeighborZone()
                    {
                        MyAbsPoint = new Point(x + 1, y),
                        NeighborAbsPoint = new Point(x, y),
                        Neighbor = potentialNeighbor
                    });
                }
            }
            //right
            for (var y = zone.Y; y < zone.Y + zone.Height; y++)
            {
                var x = zone.X + zone.Width;
                if (!(x >= 0 && x < _MapWidth && y >= 0 && y < _MapHeight))
                    continue;
                var potentialNeighbor = _Grid[x, y];
                PrintToConsole(new Point(x, y));
                if (potentialNeighbor != null && zone.id != potentialNeighbor.id)
                {
                    neighborZones.Add(new NeighborZone()
                    {
                        MyAbsPoint = new Point(x - 1, y),
                        NeighborAbsPoint = new Point(x, y),
                        Neighbor = potentialNeighbor
                    });
                }
            }
            //bottom
            for (var x = zone.X; x < zone.X + zone.Width; x++)
            {
                var y = zone.Y +zone.Height;
                if (!(x >= 0 && x < _MapWidth && y >= 0 && y < _MapHeight))
                    continue;
                var potentialNeighbor = _Grid[x, y];
                PrintToConsole(new Point(x, y));
                if (potentialNeighbor != null && zone.id != potentialNeighbor.id)
                {
                    neighborZones.Add(new NeighborZone()
                    {
                        MyAbsPoint = new Point(x, y-1),
                        NeighborAbsPoint = new Point(x, y),
                        Neighbor = potentialNeighbor
                    });
                }
            }

            foreach(var group in neighborZones.GroupBy(x => x.Neighbor.id))
            {
                var zoneList = group.ToList();
                var chosen = zoneList[_Random.Next(0, zoneList.Count)];
                if (zone.ZonePortals.Any(x => x.Destination.id == chosen.Neighbor.id))
                    continue;
                zone.ZonePortals.Add(new ZonePortal()
                {
                    Destination = chosen.Neighbor,
                    MapRelativePosition = chosen.MyAbsPoint,
                    ZoneRelativePosition = new Point(chosen.MyAbsPoint.X - zone.X, chosen.MyAbsPoint.Y - zone.Y)
                });
                chosen.Neighbor.ZonePortals.Add(new ZonePortal()
                {
                    Destination = zone,
                    MapRelativePosition = chosen.NeighborAbsPoint,
                    ZoneRelativePosition = new Point(chosen.NeighborAbsPoint.X - chosen.Neighbor.X, chosen.NeighborAbsPoint.Y - chosen.Neighbor.Y)
                });
            }
        }

        public void CreatePaths()
        {
            List<HierarchicalMapPortal> portals = new List<HierarchicalMapPortal>();
            portals.AddRange(_Portals);
            if(portals.Count == 1)
            {
                //add dummy portal at center
                var newPoint = new Point(
                        (int)Math.Floor((double)_MapWidth / 2),
                        (int)Math.Floor((double)_MapHeight / 2)
                    );
                Point newDirection;
                if(newPoint.X >= portals[0].point.X)
                {
                    newDirection = new Point(1, 0);
                }
                else
                {
                    newDirection = new Point(-1, 0);
                }
                
                portals.Add(new HierarchicalMapPortal()
                {
                    point = newPoint,
                    direction = newDirection
                });
            }


            List<HierarchicalMapPortal> connected = new List<HierarchicalMapPortal>();
            foreach(var portal in portals)
            {
                foreach(var destPortal in portals.Where(x=>x.id != portal.id && !connected.Any(c=>c.id == x.id)))
                {
                    _Paths.Add(
                        new Path(
                            new PathPoint(portal.point, portal.direction), 
                            new PathPoint(destPortal.point, destPortal.direction),
                            _MapWidth,
                            _MapHeight,
                            null,
                            _Random
                        )
                    );
                    PrintToConsole();
                }
                connected.Add(portal);
            }
        }

        private class SearchCache{
            public Point? result;
            public Point position;
            public int maxY = 1;
            public int maxX = 1;
            public bool minnedX = true;
            public bool minnedY = false;
            public bool horizTravel = true;
        }
        private SearchCache SearchForFreePosition(SearchCache searchCache = null)
        {
            Point center = new Point(
                (int)Math.Floor(((decimal)_MapWidth) / 2),
                (int)Math.Floor(((decimal)_MapHeight) / 2)
            );
            if (searchCache == null)
            {
                searchCache = new SearchCache();
                searchCache.position = center;
            }

            searchCache.result = null;

            //int maxY = 1;
            //int maxX = 1;
            //bool minnedX = true;
            //bool minnedY = false;
            //bool horizTravel = true;
            while(_Grid[searchCache.position.X, searchCache.position.Y] != null)
            {
                if(searchCache.horizTravel == false && center.Y - searchCache.position.Y >= 0 && (int)Math.Abs(center.Y - searchCache.position.Y) == searchCache.maxY)
                {
                    searchCache.horizTravel = true;
                    searchCache.minnedY = true;
                    searchCache.maxX += 1;
                }
                else if (searchCache.horizTravel == true && center.X - searchCache.position.X >= 0 && (int)Math.Abs(center.X - searchCache.position.X) == searchCache.maxX)
                {
                    searchCache.horizTravel = false;
                    searchCache.minnedX = true;
                    searchCache.maxY += 1;
                }
                else if (searchCache.horizTravel == false && center.Y - searchCache.position.Y < 0 && (int)Math.Abs(center.Y - searchCache.position.Y) == searchCache.maxY)
                {
                    searchCache.horizTravel = true;
                    searchCache.minnedY = false;
                }
                else if (searchCache.horizTravel == true && center.X - searchCache.position.X < 0 && (int)Math.Abs(center.X - searchCache.position.X) == searchCache.maxX)
                {
                    searchCache.horizTravel = false;
                    searchCache.minnedX = false;
                }

                if (searchCache.horizTravel)
                {
                    if (!searchCache.minnedX)
                    {
                        searchCache.position.X -= 1;
                    }
                    else
                    {
                        searchCache.position.X += 1;
                    }
                }
                else
                {
                    if (!searchCache.minnedY)
                    {
                        searchCache.position.Y -= 1;
                    }
                    else
                    {
                        searchCache.position.Y += 1;
                    }
                }

                if (searchCache.position.X < 0 || searchCache.position.X >= _MapWidth || searchCache.position.Y < 0 || searchCache.position.Y >= _MapHeight)
                {
                    searchCache.result = null;
                    return searchCache;
                }
                PrintToConsole(searchCache.position);
            }
            searchCache.result = searchCache.position;
            return searchCache;
        }

        public Point? CanFitZoneOverPoint(Point zoneDim, Point point)
        {
            Point checkPoint = new Point();
            //var checkPoint = new Point(
            //    point.X - (zoneDim.X -1),
            //    point.Y - (zoneDim.Y -1)
            //);

            for(var y = 0; y < zoneDim.Y; y++)
            {
                for(var x = 0; x < zoneDim.X; x++)
                {
                    checkPoint.X = (point.X - (zoneDim.X -1)) + x;
                    checkPoint.Y = (point.Y - (zoneDim.Y - 1)) + y;

                    var spaceFree = true;
                    for(var suby = 0; suby < zoneDim.Y; suby++)
                    {
                        for(var subx = 0; subx < zoneDim.X; subx++)
                        {
                            if (checkPoint.X + subx >= _MapWidth || checkPoint.Y + suby >= _MapHeight || checkPoint.X + subx < 0 || checkPoint.Y + suby < 0 )
                            {
                                spaceFree = false;
                                break;
                            }

                            if(_Grid[checkPoint.X+subx, checkPoint.Y + suby] != null)
                            {
                                spaceFree = false;
                                break;
                            }
                        }
                        if (spaceFree == false)
                            break;
                    }
                    if (spaceFree)
                        return checkPoint;
                }
            }
            return null;
        }
        public void PrintToConsoleRecursive(Point? cursorPosition = null)
        {
            for (var y = 0; y < _MapHeight; y++)
            {
                for (var x = 0; x < _MapWidth; x++)
                {

                }
            }
        }
        public void PrintToConsole(Point? cursorPosition = null)
        {
            for(var y = 0; y < _MapHeight; y++)
            {
                for (var x = 0; x < _MapWidth; x++)
                {
                    string s = "-";

                    if (_Paths.Any(p => p._pathPoints.Any(pp => pp.point.X == x && pp.point.Y == y)))
                    {
                        s = "+";
                    }

                    if (_Grid[x, y] != null)
                    {
                        s = "X";
                    }

                    if(_Grid[x,y] != null && _Grid[x,y].ZonePortals.Any(p=>p.MapRelativePosition.X == x && p.MapRelativePosition.Y == y))
                    {
                        s = "P";
                    }

                    

                    if (cursorPosition != null && cursorPosition.Value.X == x && cursorPosition.Value.Y == y)
                    {
                        s = "C";
                    }

                    Console.Write(s);
                }
                Console.Write(Environment.NewLine);
                
            }
            Console.Write(Environment.NewLine);
            Console.Write(Environment.NewLine);
            //Console.ReadKey();
        }


        public void PrintMasterToConsole()
        {
            var _masterMap = GetMasterMap();
            for(var y = 0; y < _masterMap.GetLength(1); y++)
            {
                for (var x = 0; x < _masterMap.GetLength(0); x++)
                {
                    var color = GetTopColor(_masterMap[x, y]);
                    Console.BackgroundColor = color;
                    Console.Write(' ');
                }
                Console.WriteLine();
            }
            Console.ReadKey();
        }

        public void PrintMasterToBitmap(string filepath)
        {
            var _masterMap = GetMasterMap();

            Bitmap bmp = new Bitmap(_masterMap.GetLength(0), _masterMap.GetLength(1));
            
            for (var y = 0; y < _masterMap.GetLength(1); y++)
            {
                for (var x = 0; x < _masterMap.GetLength(0); x++)
                {
                    var color = GetAvgColor(_masterMap[x, y]);
                    bmp.SetPixel(x, y, color);
                    
                }
                Console.WriteLine();
            }
            Console.ReadKey();

            BitmapOperations.SaveBitmapToFile(filepath, bmp);
        }

        public Color GetAvgColor(List<HierarchicalMap> maps)
        {
            Color color = Color.Black;
            int r = 0;
            int g = 0;
            int b = 0;
            foreach(var map in maps)
            {
                r += map.dbColor.R;
                g += map.dbColor.G;
                b += map.dbColor.B;
            }
            color = Color.FromArgb(
                r/maps.Count,
                g / maps.Count,
                b / maps.Count
            );

            return color;
        }
        public ConsoleColor GetTopColor(List<HierarchicalMap> maps)
        {
            ConsoleColor color = ConsoleColor.Black;
            if(maps.Count() >= 3)
            {
                var breaka = "here";
            }
            var map = maps.OrderByDescending(m => m._Level).FirstOrDefault();
            if (map != null)
                color = map.dbColorConsole;
            return color;
        }
    }
}
