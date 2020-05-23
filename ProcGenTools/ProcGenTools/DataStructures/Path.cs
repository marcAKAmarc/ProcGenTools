using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;


namespace ProcGenTools.DataStructures
{
    public class PathPoint
    {
        private readonly Point Point;
        private Point ToDirection;
        private Point FromDirection;

        public Point point { get { return Point; } }
        public Point toDirection { get { return ToDirection; } set { ToDirection = value; } }
        public Point fromDirection { get { return FromDirection; } set { FromDirection = value; } }

        public PathPoint(Point _point, Point? _toDirection, Point? _fromDirection = null)
        {
            this.ToDirection = new Point();
            if (_toDirection != null)
            {
                this.ToDirection = _toDirection.Value;
            }
            this.FromDirection = new Point();
            if (_fromDirection != null)
            {
                this.FromDirection = _fromDirection.Value;
            }
            this.Point = _point;
        }
    }

    public class SearchNode
    {

        public Point point;
        public SearchNode previous;

        public SearchNode(Point _point, SearchNode _previous)
        {
            previous = _previous;
            point = _point;
        }
    }

    public class Path
    {
        public List<PathPoint> _pathPoints;
        public PathPoint _entrance;
        public PathPoint _exit;
        public int _pathWidth;
        public int _pathHeight;
        public Random _random;

        public Path(PathPoint entrance, PathPoint exit, int pathWidth, int pathHeight, int? seed = null, Random random = null, bool LowRes = false)
        {
            _entrance = entrance;
            _exit = exit;
            _pathWidth = pathWidth;
            _pathHeight = pathHeight;
            _pathPoints = new List<PathPoint>();

            if (seed == null)
                _random = new Random();
            else
                _random = new Random(seed.Value);
            if (random != null)
                _random = random;

            if (!LowRes)
                CreateBasePath();
            else
            {
                var attempts = 0;
                while (!CreateBasePathLowRes()) {
                    attempts += 1;
                    if(attempts > 100)
                    {
                        break;
                    }
                }
                printToConsole();
            }
        }

        //public void DownSample(int divisor)
        //{
        //    var downSampledPoints = new List<PathPoint>();
        //    for (var i = 0; i < _pathPoints.Count(); i++)
        //    {

        //        downSampledPoints.Add( new PathPoint(
        //                new Point(
        //                    (int)Math.Floor(_pathPoints[i].point.X / 2f),
        //                    (int)Math.Floor(_pathPoints[i].point.Y / 2f)
        //                ),
        //                _pathPoints[i].toDirection,
        //                _pathPoints[i].fromDirection
        //        ));
        //    }
        //    var upSampledDownSampledPoints = new List<PathPoint>();
        //    for (var i = 0; i < _pathPoints.Count(); i++)
        //    {

        //        upSampledDownSampledPoints.Add(new PathPoint(
        //                new Point(
        //                    downSampledPoints[i].point.X * 2,
        //                    downSampledPoints[i].point.Y * 2
        //                ),
        //                _pathPoints[i].toDirection,
        //                _pathPoints[i].fromDirection
        //        ));
        //    }

            

        //    if (_pathPoints.First().point != upSampledDownSampledPoints.First().point)
        //    {
        //        var difference = new Point(
        //               _pathPoints.First().point.X - upSampledDownSampledPoints.First().point.X,
        //               _pathPoints.First().point.Y - upSampledDownSampledPoints.First().point.Y
        //        );
        //        for(var i = 0; i < upSampledDownSampledPoints.Count; i++)
        //        {
        //            upSampledDownSampledPoints[i] = new PathPoint(
        //                new Point(
        //                    upSampledDownSampledPoints[i].point.X + difference.X,
        //                    upSampledDownSampledPoints[i].point.Y + difference.Y
        //                ),
        //                upSampledDownSampledPoints[i].toDirection,
        //                upSampledDownSampledPoints[i].fromDirection
        //            );
        //        }
        //    }
        //    //adjust exit
        //    if (_pathPoints.Last().point != upSampledDownSampledPoints.Last().point)
        //    {
        //        var breaka = "here";
        //    }
        //}

