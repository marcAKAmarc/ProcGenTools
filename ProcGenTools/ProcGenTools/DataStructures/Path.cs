using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcGenTools.DataStructures
{
    public class PathPoint
    {
        private readonly Point Point;
        private Point Direction;

        public Point point { get { return Point; } }
        public Point direction { get { return Direction; } set { Direction = value; } }

        public PathPoint(Point _point, Point? _direction)
        {
            this.Direction = new Point();
            if (_direction != null)
            {
                this.Direction = _direction.Value;
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

        public Path(PathPoint entrance, PathPoint exit, int pathWidth, int pathHeight, int? seed = null, Random random = null)
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

            CreateBasePath();
        }

        private void CreateBasePath()
        {
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
                    result.Insert(0, new PathPoint(checkNode.point, _exit.direction));
                    while (prev != null)
                    {
                        if(result[0].direction == new Point())
                            result[0].direction = new Point(result[0].point.X - prev.point.X, result[0].point.Y - prev.point.Y);
                        next = current;
                        current = prev;
                        prev = current.previous;
                        result.Insert(0, new PathPoint(current.point, null));
                    }
                    result[0].direction = _entrance.direction;
                    _pathPoints = result;
                    return;
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

        public void printToConsole()
        {
            for (var y = 0; y < _pathHeight; y++)
            {
                for (var x = 0; x < _pathWidth; x++)
                {
                    if (_pathPoints.Any(pp => pp.point == new Point(x, y)))
                    {
                        Console.Write('X');
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
