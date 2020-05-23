using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using ProcGenTools.DataProcessing;
using ProcGenTools.Helper;
namespace ProcGenTools.DataStructures
{
    public enum CreationMethod { Cluster, PathCover};
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

        public CreationMethod CreationMethod;

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
            SubMap.CreationMethod = CreationMethod;
           

            foreach (var portal in ZonePortals)
            {
                var newX = portal.ZoneRelativePosition.X * scale;
                var newY = portal.ZoneRelativePosition.Y * scale;
                if(portal.direction.X == 1)
                    newX = (Width * scale) -1;
                if (portal.direction.X == -1)
                    newX = 0;
                if (portal.direction.Y == 1)
                    newY = (Height * scale) - 1;
                if (portal.direction.Y == -1)
                    newY = 0;
                var subportal = new HierarchicalMapPortal()
                {
                    direction = portal.direction,
                    point = new Point(
                       newX,
                       newY
                    ),
                    ParentPortal = portal,
                    directionOfPassage = portal.directionOfPassage,
                    Map = SubMap
                };

                portal.SubPortal = subportal;

                if(portal.DestinationPortal != null && portal.DestinationPortal.SubPortal != null)
                {
                    subportal.DestinationPortal = portal.DestinationPortal.SubPortal;
                    subportal.destination = ((HierarchicalMapPortal)portal.DestinationPortal.SubPortal).Map;

                    //reflexive
                    portal.DestinationPortal.SubPortal.DestinationPortal = subportal;
                    ((HierarchicalMapPortal)portal.DestinationPortal.SubPortal).destination = SubMap;
                }
                
                SubMap.AddPortal(subportal);
            }

            HierarchicalMap parent = FromMap;
            while (parent != null)
            {
                parent._AllSubChildren.Add(SubMap);
                if (parent.fromZone == null || parent.fromZone.FromMap == null)
                    parent = null;
                else
                    parent = parent.fromZone.FromMap;
            }

            //generative stuff after here   
            if (HierarchicalMap.RelativeScales.Count() - 1 < Level)
                return;

            SubMap.CreatePaths();
            var result = SubMap.CoverPathsWithZones(3, 2);
            if(result == false)
                throw new Exception("failed to create path");
            if (SubMap.flatZones.Any(x => !x.touchesAnotherZone()) && SubMap.flatZones.Count() > 1)
                throw new Exception("failed - zone does not touch another zone");
            var randomZone = SubMap.flatZones[random.Next(SubMap.flatZones.Count)];
           // SubMap.SpawnZoneAtSearchPosition(3, 2, new Point(randomZone.X, randomZone.Y), true);
            if (SubMap.flatZones.Any(x => !x.touchesAnotherZone()) && SubMap.flatZones.Count() > 1)
                throw new Exception("failed - zone does not touch another zone");
            //SubMap.SpawnZoneAtSearchPosition(3, 2, new Point(randomZone.X, randomZone.Y), true);
            if (SubMap.flatZones.Any(x => !x.touchesAnotherZone()) && SubMap.flatZones.Count() > 1)
                throw new Exception("failed - zone does not touch another zone");
            //SubMap.SpawnZone(3, 2, new Point(randomZone.X, randomZone.Y));

            //create extraneous zones?
            SubMap.SpawnZoneAtClusterPosition(3, 2, null, true);
            //SubMap.SpawnZoneAtClusterPosition(3, 2, null, true);
            //SubMap.SpawnZoneAtClusterPosition(3, 2, null, true);

            SubMap.MarkExitOnlyPortals();