        private bool CreateBasePath(int resolutionDivisor = 1)
        {
            //can fail

            _pathPoints.Clear();
            List<SearchNode> reachable = new List<SearchNode>() { new SearchNode(_entrance.point, null) };
            List<SearchNode> explored = new List<SearchNode>();
            List<PathPoint> result = new List<PathPoint>();
            while (reachable.Count > 0)
            {
                bool checkLast = _random.NextDouble()>=0.5;
                SearchNode checkNode;
                if (checkLast)
                    checkNode = reachable.Last();
                else
                    checkNode = reachable[0];



                if (checkNode.point == _exit.point)
                {
                    SearchNode next = null;
                    var current = checkNode;
                    var prev = current.previous;

                    result.Insert(0, new PathPoint(checkNode.point, _exit.toDirection));

                    //build path list
                    while (true)
                    {
                        if (next != null && result[0].toDirection == new Point())
                            result[0].toDirection = new Point(next.point.X - result[0].point.X, next.point.Y - result[0].point.Y);
                        if(prev != null)
                            result[0].fromDirection = new Point(result[0].point.X - prev.point.X, result[0].point.Y - prev.point.Y);
                        else
                            result[0].fromDirection = _entrance.toDirection;

                        if (prev == null)
                            break;

                        next = current;
                        current = prev;
                        prev = current.previous;
                        result.Insert(0, new PathPoint(current.point, null));
                    }
                    //result[0].toDirection = new Point(next.point.X - result[0].point.X, next.point.Y - result[0].point.Y);
                    
                    _pathPoints = result;
                    return true;
                }

                if (checkLast)
                    reachable.RemoveAt(reachable.Count - 1);
                else
                    reachable.RemoveAt(0);

                explored.Add(checkNode);

                var newReachables = new List<SearchNode>()
                {
                    new SearchNode(
                        new Point(checkNode.point.X + 1, checkNode.point.Y),
                        checkNode
                    ),
                    new SearchNode(
                        new Point(checkNode.point.X - 1, checkNode.point.Y),
                        checkNode
                    ),
                    new SearchNode(
                        new Point(checkNode.point.X, checkNode.point.Y+1),
                        checkNode
                    ),
                    new SearchNode(
                        new Point(checkNode.point.X, checkNode.point.Y-1),
                        checkNode
                    )
                };
                newReachables = newReachables.Where(n => n.point.X >= 0 && n.point.Y >= 0 && n.point.X < _pathWidth && n.point.Y < _pathHeight
                    && !reachable.Any(r => r.point.X == n.point.X && r.point.Y == n.point.Y)).ToList();

                reachable.AddRange(newReachables);
            }



            //# If we get here, no path was found :(
            return false;

            //function find_path (start_node, end_node):
            //    reachable = [start_node]
            //    explored = []

            //    while reachable is not empty:
            //        # Choose some node we know how to reach.
            //        node = choose_node(reachable)

            //        # If we just got to the goal node, build and return the path.
            //        if node == goal_node:
            //            return build_path(goal_node)

            //        # Don't repeat ourselves.
            //        reachable.remove(node)
            //        explored.add(node)

            //        # Where can we get from here?
            //        new_reachable = get_adjacent_nodes(node) - explored
            //        for adjacent in new_reachable:
            //            if adjacent not in reachable
            //                adjacent.previous = node  # Remember how we got there.
            //                reachable.add(adjacent)

            //    # If we get here, no path was found :(
            //    return None
            //var heightDifference = _exit.Y - _entrance.Y;
            //var widthDifference = _exit.X - _entrance.X;
            //var hTravel = widthDifference / Math.Abs(widthDifference);
            //var heightChangePosition = Math.Min(_entrance.X, _exit.X) + (_random.NextDouble() * Math.Abs(widthDifference));
            //heightChangePosition = Math.Floor(heightChangePosition);

            //PathPoint currentPoint = _entrance;
            //do
            //{
            //    _pathPoints.Add(new PathPoint(currentPoint));
            //    if(currentPoint.X == (int)heightChangePosition && currentPoint.Y != _exit.Y)
            //    {
            //        _pathPoints.Last().direction = new Point(0,heightDifference / Math.Abs(heightDifference));
            //    }
            //    else
            //    {
            //        _pathPoints.Last().direction = new Point(widthDifference / Math.Abs(widthDifference), 0);
            //    }

            //    currentPoint.X += ((Point)_pathPoints.Last().direction).X;
            //    currentPoint.Y += ((Point)_pathPoints.Last().direction).Y;
            //}
            //while (currentPoint != _exit);
            //_pathPoints.Add(new PathPoint(currentPoint));
        }
        private bool CreateBasePathLowRes()
        {
            _pathPoints.Clear();
            List<SearchNode> reachable = new List<SearchNode>() { new SearchNode(_entrance.point, null) };
            List<SearchNode> explored = new List<SearchNode>();
            List<PathPoint> result = new List<PathPoint>();
            while (reachable.Count > 0)
            {

                SearchNode checkNode = reachable[
                    Math.Max(
                        0,
                        reachable.Count - _random.NextChoice(new List<int> { 1, 1, 1, 1, 1, 2, 3, 4, 5, 6, 7, 8})
                    )
                ];
                reachable = reachable.Where(pp => pp.point != checkNode.point).ToList();
                

                //can move just 1
                bool canMoveAnywhere = false;
                if (/*wentStraightTwice ||*/
                        (
                            checkNode.previous != null &&
                            checkNode.previous.previous != null &&
                            checkNode.previous.previous.previous != null
                        ) &&
                        (
                           (checkNode.previous.previous.point.X == checkNode.point.X && checkNode.previous.previous.previous.point.X == checkNode.point.X)
                           ||
                           (checkNode.previous.previous.point.Y == checkNode.point.Y && checkNode.previous.previous.previous.point.Y == checkNode.point.Y)
                        )
                  )
                    canMoveAnywhere = true;


                bool canExit = false;
                var exitNext = new Point(_exit.point.X + _exit.toDirection.X, _exit.point.Y + _exit.toDirection.Y);
                if (checkNode.point == _exit.point &&
                        checkNode.previous != null && checkNode.previous.previous != null &&
                        (
                           (checkNode.previous.previous.point.X == checkNode.point.X && exitNext.X == checkNode.previous.point.X)
                           ||
                           (checkNode.previous.previous.point.Y == checkNode.point.Y && exitNext.Y == checkNode.previous.point.Y)
                        )
                  )
                    canExit = true;


                if (canExit)
                {
                    SearchNode next = null;
                    var current = checkNode;
                    var prev = current.previous;

                    result.Insert(0, new PathPoint(checkNode.point, _exit.toDirection));

                    //build path list
                    while (true)
                    {
                        if (next != null && result[0].toDirection == new Point())
                            result[0].toDirection = new Point(next.point.X - result[0].point.X, next.point.Y - result[0].point.Y);
                        if (prev != null)
                            result[0].fromDirection = new Point(result[0].point.X - prev.point.X, result[0].point.Y - prev.point.Y);
                        else
                            result[0].fromDirection = _entrance.toDirection;

                        if (prev == null)
                            break;

                        next = current;
                        current = prev;
                        prev = current.previous;
                        result.Insert(0, new PathPoint(current.point, null));
                    }
                    //result[0].toDirection = new Point(next.point.X - result[0].point.X, next.point.Y - result[0].point.Y);

                    _pathPoints = result;
                    return true;
                }

                

                explored.Add(checkNode);

                //moved more than 1
                //bool wentStraightTwice = false;
                //if (checkNode.previous != null &&
                //    (
                //        Math.Abs(checkNode.previous.point.X - checkNode.point.X) > 1
                //        ||
                //        Math.Abs(checkNode.previous.point.Y - checkNode.point.Y) > 1
                //    )
                //)
                //    wentStraightTwice = true;

                //if (wentStraightTwice)
                //{
                //    //add inbetweener
                //    //explored.Add(
                //    //    new SearchNode(
                //    //        new Point(
                //    //            checkNode.point.X + Math.Sign(checkNode.previous.point.X - checkNode.point.X),
                //    //            checkNode.point.Y + Math.Sign(checkNode.previous.point.Y - checkNode.point.Y)
                //    //        ),
                //    //        checkNode.previous
                //    //    )
                //    //);

                //    //checkNode.previous = explored.Last();
                //}



                ////can move just 1
                //bool canMoveAnywhere = false;
                //if (/*wentStraightTwice ||*/
                //        (
                //            checkNode.previous != null &&
                //            checkNode.previous.previous != null &&
                //            checkNode.previous.previous.previous != null
                //        ) &&
                //        (
                //           (checkNode.previous.previous.point.X == checkNode.point.X && checkNode.previous.previous.previous.point.X == checkNode.point.X)
                //           ||
                //           (checkNode.previous.previous.point.Y == checkNode.point.Y  && checkNode.previous.previous.previous.point.Y == checkNode.point.Y)
                //        )   
                //  )
                //    canMoveAnywhere = true;

                var newReachables = new List<SearchNode>();
                if (canMoveAnywhere)
                {
                    newReachables = new List<SearchNode>()
                    {
                        new SearchNode(
                            new Point(checkNode.point.X + 1, checkNode.point.Y),
                            checkNode
                        ),
                        new SearchNode(
                            new Point(checkNode.point.X - 1, checkNode.point.Y),
                            checkNode
                        ),
                        new SearchNode(
                            new Point(checkNode.point.X, checkNode.point.Y+1),
                            checkNode
                        ),
                        new SearchNode(
                            new Point(checkNode.point.X, checkNode.point.Y-1),
                            checkNode
                        )
                    };
                }
                else
                {
                    if (checkNode.previous != null)
                    {
                        newReachables = new List<SearchNode>()
                        {

                             new SearchNode(
                                new Point(
                                    checkNode.point.X + Math.Sign(checkNode.point.X - checkNode.previous.point.X),
                                    checkNode.point.Y + Math.Sign(checkNode.point.Y - checkNode.previous.point.Y)
                                ),
                                checkNode
                            )

                        };
                    }
                    else
                    {
                        newReachables = new List<SearchNode>()
                        {

                             new SearchNode(
                                new Point(
                                    checkNode.point.X + _entrance.toDirection.X,
                                    checkNode.point.Y + _entrance.toDirection.Y
                                ),
                                checkNode
                            )

                        };
                    }
                }
                
                newReachables = newReachables.Where(n => n.point.X >= 0 && n.point.Y >= 0 && n.point.X < _pathWidth && n.point.Y < _pathHeight
                    && !reachable.Any(r => r.point.X == n.point.X && r.point.Y == n.point.Y)
                    && !explored.Any(r => r.point.X == n.point.X && r.point.Y == n.point.Y)).ToList();

                if(newReachables.Count == 0)
                {
                    explored = explored.Where(x => x.point != checkNode.point).ToList();
                }

                reachable.AddRange(newReachables);
                reachable = reachable.OrderByDescending(pp => Math.Abs(pp.point.X - _exit.point.X) + Math.Abs(pp.point.Y - _exit.point.Y)).ToList();
            }

            return false;
        }
            //private void CreateBasePathLowerRes()
            //{
            //    _pathPoints.Clear();
            //    List<SearchNode> reachable = new List<SearchNode>() { new SearchNode(_entrance.point, null) };
            //    List<SearchNode> terminables = new List<SearchNode>();
            //    List<SearchNode> explored = new List<SearchNode>();
            //    List<PathPoint> result = new List<PathPoint>();
            //    while (reachable.Count > 0)
            //    {
            //        bool checkLast = _random.NextDouble() >= 0.5;
            //        SearchNode checkNode;
            //        if (checkLast)
            //            checkNode = reachable.Last();
            //        else
            //            checkNode = reachable[0];

