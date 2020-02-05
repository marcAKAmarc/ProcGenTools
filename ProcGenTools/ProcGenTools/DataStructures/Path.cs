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
        public Point point;
        public Point? direction;

        public PathPoint(Point _point)
        {
            point = _point;
        }
    }

    public class Path
    {
        public List<PathPoint> _pathPoints;
        public Point _entrance;
        public Point _exit;
        public int _pathWidth;
        public int _pathHeight;
        public Random _random;

        public Path(Point entrance, Point exit, int pathWidth, int pathHeight, int? seed = null, Random random = null)
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

            var heightDifference = _exit.Y - _entrance.Y;
            var widthDifference = _exit.X - _entrance.X;
            var hTravel = widthDifference / Math.Abs(widthDifference);
            var heightChangePosition = Math.Min(_entrance.X, _exit.X) + (_random.NextDouble() * Math.Abs(widthDifference));
            heightChangePosition = Math.Floor(heightChangePosition);
           
            Point currentPoint = _entrance;
            do
            {
                _pathPoints.Add(new PathPoint(currentPoint));
                if(currentPoint.X == (int)heightChangePosition && currentPoint.Y != _exit.Y)
                {
                    _pathPoints.Last().direction = new Point(0,heightDifference / Math.Abs(heightDifference));
                }
                else
                {
                    _pathPoints.Last().direction = new Point(widthDifference / Math.Abs(widthDifference), 0);
                }

                currentPoint.X += ((Point)_pathPoints.Last().direction).X;
                currentPoint.Y += ((Point)_pathPoints.Last().direction).Y;
            }
            while (currentPoint != _exit);
            _pathPoints.Add(new PathPoint(currentPoint));
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
