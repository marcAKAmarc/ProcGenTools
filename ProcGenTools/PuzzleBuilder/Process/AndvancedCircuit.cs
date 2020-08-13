﻿using PuzzleBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProcGenTools.Helper;
using ProcGenTools.DataStructures;
using PuzzleBuilder.Creators;
using System.Drawing;
using PuzzleBuilder.Process;
using ProcGenTools.DataProcessing;
using PuzzleBuilder.Core;

namespace PuzzleBuilder.Process
{
    public static class AdvancedCircuitProcess
    {

        public class PuzzleProcess:Process, WcfGridEventHandler
        {
            public Random Random;
            public IntentionGrid Grid;
            public int[] GroundLevels;
            public TilesetConfiguration TilesConfig;
            public WcfGrid WcfGrid;
            public iDisplayer Displayer;
            public iMeaningConverter Converter;

            public PuzzleProcess() { }

            public PuzzleProcess(Random random, IntentionGrid grid, TilesetConfiguration tilesConfig)
            {
                Init(random, grid, tilesConfig);
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

            public override PuzzleInfo CreateIt(List<Point> Entrances, List<Point> Exits)
            {
                int[] GroundLevels;
                if(Random.NextDouble() >= .5d)
                    GroundLevels = BasicCircuitCreators.CreateGroundLevels(Grid);
                else
                    GroundLevels = BasicCircuitCreators.CreateMinimalGroundLevels(Grid);

                //if all entrances have ground level
                var allPortals = new List<Point>();
                allPortals.AddRange(Entrances);
                allPortals.AddRange(Exits);
                if (PointsOnGroundLevels(GetHorizontalPortals(allPortals, Grid), Grid))
                {
                    BasicCircuitCreators.CreateCircuitFractured(Grid, Random);
                }
                else
                {
                    BasicCircuitCreators.CreateCircuit(Grid, Random);
                }

                var hEntrances = GetHorizontalPortals(Entrances, Grid);
                var vEntrances = GetVerticalPortals(Entrances, Grid);
                var vExits = GetVerticalPortals(Exits, Grid);
                var hExits = GetHorizontalPortals(Exits, Grid);

                foreach (var entrance in hEntrances)
                {
                    BasicCircuitCreators.CreateHorizontalEntrancePath(Grid, entrance.X, entrance.Y);
                }
                //Grid.Commit();
                foreach (var entrance in hExits)
                {
                    BasicCircuitCreators.CreateHorizontalExitPath(Grid, entrance.X, entrance.Y);
                }
                //Creators.CreateHorizontalExitPath(Grid, Exits.X, Exits.Y);
                foreach (var entrance in vEntrances)
                {
                    BasicCircuitCreators.CreateVerticalEntrancePath(Grid, entrance.X, entrance.Y);
                }
                foreach (var exit in vExits)
                {
                    BasicCircuitCreators.CreateVerticalExitPath(Grid, exit.X, exit.Y);
                }

                BasicCircuitCreators.CreateToggleExitDoor(Grid);

                BasicCircuitCreators.CreateToggleExitDoorButton(Grid, Random);

                //conveyor jump
                if (Random.NextDouble() < .666f)
                {
                    BasicCircuitCreators.AddConveyorJumpToHiddenPlace(Grid, Random);
                    if (Random.NextDouble() >= .5d)
                        BasicCircuitCreators.LengthenExistingConveyor(Grid, Random);
                }

                //rope cube
                if (Random.NextDouble() < .666f)
                    BasicCircuitCreators.CreateRopeCube(Grid, Random);

                //conveyor jump to nothing maybe
                if (Random.NextDouble() < .666f)
                {
                    BasicCircuitCreators.AddConveyorJump(Grid, Random);
                    if (Random.NextDouble() >= .5d)
                        BasicCircuitCreators.LengthenExistingConveyor(Grid, Random);
                }

                //box push to button
                if (Random.NextDouble() < .666f)
                    BasicCircuitCreators.CreateBoxButtonSection(Grid, Random);

                //shooters
                if (Random.NextDouble() < .333f)
                    BasicCircuitCreators.CreateShooterSection(Grid, Random);
                if(Random.NextDouble() < .333f)
                    BasicCircuitCreators.CreateShooterSection(Grid, Random);

                //add random conveyor
                if (Random.NextDouble() < .666f)
                {
                    BasicCircuitCreators.AddConveyor(Grid, Random);
                    if (Random.NextDouble() >= .5d)
                        BasicCircuitCreators.LengthenExistingConveyor(Grid, Random);
                }

                
                //enemy
                if (Random.NextDouble() < .5f)
                    BasicCircuitCreators.CreateEnemy(Grid, Random);

                //impede box door
                if (Random.NextDouble() < .666f)
                {
                    //box Impede door
                    var boxImpedePoint = BasicCircuitCreators.CreateBoxPathImpedingExtraDoor(Grid, Random);
                    if (boxImpedePoint != null)
                    {
                        var buttonResult = BasicCircuitCreators.CreateExtraDoorButton(Grid, Random, boxImpedePoint.Value);
                        if (buttonResult == false)
                        {
                            //undo last door creation if needed
                            Grid.Positions[boxImpedePoint.Value.X, boxImpedePoint.Value.Y].Intentions = Grid.Positions[boxImpedePoint.Value.X, boxImpedePoint.Value.Y].Intentions.Where(x => x.Meaning != Meaning.ToggleDoor).ToList();
                        }
                    }
                }

                //random extra door
                if (Random.NextDouble() < .666f)
                {
                    //create extra door
                    var doorloopsuccess = true;
                    var maxAmount = 1;
                    while (doorloopsuccess && maxAmount > 0)
                    {
                        var resultingPoint = BasicCircuitCreators.CreateExtraDoor(Grid, Random);
                        if (resultingPoint != null)
                        {
                            var buttonResult = BasicCircuitCreators.CreateExtraDoorButton(Grid, Random, resultingPoint.Value);
                            if (buttonResult == false)
                            {
                                //undo last door creation if needed
                                Grid.Positions[resultingPoint.Value.X, resultingPoint.Value.Y].Intentions = Grid.Positions[resultingPoint.Value.X, resultingPoint.Value.Y].Intentions.Where(x => x.Meaning != Meaning.ToggleDoor).ToList();
                                doorloopsuccess = false;
                            }
                        }
                        else
                        {
                            doorloopsuccess = false;
                        }
                        maxAmount -= 1;
                    }
                }

                BasicCircuitCreators.CreateBorder(Grid, Random);
                BasicCircuitCreators.AddSolidToSidesOfCircuit(Grid, Random);
                BasicCircuitCreators.BlanketNonDynamic(Grid, Random);

                var result = Helpers.ApplyIntentionToGrid(Grid, WcfGrid, TilesConfig, Converter);

                return new PuzzleInfo()
                {
                    TileMap = Helpers.ToBitmap(WcfGrid, TilesConfig),
                    Grid = Grid,
                    Success = result
                };

            }
            private List<Point> GetHorizontalPortals(List<Point> points, IntentionGrid grid)
            {
                return points.Where(p => p.X == 0 || p.X == grid.Width - 1).ToList();
            }
            private List<Point> GetVerticalPortals(List<Point> points, IntentionGrid grid)
            {
                return points.Where(p => p.Y == 0 || p.Y == grid.Height - 1).ToList();
            }
            private bool PointsOnGroundLevels(List<Point> points, IntentionGrid grid)
            {
                var groundlevels = grid.GetByMeaning(Meaning.GroundLevel);
                return !points.Any(p => !groundlevels.Any(gl => gl.Y == p.Y));
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
        }

    }
       
}