            //        terminables = new List<SearchNode>()
            //        {
            //            new SearchNode(
            //                new Point(checkNode.point.X + 1, checkNode.point.Y),
            //                checkNode
            //            ),
            //            new SearchNode(
            //                new Point(checkNode.point.X - 1, checkNode.point.Y),
            //                checkNode
            //            ),
            //            new SearchNode(
            //                new Point(checkNode.point.X, checkNode.point.Y+1),
            //                checkNode
            //            ),
            //            new SearchNode(
            //                new Point(checkNode.point.X, checkNode.point.Y-1),
            //                checkNode
            //            )
            //        };

            //        if (checkNode.point == _exit.point
            //            || terminables[0].point == _exit.point
            //            || terminables[1].point == _exit.point
            //            || terminables[2].point == _exit.point
            //            || terminables[3].point == _exit.point)
            //        {
            //            SearchNode next = null;
            //            var current = checkNode;
            //            var prev = current.previous;

            //            result.Insert(0, new PathPoint(checkNode.point, _exit.toDirection));

            //            //incase endpoint is just a terminable neighbor
            //            if (checkNode.point != _exit.point)
            //                result.Insert(0, new PathPoint(_exit.point, _exit.toDirection));
            //            //build path list
            //            while (true)
            //            {
            //                if (next != null && result[0].toDirection == new Point())
            //                    result[0].toDirection = new Point(next.point.X - result[0].point.X, next.point.Y - result[0].point.Y);
            //                if (prev != null)
            //                    result[0].fromDirection = new Point(result[0].point.X - prev.point.X, result[0].point.Y - prev.point.Y);
            //                else
            //                    result[0].fromDirection = _entrance.toDirection;

