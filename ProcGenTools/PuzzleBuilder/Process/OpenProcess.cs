using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using ProcGenTools.DataStructures;
using PuzzleBuilder.Core;

namespace PuzzleBuilder.Process
{
    public static class OpenProcess
    {
        public class PuzzleProcess : Process, WcfGridEventHandler
        {

            public Random Random;
            public IntentionGrid Grid;
            public TilesetConfiguration TilesConfig;
            public WcfGrid WcfGrid;
            public iDisplayer Displayer;
            public iMeaningConverter Converter;

            public PuzzleProcess() { }

            public PuzzleProcess(Random random, IntentionGrid grid, TilesetConfiguration tilesConfig)
            {
                Init(random, grid, tilesConfig);
            }

            public override void ClearForReuse(Random random)
            {
                Random = random;
                Grid.Clear();
                if (WcfGrid.Width != Grid.Width || WcfGrid.Height != Grid.Height)
                    WcfGrid = Helpers.InitWcfGrid(random, Grid, TilesConfig);
                else
                {
                    WcfGrid.ClearForReuse();
                    WcfGrid.SetRandom(random);

                }
                WcfGrid.eventHandler = this;
            }

            Point EntranceDir;
            Point ExitDir;
            Point NewEntrance;
            Point NewExit;

            public override PuzzleInfo CreateIt(List<Point> Entrances, List<Point> Exits)
            {
                SetPaths(Entrances, Exits);
                BlanketOpenOrNonDynamicOrSolid();
                var result = Helpers.ApplyIntentionToGrid(Grid, WcfGrid, TilesConfig, Converter);

                return new PuzzleInfo()
                {
                    TileMap = Helpers.ToBitmap(WcfGrid, TilesConfig),
                    Grid = Grid,
                    Success = result
                };
            }

            public override void Init(Random random, IntentionGrid grid, TilesetConfiguration tilesConfig)
            {
                Grid = grid;
                Random = random;
                TilesConfig = tilesConfig;

                Converter = new BasicCircuitProcess.MyMeaningConverter();

                WcfGrid = Helpers.InitWcfGrid(random, grid, TilesConfig);
                WcfGrid.eventHandler = this;
            }

            public override void SetDisplayer(iDisplayer displayer)
            {
                Displayer = displayer;
            }

            void WcfGridEventHandler.OnPropagation()
            {
                if (Displayer != null)
                    Displayer.Display(Helpers.ToBitmap(WcfGrid, TilesConfig));
                //BitmapOperations.SaveBitmapToFile("D:\\_Projects\\ProcGenTools\\ProcGenTools\\Test.World.Circuit\\Output\\liveUpdate.bmp", Helpers.ToBitmap(WcfGrid, TilesConfig));
            }

            void WcfGridEventHandler.OnCollapse()
            {
                if (Displayer != null)
                    Displayer.Display(Helpers.ToBitmap(WcfGrid, TilesConfig));
                //BitmapOperations.SaveBitmapToFile("D:\\_Projects\\ProcGenTools\\ProcGenTools\\Test.World.Circuit\\Output\\liveUpdate.bmp", Helpers.ToBitmap(WcfGrid, TilesConfig));

            }
            private void BlanketOpenOrNonDynamicOrSolid()
            {
                for (var x = 0; x < Grid.Width; x++)
                {
                    var lowestPathAtThisX = Grid.GetByMeaning(Meaning.HTraversablePlain).Union(Grid.GetByMeaning(Meaning.VTraversable)).Where(b => b.X == x).OrderByDescending(b => b.Y).Select(b=>b.Y).FirstOrDefault();

                    for (var y = 0; y < Grid.Height; y++)
                    {
                        if (Grid.Positions[x, y].Intentions.Count == 0)
                        {
                            
                            if  (x == 0 || y == 0 || x == Grid.Width - 1 || y == Grid.Height - 1)
                            {
                                Grid.Positions[x, y].Intentions.Add(Intention.SolidIntention());
                            }
                            else if(y < lowestPathAtThisX)
                            {
                                if (Random.NextDouble() > .5d)
                                {
                                    Grid.Positions[x, y].Intentions.Add(Intention.NonDynamic());
                                }
                                else
                                {
                                    Grid.Positions[x, y].Intentions.Add(Intention.EmptyIntention());
                                }
                            }
                            else
                            {
                                Grid.Positions[x, y].Intentions.Add(Intention.SolidIntention());
                            }
                        }   
                    }
                }
            }
            private void SetPaths(List<Point> Entrances, List<Point> Exits)
            {
                foreach (var entrance in Entrances)
                {
                    EntranceDir.X = 0;
                    EntranceDir.Y = 0;
                    if (entrance.X == 0)
                        EntranceDir.X = -1;
                    else if (entrance.Y == 0)
                        EntranceDir.Y = -1;
                    else if (entrance.Y == Grid.Height - 1)
                        EntranceDir.Y = 1;
                    else if (entrance.X == Grid.Width - 1)
                        EntranceDir.X = 1;

                    NewEntrance = new Point(entrance.X + EntranceDir.X, entrance.Y + EntranceDir.Y);
                    if (EntranceDir.X != 0)
                        Grid.Positions[entrance.X, entrance.Y].Intentions.Add(Intention.EntrancePathIntention());
                    else
                        Grid.Positions[entrance.X, entrance.Y].Intentions.Add(Intention.VerticalExitIntention());

                    foreach (var exit in Exits)
                    {
                        ExitDir.X = 0;
                        ExitDir.Y = 0;
                        if (exit.X == 0)
                            ExitDir.X = -1;
                        else if (exit.Y == 0)
                            ExitDir.Y = -1;
                        else if (exit.Y == Grid.Height - 1)
                            ExitDir.Y = 1;
                        else if (exit.X == Grid.Width - 1)
                            ExitDir.X = 1;

                        NewExit = new Point(exit.X + ExitDir.X, exit.Y + ExitDir.Y);
                        if (ExitDir.X != 0)
                            Grid.Positions[exit.X, exit.Y].Intentions.Add(Intention.ExitPathIntention());
                        else
                            Grid.Positions[exit.X, exit.Y].Intentions.Add(Intention.VerticalExitIntention());

                        var path = new Path(
                            new PathPoint(NewEntrance, EntranceDir),
                            new PathPoint(NewExit, ExitDir),
                            Grid.Width-1,
                            Grid.Height-1,
                            null,
                            Random
                        );

                        for(var i = 0; i <  path._pathPoints.Count; i++)
                        {
                            if(i == 0)
                            {
                           
                            }
                            if (path._pathPoints[i].toDirection == path._pathPoints[i].fromDirection)
                            {
                                if (path._pathPoints[i].toDirection.X != 0)
                                {
                                    Grid.Positions[path._pathPoints[i].point.X, path._pathPoints[i].point.Y].Intentions.Add(Intention.HorizontalPlainIntention());
                                }
                                if (path._pathPoints[i].toDirection.Y != 0)
                                {
                                    Grid.Positions[path._pathPoints[i].point.X, path._pathPoints[i].point.Y].Intentions.Add(Intention.VTraversableIntention());
                                }
                            }
                            else
                            {
                                Grid.Positions[path._pathPoints[i].point.X, path._pathPoints[i].point.Y].Intentions.Add(Intention.HorizontalPlainIntention());
                            }
                        }
                    }
                }
            }
        }
    }
}