            SubMap.CreateSubMaps();
        }

        public bool touchesAnotherZone()
        {
            //check if touches another item
            var touchesAnother = false;
            var _Grid = FromMap._Grid;
            for (var tx = 0; tx < Width; tx++)
            {

                if (
                     tx + X < _Grid.GetLength(0)
                     && Y - 1 > 0
                     && _Grid[tx + X, Y - 1] != null
                )
                {
                    touchesAnother = true;
                    break;
                }
            }
            if (touchesAnother)
                return true;
            for (var tx = 0; tx < Width; tx++)
            {

                if (
                     tx + X < _Grid.GetLength(0)
                     && Y + Height < _Grid.GetLength(1)
                     && _Grid[tx + X, Y + Height] != null
                )
                {
                    touchesAnother = true;
                    break;
                }
            }
            if (touchesAnother)
                return true;
            for (var ty = 0; ty < Height; ty++)
            {
                if (

                     ty + Y < _Grid.GetLength(1)
                     && X - 1 > 0
                     && _Grid[X - 1, ty + Y] != null
                )
                {
                    touchesAnother = true;
                    break;
                }
            }
            if (touchesAnother)
                return true;
            for (var ty = 0; ty < Height; ty++)
            {

                if (
                     ty + Y < _Grid.GetLength(1)
                     && X + Width < _Grid.GetLength(0)
                     && _Grid[X + Width, ty + Y] != null
                )
                {
                    touchesAnother = true;
                    break;
                }
            }
            if (touchesAnother)
                return true;
            return false;
        }
    }
    public interface NestedPortal
    {
        NestedPortal SubPortal { get; set; }
        NestedPortal ParentPortal { get; set; }
        NestedPortal DestinationPortal { get; set; }
    }
    public class ZonePortal : NestedPortal
    {
        public Point ZoneRelativePosition;
        public Point MapRelativePosition;
        public Point direction;
        public Zone Destination;
        public NestedPortal SubPortal { get; set; }
        public NestedPortal ParentPortal { get; set; }
        public ZonePortalDirection directionOfPassage;
        public NestedPortal DestinationPortal { get; set; }
        public Zone zone;
    }
    public enum ZonePortalDirection { In, Out, Bidirectional};
    public class HierarchicalMapPortal:NestedPortal
    {
        public Guid id;
        public Point point;
        public HierarchicalMap destination;
        public Point direction; //pass through
        public NestedPortal SubPortal { get; set; }
        public NestedPortal ParentPortal { get; set; }
        public ZonePortalDirection directionOfPassage;
        public NestedPortal DestinationPortal { get; set; }
        public HierarchicalMap Map;
        public Point portalOffsetFromRoom;
        public HierarchicalMapPortal()
        {
            id = Guid.NewGuid();
        }
    }

    public class HierarchicalMap
    {
        public Guid _Id;
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
        public List<HierarchicalMap> _AllSubChildren;
        public List<Path> _Paths;
        public Random _Random;
        public Zone fromZone = null;
        public CreationMethod CreationMethod;
        public Bitmap contents = null;
        public static int[] RelativeScales;

        private ConsoleColor dbColorConsole;
        private Color dbColor;

        private static int consoleColorIndex = 0;
        private static DifferentColors differentColors;


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
            _Id = Guid.NewGuid();
            _AllSubChildren = new List<HierarchicalMap>();
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

            if (differentColors == null)
                differentColors = new DifferentColors();
            dbColor = differentColors.GetVeryDifferentColor();

            //var colorMode = _Random.Next(3);
            //if (colorMode == 2)
            //    dbColor = Color.FromArgb(_Random.Next(128) + 128, _Random.Next(128) + 128, 0);
            //if (colorMode == 1)
            //    dbColor = Color.FromArgb(_Random.Next(128) + 128, 0, _Random.Next(128) + 128);
            //if (colorMode == 0)
            //    dbColor = Color.FromArgb(0, _Random.Next(128) + 128, _Random.Next(128) + 128);
        }
        public void MarkExitOnlyPortals()
        {
            List<Zone> lowerNumberedExtraneousZones = new List<Zone>();
            for(var i = 0; i < flatZones.Count; i++)
            {
                

                if (flatZones[i].CreationMethod != CreationMethod.Cluster)
                    continue;

                lowerNumberedExtraneousZones.Add(flatZones[i]);
                //filtered to cluster only

                //debugging
                //var connectedIds = flatZones[i].ZonePortals.Select(zp => zp.Destination.id).ToList();
                //var lowerIds = lowerNumberedExtraneousZones.Select(x => x.id).ToList();

                //continue if there are not any portals that connect to a lower indexed extraneous zone
                if (!flatZones[i].ZonePortals.Any(zp => lowerNumberedExtraneousZones.Any(ln => ln.id == zp.Destination.id)))
                    continue;

                foreach(var portalToEssential in flatZones[i].ZonePortals.Where(zp=>zp.Destination.CreationMethod == CreationMethod.PathCover))
                {
                    portalToEssential.directionOfPassage = ZonePortalDirection.Out;
                    ((ZonePortal)portalToEssential.DestinationPortal).directionOfPassage = ZonePortalDirection.In;
                }

                
            }
        }
        public List<HierarchicalMap>[,] GetMasterMap(List<HierarchicalMap>[,] _masterMap = null, int originalLevel = -1, int depth = 0)
        {

            if(depth == 3)
            {
                var breaka = "here";
            }

            if(originalLevel == -1)
                originalLevel = this._Level;
            var scaleAtLevel = HierarchicalMap.ScaleAtLevel(_Level);

            if (_masterMap == null)
                _masterMap = new List<HierarchicalMap>[scaleAtLevel*_MapWidth, scaleAtLevel * _MapHeight];

            var myWidth = scaleAtLevel * _MapWidth;
            var myHeight = scaleAtLevel * _MapHeight;


            var currentZone = this.fromZone;
            int absX = 0;
            int absY = 0;
            while (currentZone != null && currentZone.Level >= originalLevel)
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
                    zone.SubMap.GetMasterMap(_masterMap, originalLevel, depth + 1);
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

        public List<HierarchicalMapPortal>[,] GetMasterPortal(List<HierarchicalMapPortal>[,] _masterMap = null, int originalLevel = -1, int depth = 0)
        {

            if (depth == 3)
            {
                var breaka = "here";
            }

            if (originalLevel == -1)
                originalLevel = this._Level;
            var scaleAtLevel = HierarchicalMap.ScaleAtLevel(_Level);

            if (_masterMap == null)
                _masterMap = new List<HierarchicalMapPortal>[scaleAtLevel * _MapWidth, scaleAtLevel * _MapHeight];

            var myWidth = scaleAtLevel * _MapWidth;
            var myHeight = scaleAtLevel * _MapHeight;


            var currentZone = this.fromZone;
            int absX = 0;
            int absY = 0;
            while (currentZone != null && currentZone.Level >= originalLevel)
            {
                absX += currentZone.X * ScaleAtLevel(currentZone.Level);
                absY += currentZone.Y * ScaleAtLevel(currentZone.Level);
                currentZone = currentZone.FromMap.fromZone;
            }


            //
            // foreach(var portal in _Portals)
            //{
            for (var y = absY; y < absY + myHeight; y++)
            {
                for (var x = absX; x < absX + myWidth; x++)
                {
                    
                    if (_masterMap[x, y] == null)
                        _masterMap[x, y] = new List<HierarchicalMapPortal>();
                    foreach(var portal in _Portals.Where(p=>p.point.X == x - absX && p.point.Y == y - absY))
                        _masterMap[absX + portal.point.X, absY + portal.point.Y].Add(portal);
                }
            }
            //for (var y = absY; y < absY + myHeight; y++)
            //{
            //    for (var x = absX; x < absX + myWidth; x++)
            //    {
            //        if (_masterMap[x, y] == null)
            //            _masterMap[x, y] = new List<HierarchicalMapPortal>();
            //        _masterMap[x, y].Add(this);
            //    }
            //}

            foreach (var zone in flatZones)
            {
                if (zone.SubMap != null)
                    zone.SubMap.GetMasterPortal(_masterMap, originalLevel, depth + 1);
            }

            return _masterMap;
        }

        public List<HierarchicalMapPortal> GetFlatPortals()
        {
            var result = new List<HierarchicalMapPortal>();
            result.AddRange(_Portals);
            foreach(var subHierarchicalMap in flatZones.Where(fz=>fz.SubMap != null).Select(fz=>fz.SubMap))
            {
                result.AddRange(subHierarchicalMap.GetFlatPortals());
            }
            return result;
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

        public bool SpawnZoneAtClusterPosition(int maxSide, int minSide, Point? searchStartPoint = null, bool mustTouchAnother = false)
        {
            int SideX_o = _Random.Next(minSide, maxSide+1);
            int SideY_o = _Random.Next(minSide, maxSide+1);
            int SideX = SideX_o;
            int SideY = SideY_o;
            SearchCache search = SearchForFreePosition(null, searchStartPoint);
            if (search.result == null)
                return false;

            var foundPosition = false;
            Point? adjustedOrigin = null;

            var failures = 0;
            while(search.result != null && failures < 10)
            {
                //int currentGoalSideX = SideX;
                //int currentGoalSideY = SideY;

                var failedToFindFit = false;
                adjustedOrigin = CanFitZoneOverPoint(new Point(SideX, SideY), search.result.Value, mustTouchAnother);
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
                    failures += 1;
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
                FromMap = this,
                CreationMethod = CreationMethod.Cluster
            };

            AssignZoneToGrid(zone);
            AssignPortalsToZone(zone);
            //PrintToConsole();
            return true;
        }

        public bool SpawnZoneAtPosition(int maxSide, int minSide, Point position)
        {
            int SideX_o = _Random.Next(minSide, maxSide + 1);
            int SideY_o = _Random.Next(minSide, maxSide + 1);
            int SideX = SideX_o;
            int SideY = SideY_o;
            var adjustedOrigin = CanFitZoneOverPoint(new Point(SideX, SideY), position, true);
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
                FromMap = this,
                CreationMethod = CreationMethod.PathCover
            };

            AssignZoneToGrid(zone);
            AssignPortalsToZone(zone);
            //PrintToConsole();
            return true;
        }

        public bool CoverPathsWithZones(int maxSide, int minSide)
        {
            var createdZones = new List<Zone>();
            var failures = 0;
            while (true)
            {
                //Console.WriteLine("Failures: " + failures.ToString());

                if(failures >= 10)
                {
                    //Console.ReadLine();
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
                        createdZones = createdZones.Where(cz => cz.id != deletable.id).ToList();
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
                            var result = SpawnZoneAtPosition(maxSide, minSide, pathpoint.point);
                            if (result)
                            {
                                createdZones.Add(_Grid[pathpoint.point.X, pathpoint.point.Y]);
                                //flatZones.Add(_Grid[pathpoint.point.X, pathpoint.point.Y]);
                                _Grid[pathpoint.point.X, pathpoint.point.Y].CreationMethod = CreationMethod.PathCover;
                            }
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

            //Persist portals from parent
            foreach(var portal in _Portals)
            {
                var zone = _Grid[portal.point.X, portal.point.Y];
                zone.ZonePortals.Add(
                    new ZonePortal
                    {
                        Destination = null,
                        MapRelativePosition = portal.point,
                        ZoneRelativePosition = new Point(portal.point.X - zone.X, portal.point.Y - zone.Y),
                        directionOfPassage = portal.directionOfPassage,
                        ParentPortal = portal,
                        zone = zone,
                        direction = portal.direction,
                    }
                );
                portal.SubPortal = zone.ZonePortals.Last();
                //debug
                if(zone.ZonePortals.Last().directionOfPassage != ZonePortalDirection.Bidirectional)
                {
                    var breka = "here";
                }
                if (zone.ZonePortals.Last().direction == new Point())
                {
                    var breka = "here";
                }
                if (portal.DestinationPortal != null && portal.DestinationPortal.SubPortal != null)
                {
                    zone.ZonePortals.Last().Destination = ((ZonePortal)portal.DestinationPortal.SubPortal).zone;
                    zone.ZonePortals.Last().DestinationPortal = portal.DestinationPortal.SubPortal;
                }
            }

            PrintToConsole();
            if (flatZones.Any(x => !x.touchesAnotherZone()) && flatZones.Count() > 1)
                throw new Exception("failed - zone does not touch another zone");

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
                //PrintToConsole(new Point(x, y));
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
                //PrintToConsole(new Point(x, y));
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
                //PrintToConsole(new Point(x, y));
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
                //PrintToConsole(new Point(x, y));
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
                    ZoneRelativePosition = new Point(chosen.MyAbsPoint.X - zone.X, chosen.MyAbsPoint.Y - zone.Y),
                    directionOfPassage = ZonePortalDirection.Bidirectional,
                    direction = new Point()
                    {
                        X = Math.Sign(chosen.NeighborAbsPoint.X - chosen.MyAbsPoint.X),
                        Y = Math.Sign(chosen.NeighborAbsPoint.Y - chosen.MyAbsPoint.Y)
                    },
                    zone = zone

                });
                //debug
                if(zone.ZonePortals.Last().direction == new Point())
                {
                    var breaka = "here";
                }

                chosen.Neighbor.ZonePortals.Add(new ZonePortal()
                {
                    Destination = zone,
                    MapRelativePosition = chosen.NeighborAbsPoint,
                    ZoneRelativePosition = new Point(chosen.NeighborAbsPoint.X - chosen.Neighbor.X, chosen.NeighborAbsPoint.Y - chosen.Neighbor.Y),
                    directionOfPassage = ZonePortalDirection.Bidirectional,
                    direction = new Point()
                    {
                        X = Math.Sign(chosen.MyAbsPoint.X - chosen.NeighborAbsPoint.X),
                        Y = Math.Sign(chosen.MyAbsPoint.Y - chosen.NeighborAbsPoint.Y)
                    },
                    zone = chosen.Neighbor

                });
                zone.ZonePortals.Last().DestinationPortal = chosen.Neighbor.ZonePortals.Last();
                chosen.Neighbor.ZonePortals.Last().DestinationPortal = zone.ZonePortals.Last();

                if (chosen.Neighbor.ZonePortals.Last().direction == new Point())
                {
                    var breaka = "here";
                }

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
                //Point newDirection;
                //if(newPoint.X >= portals[0].point.X)
                //{
                //    newDirection = new Point(1, 0);
                //}
                //else
                //{
                //    newDirection = new Point(-1, 0);
                //}

                portals.Add(new HierarchicalMapPortal()
                {
                    point = newPoint,
                    direction = new Point(0, 0)
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
                    //PrintToConsole();
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
        private SearchCache SearchForFreePosition(SearchCache searchCache = null, Point? startPosition = null)
        {
            Point center = new Point(
                (int)Math.Floor(((decimal)_MapWidth) / 2),
                (int)Math.Floor(((decimal)_MapHeight) / 2)
            );
            if (searchCache == null)
            {
                searchCache = new SearchCache();
                if (startPosition.HasValue)
                    searchCache.position = startPosition.Value;
                else
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
                //PrintToConsole(searchCache.position);
            }
            searchCache.result = searchCache.position;
            return searchCache;
        }

        public Point? CanFitZoneOverPoint(Point zoneDim, Point point, bool mustTouchAnother = false)
        {
            Point checkPoint = new Point();
            
            //we have to randomize this so that things get solved when attempted again maybe.
            int yStart, yLimit, yDirection, xStart, xLimit, xDirection;
            yStart = 0;
            xStart = 0;
            yLimit = zoneDim.Y;
            xLimit = zoneDim.X;
            if(_Random.Next(2) == 0)
            {
                var temp = xStart;
                xStart = xLimit-1;
                xLimit = temp-1;
            }
            if (_Random.Next(2) == 0)
            {
                var temp = yStart;
                yStart = yLimit-1;
                yLimit = temp-1;
            }
            yDirection = (yLimit - yStart) / Math.Abs(yLimit - yStart);
            xDirection = (xLimit - xStart) / Math.Abs(xLimit - xStart);
            

            for (var y = yStart; y != yLimit; y+=yDirection)
            {
                for(var x = xStart; x != xLimit; x+=xDirection)
                {
                    checkPoint.X = (point.X - (zoneDim.X -1)) + x;
                    checkPoint.Y = (point.Y - (zoneDim.Y - 1)) + y;

                    var spaceFree = true;

                    for(var suby = 0; suby < zoneDim.Y; suby++)
                    {
                        for(var subx = 0; subx < zoneDim.X; subx++)
                        {
                            //Console.WriteLine();
                            //Console.WriteLine(xDirection.ToString() +", " + yDirection.ToString());
                            //PrintToConsole(new Point(subx, suby));
                            //Console.ReadKey();
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

                    
                    if (spaceFree && (!mustTouchAnother || ZoneDimensionsAdjacentToAnother(checkPoint.X,checkPoint.Y,zoneDim.X, zoneDim.Y)))
                        return checkPoint;
                }
            }
            return null;
        }

        public bool ZoneDimensionsAdjacentToAnother(int x, int y, int width, int height)
        {
            //check if touches another item
            var touchesAnother = false;
            for (var tx = 0; tx < width; tx++)
            {
                
                if (
                     tx + x < _Grid.GetLength(0)
                     && y - 1 > 0
                     && _Grid[tx + x, y - 1] != null
                )
                {
                    touchesAnother = true;
                    break;
                }
            }
            if (touchesAnother)
                return true;
            for (var tx = 0; tx < width; tx++)
            {

                if (
                     tx + x < _Grid.GetLength(0)
                     && y + height < _Grid.GetLength(1)
                     && _Grid[tx + x, y + height] != null
                )
                {
                    touchesAnother = true;
                    break;
                }
            }
            if (touchesAnother)
                return true;
            for (var ty = 0; ty < height; ty++)
            {
                if (
                     
                     ty + y < _Grid.GetLength(1)
                     && x - 1 > 0
                     && _Grid[x-1, ty + y] != null
                )
                {
                    touchesAnother = true;
                    break;
                }
            }
            if (touchesAnother)
                return true;
            for (var ty = 0; ty < height; ty++)
            {

                if (
                     ty + y < _Grid.GetLength(1)
                     && x + width < _Grid.GetLength(0)
                     && _Grid[x + width, ty + y] != null
                )
                {
                    touchesAnother = true;
                    break;
                }
            }
            if (touchesAnother)
                return true;
            return false;
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
            /*if(this.dbColorConsole == ConsoleColor.DarkCyan)
                Console.ReadKey();*/
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
            //Console.ReadKey();
        }

        public void PrintMasterOrderingToBitmap(string filepath)
        {
            var _masterMap = GetMasterMap();
            var _masterPortal = GetMasterPortal();
            double _numberOfTerminals = _AllSubChildren.Count(x => x._AllSubChildren.Count == 0);
            var _flatPortals = GetFlatPortals();
            var scale = 10;
            Bitmap bmp = new Bitmap(_masterMap.GetLength(0) * scale, _masterMap.GetLength(1) * scale);
            
            for (var y = 0; y < _masterMap.GetLength(1); y++)
            {
                
                for (var x = 0; x < _masterMap.GetLength(0); x++)
                {
                    var color = Color.IndianRed;
                    var terminalRoom = _masterMap[x, y].OrderBy(h => h._Level).LastOrDefault();
                    //colors
                    if (terminalRoom._AllSubChildren.Count == 0)
                    {
                        //find proper color for this terminal room
                        for(var i = 0; i < _AllSubChildren.Count; i++)
                        { 
                            if (terminalRoom != null && _AllSubChildren[i]._Id == _masterMap[x, y].FirstOrDefault(h => h._AllSubChildren.Count == 0)._Id)
                            {
                                ///???????
                                color = Color.FromArgb(
                                    (int)(Math.Sin((i / _numberOfTerminals) *2*Math.PI + 2*Math.PI/3)*125)+125,
                                    (int)(Math.Sin((i / _numberOfTerminals) *2* Math.PI + 4 * Math.PI / 3) * 125)+125,
                                    (int)(Math.Sin((i / _numberOfTerminals) * 2* Math.PI) * 125)+125
                                );
                                break;
                            }
                        }
                    }
                    //draw room
                    for(var dx = 0; dx < scale; dx++)
                    {
                        for(var dy = 0; dy < scale; dy++)
                        {
                            bmp.SetPixel((x * scale)+dx, (y * scale) + dy, color);
                        }
                    }

                    
                    ////portal
                    //var terminalPortals = _masterPortal[x, y].Where(p => p.SubPortal == null).ToList();
                    //foreach(var terminalPortal in terminalPortals)
                    //{
                    //    var startPoint = new Point(x*scale, y*scale);
                    //    var endPoint = new Point((x + terminalPortal.direction.X)*scale, (y + terminalPortal.direction.Y)*scale);
                    //    bmp.SetPixel(startPoint.X, startPoint.Y, Color.White);
                    //    if(endPoint.X < bmp.Width && endPoint.X > 0 && endPoint.Y < bmp.Height && endPoint.Y > 0)
                    //        bmp.SetPixel(endPoint.X, endPoint.Y, Color.White);
                    //}
                    
                }
                Console.WriteLine();
            }
            //Console.ReadKey();

            BitmapOperations.SaveBitmapToFile(filepath, bmp);
        }

        public void PrintMasterToBitmap(string filepath)
        {
            var _masterMap = GetMasterMap();
            var _masterPortal = GetMasterPortal();

            var _flatPortals = GetFlatPortals();

            Bitmap bmp = new Bitmap(_masterMap.GetLength(0), _masterMap.GetLength(1));
            
            for (var y = 0; y < _masterMap.GetLength(1); y++)
            {
                for (var x = 0; x < _masterMap.GetLength(0); x++)
                {
                    //colors
                    var color = GetAvgColor(_masterMap[x, y]);
                    bmp.SetPixel(x, y, color);

                    //portal
                    var terminalPortals = _masterPortal[x, y].Where(p => p.SubPortal == null).ToList();
                    foreach(var terminalPortal in terminalPortals)
                    {
                        var startPoint = new Point(x, y);
                        var endPoint = new Point(x + terminalPortal.direction.X, y + terminalPortal.direction.Y);
                        bmp.SetPixel(startPoint.X, startPoint.Y, Color.White);
                        if(endPoint.X < bmp.Width && endPoint.X > 0 && endPoint.Y < bmp.Height && endPoint.Y > 0)
                            bmp.SetPixel(endPoint.X, endPoint.Y, Color.White);
                    }
                    
                }
                Console.WriteLine();
            }
            //Console.ReadKey();

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

            var last = maps.OrderBy(x => x._Level).LastOrDefault();
            if(last != null)
            {
                color = Color.FromArgb(
                    (color.R + last.dbColor.R) / 2,
                    (color.G + last.dbColor.G) / 2,
                    (color.B + last.dbColor.B) / 2
                );
            }

            if (last.flatZones.Count == 0)
            {
                color = Color.FromArgb(
                    (color.R) / 4,
                    (color.G) / 4,
                    (color.B) / 4
                );
            }
            else
            {
                color = Color.FromArgb(
                    color.R + ((255-color.R)/2),
                    color.G + ((255 - color.G) / 2),
                    color.B + ((255 - color.B) / 2)
                );
            }


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