            //                if (prev == null)
            //                    break;

            //                next = current;
            //                current = prev;
            //                prev = current.previous;
            //                result.Insert(0, new PathPoint(current.point, null));
            //            }
            //            //result[0].toDirection = new Point(next.point.X - result[0].point.X, next.point.Y - result[0].point.Y);

            //            _pathPoints = result;
            //            return;
            //        }

            //        if (checkLast)
            //            reachable.RemoveAt(reachable.Count - 1);
            //        else
            //            reachable.RemoveAt(0);

            //        explored.Add(checkNode);

            //        var newReachables = new List<SearchNode>()
            //        {
            //            new SearchNode(
            //                new Point(checkNode.point.X + 2, checkNode.point.Y),
            //                checkNode
            //            ),
            //            new SearchNode(
            //                new Point(checkNode.point.X - 2, checkNode.point.Y),
            //                checkNode
            //            ),
            //            new SearchNode(
            //                new Point(checkNode.point.X, checkNode.point.Y+2),
            //                checkNode
            //            ),
            //            new SearchNode(
            //                new Point(checkNode.point.X, checkNode.point.Y-2),
            //                checkNode
            //            )
            //        };
            //        newReachables = newReachables.Where(n => n.point.X >= 0 && n.point.Y >= 0 && n.point.X < _pathWidth && n.point.Y < _pathHeight
            //            && !reachable.Any(r => r.point.X == n.point.X && r.point.Y == n.point.Y)).ToList();



            //        reachable.AddRange(newReachables);
            //    }
            //}


        public void printToConsole()
        {
            Console.Write(Environment.NewLine);
            for (var y = 0; y < _pathHeight; y++)
            {
                for (var x = 0; x < _pathWidth; x++)
                {
                    if (_pathPoints.Any(pp => pp.point == new Point(x, y)))
                    {
                        var pp = _pathPoints.FirstOrDefault(pathp => pathp.point == new Point(x, y));
                        var idex = -1;
                        for(var i = 0; i < _pathPoints.Count; i++)
                        {
                            idex += 1;
                            if (_pathPoints[i].point == new Point(x, y))
                                break;
                        }
                        if (idex == 0)
                            Console.ForegroundColor = ConsoleColor.Green;
                        else
                            Console.ResetColor();


                        if (pp.toDirection.Y > 0)
                            Console.Write('D');
                        else if (pp.toDirection.Y < 0)
                            Console.Write('U');
                        else if (pp.toDirection.X > 0)
                            Console.Write('R');
                        else if (pp.toDirection.X < 0)
                            Console.Write('L');
                    }
                    else
                    {
                        Console.Write('-');
                    }
                }
                Console.Write(Environment.NewLine);
            }
        }
    }
}
