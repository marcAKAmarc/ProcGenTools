using System;
using System.Collections.Generic;
using System.Linq;
using ProcGenTools.Helper;
using System.Drawing;
using RoomEditor.Models;
using PuzzleBuilder.Core.Internal;

namespace PuzzleBuilder.Creators
{

    public static class BasicCircuitCreators
    {
        public static int[] CreateGroundLevels(IntentionGrid grid)
        {
            var floorHeight = 2.0;
            var groundLevels = new int[(int)Math.Floor((grid.Height - 2) / floorHeight)];
            for (var i = 0; i < groundLevels.Count(); i++)
            {
                groundLevels[i] = (int)Math.Floor((i) * (grid.Height - 2) / (float)groundLevels.Count());
                groundLevels[i] += (int)floorHeight;
            }

            for (var x = 1; x < grid.Width - 1; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    if (groundLevels.Any(gl => gl == y))
                    {
                        grid.Positions[x, y].Intentions.Add(Intention.GroundLevelIntention());
                    }
                }
            }

            DebugPrintMeaning(grid, Meaning.GroundLevel);

            return groundLevels;
        }

        public static int[] CreateMinimalGroundLevels(IntentionGrid grid)
        {
            var floorHeight = 2.0;
            var groundLevels = new int[2];//new int[(int)Math.Floor((grid.Height - 2) / floorHeight)];
            groundLevels[0] = (int)Math.Floor(grid.Height / 2f) - (int)Math.Floor(grid.Height / 4f);
            groundLevels[1] = (int)Math.Floor(grid.Height / 2f) + (int)Math.Floor(grid.Height / 4f);
            for (var x = 1; x < grid.Width - 1; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    if (groundLevels.Any(gl => gl == y))
                    {
                        grid.Positions[x, y].Intentions.Add(Intention.GroundLevelIntention());
                    }
                }
            }

            DebugPrintMeaning(grid, Meaning.GroundLevel);

            return groundLevels;
        }

        public static void CreateCircuit(IntentionGrid grid, Random random)
        {

            var groundLevels = grid.GetByMeaning(Meaning.GroundLevel).Select(x => x.Y).Distinct().ToArray();

            var verticals = new int[2];
            if (random.NextDouble() < .5d && grid.Width - 4 > 5)
            {
                verticals[0] = 3;//(int)Math.Floor(grid.Width / 4.0f);
                verticals[1] = grid.Width - 4;//(int)Math.Floor(3.0f * grid.Width / 4.0f);
            }
            else
            {
                verticals[0] = 2;//(int)Math.Floor(grid.Width / 4.0f);
                verticals[1] = grid.Width - 3;//(int)Math.Floor(3.0f * grid.Width / 4.0f);
            }


            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    if (
                        !(
                            y >= groundLevels.Min()
                            && y <= groundLevels.Max()
                            && x >= verticals[0]
                            && x <= verticals[1]
                        )
                    )
                        continue;

                    if (
                        (y == groundLevels.Min() || y == groundLevels.Max())
                        || (x == verticals[0] || x == verticals[1])
                    )
                    {
                        grid.Positions[x, y].Intentions.Add(Intention.CircuitIntention());
                    }

                    if (x == verticals[0] || x == verticals[1])
                    {
                        grid.Positions[x, y].Intentions.Add(Intention.LadderIntention());
                    }
                }
            }


            //Debug
            DebugPrintMeaning(grid, Meaning.Ladder);
            DebugPrintMeaning(grid, Meaning.Circuit);
        }

        public static void CreateCircuitFractured(IntentionGrid grid, Random random)
        {
            var groundLevels = grid.GetByMeaning(Meaning.GroundLevel).Select(x => x.Y).Distinct().ToArray();

            var verticals = new int[2];
            if (random.NextDouble() < .5d && grid.Width - 4 > 5)
            {
                verticals[0] = 3;//(int)Math.Floor(grid.Width / 4.0f);
                verticals[1] = grid.Width - 4;//(int)Math.Floor(3.0f * grid.Width / 4.0f);
            }
            else
            {
                verticals[0] = 2;//(int)Math.Floor(grid.Width / 4.0f);
                verticals[1] = grid.Width - 3;//(int)Math.Floor(3.0f * grid.Width / 4.0f);
            }

            var numberOfShiftedLevels = 1;//random.Next(0, groundLevels.Count());
            List<int> shiftedLevels = new List<int>();
            List<int> shiftedAmount = new List<int>();
            for (var i = 0; i < numberOfShiftedLevels; i++)
            {
                var shiftLevel = groundLevels.Where(gl => !shiftedLevels.Any(sl => sl == gl)).GetRandomOrDefault(random);
                shiftedLevels.Add(shiftLevel);
                shiftedAmount.Add((new List<int>() { 1, -1 }).GetRandomOrDefault(random));
            }

            bool onNormalCircuit = false;
            bool onFracturedVertical = false;
            bool[,] bufferGrid = new bool[grid.Width, grid.Height];
            for (var x = 0; x < grid.Width; x++)
            {
                for (var y = 0; y < grid.Height; y++)
                {
                    if (
                        !(
                            y >= groundLevels.Min()
                            && y <= groundLevels.Max()
                            && x >= verticals[0]
                            && x <= verticals[1]
                        )
                    )
                        continue;

                    if (
                        (y == groundLevels.Min() || y == groundLevels.Max())
                        || (x == verticals[0] || x == verticals[1])
                    )
                    {
                        bufferGrid[x, y] = true;
                    }

                    if (x == verticals[0] || x == verticals[1])
                    {
                        bufferGrid[x, y] = true;
                    }
                }
            }

            //shift
            for (var y = 0; y < grid.Height; y++)
            {
                var index = shiftedLevels.IndexOf(y);
                if (index == -1 && groundLevels.ToList().IndexOf(y) == -1)
                    index = shiftedLevels.IndexOf(y + 1);

                if (index == -1)
                    continue;

                var xStart = (int)Math.Round((Math.Sign(shiftedAmount[index]) / 2.0) + 0.5) * (bufferGrid.GetLength(0) - 1);
                var xDir = Math.Sign(shiftedAmount[index]) * -1;
                var xStop = grid.Width;
                if (Math.Sign(shiftedAmount[index]) == 1)
                    xStop = -1;
                for (var x = xStart; x != xStop; x += xDir)
                {
                    bool readValues = false;
                    int readFrom = x - shiftedAmount[index];
                    if (readFrom >= 0 && readFrom < grid.Width)
                        readValues = bufferGrid[readFrom, y];
                    bufferGrid[x, y] = readValues;
                }
            }

            //debug
            Console.WriteLine("shift debug");
            for (var y = 0; y < grid.Height; y++)
            {
                var str = "";
                for (var x = 0; x < grid.Width; x++)
                {
                    if (bufferGrid[x, y])
                        str += "X";
                    else
                        str += "-";

                }
                Console.WriteLine(str);
            }
            Console.WriteLine();

            //corners
            for (var y = 0; y < groundLevels.Max() - 1; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (
                        bufferGrid[x, y] == false
                        &&
                        (
                            (x >= 1 && bufferGrid[x - 1, y])
                            ||
                            (x < grid.Width - 1 && bufferGrid[x + 1, y])
                        )
                        &&
                        (
                            y < grid.Height - 1
                            &&
                            bufferGrid[x, y + 1]
                        )
                    )
                        bufferGrid[x, y] = true;
                }
            }

            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (!bufferGrid[x, y])
                        continue;
                    bool isLadder = false;
                    if (
                        (
                            y > 0
                            &&
                            bufferGrid[x, y - 1]
                        ) || (
                            y < grid.Height - 1
                            &&
                            bufferGrid[x, y + 1]
                        )
                    )
                    {
                        isLadder = true;
                    }

                    if (isLadder)
                        grid.Positions[x, y].Intentions.Add(Intention.LadderIntention());
                    grid.Positions[x, y].Intentions.Add(Intention.CircuitIntention());
                }
            }

            //Debug
            DebugPrintMeaning(grid, Meaning.Circuit);
            DebugPrintMeaning(grid, Meaning.Ladder);

        }

        public static void CreateHorizontalEntrancePath(IntentionGrid grid, int entranceX, int entranceY)
        {
            var onGroundLevel = grid.GetByMeaning(Meaning.GroundLevel).Any(t => t.Y == entranceY);
            var circuits = grid.GetByMeaning(Meaning.Circuit);
            var leftMost = circuits.Select(x => x.X).Min();
            var rightMost = circuits.Select(x => x.X).Max();

            var xDir = 0;
            var xStart = 0;
            var xStop = leftMost;
            if (entranceX <= 0)
                xDir = 1;
            else
            {
                xDir = -1;
                xStart = grid.Width - 1;
                xStop = rightMost;
            }

            for (var x = xStart; x != xStop; x += xDir)
            {
                if (x != xStart)
                    grid.Positions[xStop, entranceY].Intentions.Add(Intention.NonDynamic());
                if (!onGroundLevel && entranceY - 1 >= 0)
                {
                    grid.Positions[x, entranceY - 1].Intentions.Clear();
                    grid.Positions[x, entranceY - 1].Intentions.Add(Intention.SolidIntention());
                }
                if (!onGroundLevel && entranceY + 1 < grid.Height)
                {
                    grid.Positions[x, entranceY + 1].Intentions.Clear();
                    grid.Positions[x, entranceY + 1].Intentions.Add(Intention.SolidIntention());
                }
                grid.Positions[x, entranceY].Intentions.Add(Intention.EntrancePathIntention());
                //filter out solide
                grid.Positions[x, entranceY].Intentions = grid.Positions[x, entranceY].Intentions.Where(intent => intent.Meaning != Meaning.Solid).ToList();
            }

            var entranceYIter = entranceY;
            var entranceYIterDir = 0;
            if (entranceY > circuits.Select(t => t.Y).Max())
            {
                entranceYIterDir = -1;
            }
            if (entranceY < circuits.Select(t => t.Y).Min())
            {
                entranceYIterDir = 1;
            }
            if (entranceYIterDir != 0)
            {
                while (entranceYIter > circuits.Select(t => t.Y).Max() || entranceYIter < circuits.Select(t => t.Y).Min())
                {
                    grid.Positions[xStop, entranceYIter].Intentions.Add(Intention.LadderIntention());
                    grid.Positions[xStop, entranceYIter].Intentions.Add(Intention.NonDynamic());

                    entranceYIter += entranceYIterDir;
                }
            }


            /*if (entranceY > circuits.Select(t => t.Y).Max() || entranceY < circuits.Select(t => t.Y).Min())
            {
                grid.Positions[xStop, entranceY].Intentions.Add(Intention.LadderIntention());
                grid.Positions[xStop, entranceY].Intentions.Add(Intention.NonDynamic());
            }
            */
            //Debug
            DebugPrintMeaning(grid, Meaning.EntrancePath);
        }

        public static void CreateHorizontalExitPath(IntentionGrid grid, int exitX, int exitY)
        {
            var onGroundLevel = grid.GetByMeaning(Meaning.GroundLevel).Any(t => t.Y == exitY);
            var circuits = grid.GetByMeaning(Meaning.Circuit);
            var leftMost = circuits.Select(x => x.X).Min();
            var rightMost = circuits.Select(x => x.X).Max();

            var xDir = 0;
            var xStart = 0;
            var xStop = leftMost;
            if (exitX <= 0)
                xDir = 1;
            else
            {
                xDir = -1;
                xStart = grid.Width - 1;
                xStop = rightMost;
            }

            for (var x = xStart; x != xStop; x += xDir)
            {
                if (x != xStart)
                    grid.Positions[xStop, exitY].Intentions.Add(Intention.NonDynamic());

                if (!onGroundLevel && exitY - 1 >= 0)
                {
                    grid.Positions[x, exitY - 1].Intentions.Clear();
                    grid.Positions[x, exitY - 1].Intentions.Add(Intention.SolidIntention());
                }
                if (!onGroundLevel && exitY + 1 < grid.Height)
                {
                    grid.Positions[x, exitY + 1].Intentions.Clear();
                    grid.Positions[x, exitY + 1].Intentions.Add(Intention.SolidIntention());
                }
                grid.Positions[x, exitY].Intentions.Add(Intention.ExitPathIntention());
                //remove solid intention from this position
                grid.Positions[x, exitY].Intentions = grid.Positions[x, exitY].Intentions.Where(i => i.Meaning != Meaning.Solid).ToList();
            }
            var exitYIter = exitY;
            var exitYIterDir = 0;
            if(exitY > circuits.Select(t => t.Y).Max())
            {
                exitYIterDir = -1;
            }
            if (exitY < circuits.Select(t=> t.Y).Min())
            {
                exitYIterDir = 1;
            }
            if(exitYIterDir != 0)
            {
                while (exitYIter > circuits.Select(t => t.Y).Max() || exitYIter < circuits.Select(t => t.Y).Min())
                {
                    grid.Positions[xStop, exitYIter].Intentions.Add(Intention.LadderIntention());
                    grid.Positions[xStop, exitYIter].Intentions.Add(Intention.NonDynamic());

                    exitYIter += exitYIterDir;
                }
            }/*
            if (exitY > circuits.Select(t => t.Y).Max() || exitY < circuits.Select(t => t.Y).Min())
            {
                grid.Positions[xStop, exitY].Intentions.Add(Intention.LadderIntention());
                grid.Positions[xStop, exitY].Intentions.Add(Intention.NonDynamic());
            }
            */
            DebugPrintMeaning(grid, Meaning.ExitPath);
            DebugPrintMeaning(grid, Meaning.Ladder);
        }

        public static void CreateVerticalExitPath(IntentionGrid grid, int exitX, int exitY)
        {
            var circuits = grid.GetByMeaning(Meaning.Circuit);
            var topMost = circuits.Select(x => x.Y).Min();
            var bottomMost = circuits.Select(x => x.Y).Max();

            var yDir = 0;
            var yStart = 0;
            var yStop = topMost + 1;
            if (exitY <= 0)
                yDir = 1;
            else
            {
                yDir = -1;
                yStart = grid.Height - 1;
                yStop = bottomMost - 1;
            }

            for (var y = yStart; y != yStop; y += yDir)
            {
                grid.Positions[exitX, y].Intentions.Add(Intention.ExitPathIntention());
                //remove solid intention from this position
                grid.Positions[exitX, y].Intentions = grid.Positions[exitX, y].Intentions.Where(i => i.Meaning != Meaning.Solid).ToList();
                if (y != yStop - yDir)//not the last one //but why? tryting witout
                {
                    grid.Positions[exitX, y].Intentions.Add(Intention.VerticalExitIntention());
                }
                else
                {
                    //grid.Positions[exitX, y].Intentions.Add(Intention.LadderIntention());
                }
            }

            DebugPrintMeaning(grid, Meaning.VerticalExitWay);
        }

        public static void CreateVerticalEntrancePath(IntentionGrid grid, int exitX, int exitY)
        {
            var circuits = grid.GetByMeaning(Meaning.Circuit);
            var topMost = circuits.Select(x => x.Y).Min();
            var bottomMost = circuits.Select(x => x.Y).Max();

            var yDir = 0;
            var yStart = 0;
            var yStop = topMost + 1;
            if (exitY <= 0)
                yDir = 1;
            else
            {
                yDir = -1;
                yStart = grid.Height - 1;
                yStop = bottomMost - 1;
            }

            for (var y = yStart; y != yStop; y += yDir)
            {
                grid.Positions[exitX, y].Intentions.Add(Intention.EntrancePathIntention());
                if (y != yStop - yDir)
                {
                    grid.Positions[exitX, y].Intentions.Add(Intention.VerticalExitIntention());
                }

                //filter out solide
                grid.Positions[exitX, y].Intentions = grid.Positions[exitX, y].Intentions.Where(intent => intent.Meaning != Meaning.Solid).ToList();
            }

            DebugPrintMeaning(grid, Meaning.ExitPath);
        }

        public static void CreateToggleExitDoor(IntentionGrid grid, List<Portal> exits)
        {
            /*
            var tile = grid.GetByMeaning(Meaning.ExitPath).Where(t => 
                !t.Intentions.Any(i=>
                    i.Meaning == Meaning.VerticalExit
                ) 
                &&( t.X == grid.Width - 1 || t.X == 0 || t.Y == 0 || t.Y == grid.Height - 1)
            ).FirstOrDefault();
            
            if (tile == null)
                return;

            grid.Positions[tile.X, tile.Y].Intentions.Add(Intention.ToggleDoorIntention());
            */

            foreach (var exit in exits)
            {

                grid.Positions[exit.point.X, exit.point.Y].Intentions.Add(Intention.ToggleDoorIntention(exit.GetLockId()));
                grid.Positions[exit.point.X, exit.point.Y].Intentions = 
                    grid.Positions[exit.point.X, exit.point.Y].Intentions.Where(x => x.Meaning != Meaning.VerticalExitWay).ToList();
            }
            

            DebugPrintMeaning(grid, Meaning.ToggleDoor);
        }

        public static Point? CreateExtraDoor(IntentionGrid grid, Random random)
        {
            var tile = grid.GetByMeaning(Meaning.GroundLevel).Where(x =>
                x.Intentions.Count() == 1
                &&
                x.X > 1 && x.X < grid.Width - 2
            ).ToList().GetRandomOrDefault(random);

            if (tile == null)
                return null;

            //remove walkable?
            tile.Intentions = tile.Intentions.Where(x => x.Meaning != Meaning.Walkable && x.Meaning != Meaning.NonDynamnic && x.Meaning != Meaning.BoxPath && x.Meaning != Meaning.BoxPathVertical).ToList();

            var intention = Intention.ToggleDoorIntention();
            grid.Positions[tile.X, tile.Y].Intentions.Add(Intention.ToggleDoorIntention());

            DebugPrintMeaning(grid, Meaning.ToggleDoor);

            return new Point(tile.X, tile.Y);
        }

        public static Point? CreateBoxPathImpedingExtraDoor(IntentionGrid grid, Random random)
        {
            var tile = grid.GetByMeaning(Meaning.GroundLevel).Where(x =>
                /*x.Intentions.Count() == 1
                ||(x.Intentions.Count() == 2 && x.Intentions.Any(i => i.Meaning == Meaning.BoxPath))*/
                x.Intentions.Count() == 4 && x.Intentions.Any(i => i.Meaning == Meaning.BoxPath) && x.Intentions.Any(i => i.Meaning == Meaning.NonDynamnic) && !x.Intentions.Any(i => i.Meaning == Meaning.BoxPathVertical)
                && !grid.Positions[x.X, x.Y - 1].Intentions.Any(i=> i.Meaning == Meaning.BoxPathVertical || i.Meaning == Meaning.VerticalExitWay || i.Meaning == Meaning.VTraversable || i.Meaning == Meaning.Ladder || i.Meaning == Meaning.Empty)
            ).ToList().GetRandomOrDefault(random);

            if (tile == null)
                return null;

            //remove walkable, non dynamic, box path?
            tile.Intentions = tile.Intentions.Where(x => x.Meaning != Meaning.Walkable && x.Meaning != Meaning.NonDynamnic && x.Meaning != Meaning.BoxPath && x.Meaning != Meaning.BoxPathVertical).ToList();

            var intention = Intention.ToggleDoorIntention();
            grid.Positions[tile.X, tile.Y].Intentions.Add(Intention.ToggleDoorIntention());

            DebugPrintMeaning(grid, Meaning.ToggleDoor);

            return new Point(tile.X, tile.Y);
        }

        public static bool CreateExtraDoorButton(IntentionGrid grid, Random random, Point doorIntentionPoint)
        {
            var tile = grid.GetByMeaning(Meaning.GroundLevel).Where(x =>
                (x.Intentions.Count() == 1
                ||(x.Intentions.Count() == 2 && x.Intentions.Any(i => i.Meaning == Meaning.Circuit)))
                && !grid.Positions[x.X, x.Y - 1].Intentions.Any(i => i.Meaning == Meaning.BoxPathVertical || i.Meaning == Meaning.VerticalExitWay || i.Meaning == Meaning.VTraversable || i.Meaning == Meaning.Ladder)
            ).ToList().GetRandomOrDefault(random);

            if (tile == null)
                return false;

            var buttonIntention = Intention.ButtonIntention();
            buttonIntention.Info = "toggle";
            buttonIntention.RelatedTileMeaning = grid.Positions[doorIntentionPoint.X, doorIntentionPoint.Y].Intentions.First(x=>x.Meaning == Meaning.ToggleDoor);
            buttonIntention.RelatedTilePosition = doorIntentionPoint;
            buttonIntention.RelatedTileMeaning.RelatedTileMeaning = buttonIntention;
            buttonIntention.RelatedTileMeaning.RelatedTilePosition = new Point(tile.X, tile.Y);
            grid.Positions[tile.X, tile.Y].Intentions.Add(buttonIntention);

            //remove walkable , nondynamic, box paths
            tile.Intentions = tile.Intentions.Where(x => x.Meaning != Meaning.Walkable && x.Meaning != Meaning.NonDynamnic && x.Meaning != Meaning.BoxPath && x.Meaning != Meaning.BoxPathVertical).ToList();


            DebugPrintMeaning(grid, Meaning.Button);

            return true;

        }

        public static void CreateToggleExitDoorButton(IntentionGrid grid, Random random)
        {
            //get a humble GroundLevel Tile
            var exitTiles = grid.GetByMeaning(Meaning.ExitPath).Where(t => 
                grid.Positions[t.X, t.Y].Intentions.Any(i => 
                    i.Meaning == Meaning.ToggleDoor 
                    && (
                        i.Info == null 
                        || 
                        !i.Info.Contains("LockId")
                    )
                )
            ).ToList();
            
            foreach (var exitTile in exitTiles)
            {
                var minCirc = grid.GetByMeaning(Meaning.Circuit).Min(x => x.X);
                var maxCirc = grid.GetByMeaning(Meaning.Circuit).Max(x => x.X);
                var tile = grid.GetByMeaning(Meaning.GroundLevel).Union(grid.GetByMeaning(Meaning.Circuit)).Where(t => t.X > minCirc && t.X < maxCirc && OnlyContainsCircuitAndGround(grid.Positions[t.X, t.Y].Intentions)).GetRandomOrDefault(random);
                if (tile == null)
                    return;
                var buttonIntention = Intention.ButtonIntention();
                buttonIntention.Info = "toggle";

                var exit = exitTile.Intentions.Where(i => i.Meaning == Meaning.ToggleDoor).FirstOrDefault();

                buttonIntention.RelatedTileMeaning = exit;
                buttonIntention.RelatedTilePosition = new Point(
                    exitTile.X,
                    exitTile.Y
                    //grid.GetByMeaning(Meaning.ExitPath).Where(t => grid.Positions[exitTile.X, t.Y].Intentions.Any(i => i.Meaning == Meaning.ToggleDoor)).First().X,
                    //grid.GetByMeaning(Meaning.ExitPath).Where(t => grid.Positions[t.X, t.Y].Intentions.Any(i => i.Meaning == Meaning.ToggleDoor)).First().Y
                );
                exit.RelatedTileMeaning = buttonIntention;
                exit.RelatedTilePosition = new Point(tile.X, tile.Y);
                grid.Positions[tile.X, tile.Y].Intentions.Add(buttonIntention);
            }
            DebugPrintMeaning(grid, Meaning.Button);
        }

        public static void CreateRopeSection(IntentionGrid grid, Random random)
        {

            //get a humble groundlevel tile
            var tiles = grid.GetByMeaning(Meaning.GroundLevel).Where(t => 
                grid.Positions[t.X, t.Y].Intentions.Count == 1
            ).ToList();


            //find 3 in a row
            var middle = tiles.Where(t => tiles.Any(tleft => tleft.Y == t.Y && tleft.X == t.X - 1) && tiles.Any(tright => tright.Y == t.Y && tright.X == t.X + 1)
                && grid.Positions[t.X, t.Y -1].Intentions.Count == 0
            ).GetRandomOrDefault(random);
            if (middle != null)
            {
                grid.Positions[middle.X, middle.Y].Intentions.Add(Intention.RopeIntention());
                //grid.Positions[middle.X, middle.Y-1].Intentions.Add(Intention.RopeIntention());
            }

            DebugPrintMeaning(grid, Meaning.Rope);
        }

        public static bool OnlyContainsCircuitAndGround(List<Intention> intentions)
        {
            if (intentions.Count == 0)
                return true;
            if (intentions.Count(x => x.Meaning == Meaning.Circuit || x.Meaning == Meaning.GroundLevel) == intentions.Count)
                return true;

            return false;
        }

        //big
        public static void CreateRopeCube(IntentionGrid grid, Random random)
        {
            var bottomCircuit = grid.GetByMeaning(Meaning.Circuit).OrderBy(x => x.Y).Select(x => x.Y).LastOrDefault();
            var trChance = .025f;
            var brChance = .025f;
            var tlChance = .025f;
            var blChance = .025f;
            //get a humble groundlevel tile
            var tile = grid.GetByMeaning(Meaning.GroundLevel).Union(grid.GetByMeaning(null)).Where(t =>
                t.X >= 2 && t.Y > 1 && t.X <= grid.Width - 3 && t.Y <= grid.Height - 2
                && t.Y < bottomCircuit
                && OnlyContainsCircuitAndGround(grid.Positions[t.X, t.Y].Intentions)
                && OnlyContainsCircuitAndGround(grid.Positions[t.X -1, t.Y].Intentions)
                && OnlyContainsCircuitAndGround(grid.Positions[t.X + 1, t.Y].Intentions)
                && OnlyContainsCircuitAndGround(grid.Positions[t.X - 2, t.Y].Intentions)
                && OnlyContainsCircuitAndGround(grid.Positions[t.X + 2, t.Y].Intentions)
                && grid.Positions[t.X-1, t.Y -1].Intentions.Count == 0
                && grid.Positions[t.X, t.Y -1].Intentions.Count == 0
                && !grid.Positions[t.X, t.Y -2].Intentions.Any(m=>m.Meaning == Meaning.Empty)
                && grid.Positions[t.X + 1, t.Y - 1].Intentions.Count == 0
                && grid.Positions[t.X - 1, t.Y + 1].Intentions.Count == 0
                && grid.Positions[t.X, t.Y + 1].Intentions.Count == 0
                && grid.Positions[t.X + 1, t.Y + 1].Intentions.Count == 0

            ).GetRandomOrDefault(random);

           

            if (tile==null)
                return;

            if(random.NextDouble() > trChance)
            {
                grid.Positions[tile.X+1, tile.Y - 1].Intentions.Add(Intention.EmptyIntention());
            }
            else
            {
                grid.Positions[tile.X + 1, tile.Y - 1].Intentions.Add(Intention.SolidIntention());
            }

            if (random.NextDouble() > tlChance)
            {
                grid.Positions[tile.X - 1, tile.Y - 1].Intentions.Add(Intention.EmptyIntention());
            }
            else
            {
                grid.Positions[tile.X - 1, tile.Y - 1].Intentions.Add(Intention.SolidIntention());
            }

            if (random.NextDouble() > brChance)
            {
                //grid.Positions[tile.X + 1, tile.Y + 1].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[tile.X + 1, tile.Y].Intentions.Add(Intention.LedgeIntention()); //MAY NOT EVEN HAVE A THING
            }
            else
            {
                grid.Positions[tile.X + 1, tile.Y].Intentions.Add(Intention.WalkableIntention());
            }

            if (random.NextDouble() > blChance)
            {
                //grid.Positions[tile.X - 1, tile.Y + 1].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[tile.X - 1, tile.Y].Intentions.Add(Intention.LedgeIntention());
            }
            else
            {
                grid.Positions[tile.X - 1, tile.Y].Intentions.Add(Intention.WalkableIntention());
            }

            grid.Positions[tile.X, tile.Y].Intentions.Add(Intention.RopeIntention());

            DebugPrintMeaning(grid, Meaning.Rope);
        }

        public static void CreateShooterSection(IntentionGrid grid, Random random)
        {
            //counts on having a border of width of 1

            //get a humble groundlevel tile at edge
            var tile = grid.GetByMeaning(Meaning.GroundLevel).Where(t =>
                (t.X == grid.Width - 2 || t.X == 1)
                && grid.Positions[t.X, t.Y].Intentions.Count == 1
            )
            //not next to exit or entrance path
            .Where(t =>
                !grid.Positions[t.X + 1, t.Y].Intentions.Any(n => n.Meaning == Meaning.EntrancePath || n.Meaning == Meaning.ExitPath)
                &&
                !grid.Positions[t.X - 1, t.Y].Intentions.Any(n => n.Meaning == Meaning.EntrancePath || n.Meaning == Meaning.ExitPath)
            )
            .ToList().GetRandomOrDefault(random);

            if (tile != null)
            {
                grid.Positions[tile.X, tile.Y].Intentions.Clear();
                grid.Positions[tile.X, tile.Y].Intentions.Add(Intention.ShooterIntention());
            }

            DebugPrintMeaning(grid, Meaning.Shooter);
        }

        public static void CreateEnemy(IntentionGrid grid, Random random)
        {
            var entrance = grid.GetByMeaning(Meaning.EntrancePath).FirstOrDefault();
            if (entrance == null)
                return;
            //get humble ground level/circuit furthest from entrance
            var tile = grid.GetByMeaning(Meaning.GroundLevel).Where(t =>
               grid.Positions[t.X, t.Y].Intentions.Count == 1 
               || (
                    grid.Positions[t.X, t.Y].Intentions.Count == 2 
                    && grid.Positions[t.X, t.Y].Intentions.Any(i=>i.Meaning == Meaning.Circuit)
               )
            ).OrderByDescending(t => Math.Abs(t.X - entrance.X) + Math.Abs(t.Y - entrance.Y)).FirstOrDefault();

            if (tile == null)
                return;
                /*throw new Exception("No room for enemy!");*/

            grid.Positions[tile.X, tile.Y].Intentions.Clear();
            grid.Positions[tile.X, tile.Y].Intentions.Add(Intention.EnemyIntention());
        }

        //big
        public static void CreateBoxButtonSection(IntentionGrid grid, Random random)
        {
            var button = grid.GetByMeaning(Meaning.Button).Where(x=>!grid.Positions[x.X, x.Y].Intentions.Any(m=>m.Meaning == Meaning.BoxPath)).GetRandomOrDefault(random);
            if (button == null)
                return;

            var groundLevelYs = grid.GetByMeaning(Meaning.GroundLevel).Select(gl => gl.Y).Distinct();

            var overAllAttempts = 0;
            var path = new List<Point>();

            while (true)
            {
                path = new List<Point>() { new Point(button.X, button.Y) };

                var tryGoDirection = new Point(0, 0);
                int tryGoDirectionIndex = -1;
                var tryGoDirectionOrder = (new List<Point>() { new Point(1, 0), new Point(-1, 0), new Point(0, -1) }).Shuffle(random);
                var oldTryGoDirection = new Point(0, 0);
                var attempts = 0;
                while (true)
                {
                    
                    tryGoDirectionIndex += 1;
                    tryGoDirectionIndex = tryGoDirectionIndex % tryGoDirectionOrder.Count;
                    tryGoDirection = tryGoDirectionOrder[tryGoDirectionIndex];
                    if (oldTryGoDirection.X != 0 && tryGoDirection.X != 0 && tryGoDirection.X != oldTryGoDirection.X)
                    {
                        tryGoDirection = oldTryGoDirection;
                        continue; //doesn't count for attempts
                    }
                    //if try go horizontal and at ground level and no additional meanings at try go
                    if (
                        (tryGoDirection.X == 1 || tryGoDirection.X == -1)
                        &&
                        path.Last().X + tryGoDirection.X > 0 && path.Last().X + tryGoDirection.X < grid.Width - 1
                        &&
                        !(
                            /*grid.Positions[path.Last().X + tryGoDirection.X, path.Last().Y + oldTryGoDirection.Y].Intentions.Any(i => i.Meaning == Meaning.Circuit)//can this be removed?
                            &&*/
                            grid.Positions[path.Last().X + tryGoDirection.X, path.Last().Y + oldTryGoDirection.Y].Intentions.Any(i => i.Meaning == Meaning.Ladder)
                        )
                        &&
                        !grid.Positions[path.Last().X + tryGoDirection.X, path.Last().Y + oldTryGoDirection.Y].Intentions.Any(i => i.Meaning == Meaning.VTraversable)
                        &&
                        groundLevelYs.Any(gl => gl == path.Last().Y)
                        &&
                        //list of allowed coexisting meanings:
                        !grid.Positions[path.Last().X + tryGoDirection.X, path.Last().Y].Intentions.Any(i =>
                            i.Meaning != Meaning.GroundLevel
                            && i.Meaning != Meaning.Circuit
                            && i.Meaning != Meaning.Walkable
                            && i.Meaning != Meaning.ShooterAlley
                            && i.Meaning != Meaning.Loot
                            && i.Meaning != Meaning.Key
                            && i.Meaning != Meaning.HiddenPosition
                            && i.Meaning != Meaning.Enemy
                            && i.Meaning != Meaning.Button
                            && i.Meaning != Meaning.BoxPath
                            && i.Meaning != Meaning.Box
                            && i.Meaning != Meaning.Ability)
                    )
                    {
                        attempts = -1;
                        oldTryGoDirection = tryGoDirection;
                        path.Add(new Point(path.Last().X + tryGoDirection.X, path.Last().Y));
                    }

                    else if//if not going horizontal and no ladders next to current and no ladders next to next and there is a ground level above us
                    (
                        tryGoDirection.X == 0
                        &&
                        path.Last().Y + tryGoDirection.Y > 0 && path.Last().Y + tryGoDirection.Y < grid.Height - 1
                        /*&&
                        !grid.Positions[path.Last().X + tryGoDirection.X, path.Last().Y + oldTryGoDirection.Y].Intentions.Any(i=>i.Meaning == Meaning.Circuit)
                        */
                        &&
                        !grid.Positions[path.Last().X + 1, path.Last().Y].Intentions.Any(i => i.Meaning == Meaning.Ladder)
                        &&
                        !grid.Positions[path.Last().X - 1, path.Last().Y].Intentions.Any(i => i.Meaning == Meaning.Ladder)
                        &&
                        !grid.Positions[path.Last().X + 1, path.Last().Y + tryGoDirection.Y].Intentions.Any(i => i.Meaning == Meaning.Ladder)
                        &&
                        !grid.Positions[path.Last().X - 1, path.Last().Y + tryGoDirection.Y].Intentions.Any(i => i.Meaning == Meaning.Ladder)
                        &&
                        groundLevelYs.Any(gl => gl < path.Last().Y)
                        &&
                        //list of accetable intentions
                        !grid.Positions[path.Last().X, path.Last().Y + tryGoDirection.Y].Intentions.Any(m=>
                            m.Meaning != Meaning.GroundLevel 
                            && m.Meaning != Meaning.Circuit 
                            && m.Meaning != Meaning.BoxPath 
                            && m.Meaning != Meaning.BoxPathVertical 
                            && m.Meaning != Meaning.Empty 
                            && m.Meaning != Meaning.PassThrough 
                            && m.Meaning != Meaning.VTraversable
                        )
                    )
                    {
                        attempts = -1;
                        oldTryGoDirection = tryGoDirection;
                        path.Add(new Point(path.Last().X + tryGoDirection.X, path.Last().Y + tryGoDirection.Y));
                    }
                    else
                    {//what is this case about
                        tryGoDirection = oldTryGoDirection;
                    }
                    attempts += 1;
                    if (
                        //long success
                        (path.Count() >= 9 && path.Last().Y == path[path.Count - 2].Y)
                        ||
                        //attempt failure maybe
                        attempts > 3)
                        break;
                }
                overAllAttempts += 1;
                if (
                    (path.Count >= 3 && path.Last().Y == path[path.Count - 2].Y)
                    ||
                    //attempt failure maybe
                    overAllAttempts >= 10
                )
                    break;
                
            }

            while(path.Count >=3 && 
                (path.Last().Y != path[path.Count - 2].Y 
                || grid.Positions[path.Last().X, path.Last().Y].Intentions.Any(m=>m.Meaning == Meaning.Button))
            )
            {
                path.RemoveAt(path.Count - 1);
            }

            if (path.Count < 3)
                return; // :(
            if (path.Last().Y != path[path.Count - 2].Y)
                return; // :(

            Point? prev = null;
            Point? next = null;
            var isVert = false;
            var isHoriz = true;
            for (var i = 0; i < path.Count; i++)
            {
                prev = null;
                next = null;
                //isVert = false;
                var wasVert = isVert;
                if (i > 0)
                    prev = path[i - 1];
                if (i + 1 <= path.Count - 1)
                    next = path[i + 1];

                if(
                    //(isVert == true && prev != null && prev.Value.Y != path[i].Y)
                    //||
                    (prev != null && prev.Value.Y != path[i].Y)
                )
                {
                    isVert = true;
                }
                else
                {
                    isVert = false;
                }

                if (prev == null || prev.Value.Y == path[i].Y)
                    isHoriz = true;
                else
                    isHoriz = false;
                string pathdir = "down";
                if(i > 0 && path[i-1].Y == path[i].Y)
                {
                    if (path[i - 1].X < path[i].X)
                        pathdir = "left";
                    else
                        pathdir = "right";
                }
                if (isVert && /*wasVert*/true)
                    grid.Positions[path[i].X, path[i].Y].Intentions.Add(Intention.BoxPathVerticalIntention());
                if(isHoriz)
                    grid.Positions[path[i].X, path[i].Y].Intentions.Add(Intention.WalkableIntention());

                grid.Positions[path[i].X, path[i].Y].Intentions.Add(Intention.BoxPathIntention(pathdir));
                

                if (i == path.Count -1)
                    grid.Positions[path[i].X, path[i].Y].Intentions.Add(Intention.BoxIntention());
                else if(i != 0 && pathdir == "down")
                    grid.Positions[path[i].X, path[i].Y].Intentions.Add(Intention.NonDynamic());
            }

            button.Intentions.Where(x => x.Meaning == Meaning.Button).All(b => { b.Info = "momentary"; return true;});

            DebugPrintMeaning(grid, Meaning.Walkable);
            DebugPrintMeaning(grid, Meaning.BoxPathVertical);            
            DebugPrintMeaning(grid, Meaning.BoxPath);
            DebugPrintMeaning(grid, Meaning.Box);
        }

        public static void CreateBorder(IntentionGrid grid, Random random)
        {
            var groundLevelMax = grid.GetByMeaning(Meaning.GroundLevel).Select(t => t.Y).Max();
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (

                        (x == 0 || y == 0 || x == grid.Width - 1 || y > groundLevelMax)
                        &&
                        grid.Positions[x, y].Intentions.Count() == 0
                    )
                    {
                        grid.Positions[x, y].Intentions.Add(Intention.SolidIntention());
                    }
                }
            }

            DebugPrintMeaning(grid, Meaning.Solid);
        }

        public static void AddSolidToSidesOfCircuit(IntentionGrid grid, Random random)
        {
            var circuitMaxX = grid.GetByMeaning(Meaning.Circuit).Select(t => t.X).Max();
            var circuitMinX = grid.GetByMeaning(Meaning.Circuit).Select(t => t.X).Min();
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (
                        (x < circuitMinX || x > circuitMaxX)
                        &&
                        grid.Positions[x, y].Intentions.Count == 0
                    )
                        grid.Positions[x, y].Intentions.Add(Intention.SolidIntention());
                }
            }
            DebugPrintMeaning(grid, Meaning.Solid);
        }

        //big
        public static void AddConveyorJump(IntentionGrid grid, Random random)
        {
            //WARNING: REMOVES GROUND LEVELS FOR BIG JUMPS
            var bottomCircuit = grid.GetByMeaning(Meaning.Circuit).Max(m => m.Y);
            var tile = grid.GetByMeaning(Meaning.GroundLevel).Union(grid.GetByMeaning(null))
                .Where(t =>
                    t.X > 3 && t.X < grid.Width - 3 && t.Y > 2 && t.Y < bottomCircuit
                    && OnlyContainsCircuitAndGround(grid.Positions[t.X, t.Y].Intentions)
                    && grid.Positions[t.X, t.Y - 1].Intentions.Count == 0
                    && grid.Positions[t.X, t.Y + 1].Intentions.Count == 0
                );
            bool cangoright = false;
            bool cangoleft = false;
            if(tile.Any(t=>   
                OnlyContainsCircuitAndGround(grid.Positions[t.X + 1, t.Y].Intentions)
                && OnlyContainsCircuitAndGround(grid.Positions[t.X + 2, t.Y].Intentions)
                && grid.Positions[t.X + 1, t.Y - 1].Intentions.Count == 0
                && grid.Positions[t.X + 2, t.Y - 1].Intentions.Count == 0
            ))
            {
                cangoright = true;
            }
            if(tile.Any(t=>
                OnlyContainsCircuitAndGround(grid.Positions[t.X - 1, t.Y].Intentions)
                && OnlyContainsCircuitAndGround(grid.Positions[t.X - 2, t.Y].Intentions)
                && grid.Positions[t.X - 1, t.Y - 1].Intentions.Count == 0
                 && grid.Positions[t.X - 2, t.Y - 1].Intentions.Count == 0
            ))
            {
                cangoleft = true;
            }

            if(!cangoright && !cangoleft)
            {
                return;
            }
            if(cangoleft && cangoright)
            {
                if (random.NextDouble() < .5f)
                    cangoleft = false;
                else
                    cangoright = false;
            }

            IntentionTiles theTile = null;
            if(cangoright)
            {
                theTile = tile.Where(t =>
                    OnlyContainsCircuitAndGround(grid.Positions[t.X + 1, t.Y].Intentions)
                    && OnlyContainsCircuitAndGround(grid.Positions[t.X + 2, t.Y].Intentions)
                    && grid.Positions[t.X + 1, t.Y - 1].Intentions.Count == 0
                    && grid.Positions[t.X + 2, t.Y - 1].Intentions.Count == 0
                ).GetRandomOrDefault(random);

                theTile.Intentions.Add(Intention.ConveyorIntention("right"));
                grid.Positions[theTile.X + 1, theTile.Y].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X + 2, theTile.Y].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X + 1, theTile.Y - 1].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X + 2, theTile.Y - 1].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X + 1, theTile.Y].Intentions = grid.Positions[theTile.X + 1, theTile.Y].Intentions.Where(i => i.Meaning != Meaning.GroundLevel).ToList();
                grid.Positions[theTile.X + 2, theTile.Y].Intentions = grid.Positions[theTile.X + 2, theTile.Y].Intentions.Where(i => i.Meaning != Meaning.GroundLevel).ToList();
            }
            else
            {
                theTile = tile.Where(t =>
                    OnlyContainsCircuitAndGround(grid.Positions[t.X - 1, t.Y].Intentions)
                    && OnlyContainsCircuitAndGround(grid.Positions[t.X - 2, t.Y].Intentions)
                    && grid.Positions[t.X - 1, t.Y - 1].Intentions.Count == 0
                    && grid.Positions[t.X - 2, t.Y - 1].Intentions.Count == 0
                ).GetRandomOrDefault(random);

                theTile.Intentions.Add(Intention.ConveyorIntention("left"));
                grid.Positions[theTile.X - 1, theTile.Y].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X - 2, theTile.Y].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X - 1, theTile.Y - 1].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X - 2, theTile.Y - 1].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X - 1, theTile.Y].Intentions = grid.Positions[theTile.X - 1, theTile.Y].Intentions.Where(i => i.Meaning != Meaning.GroundLevel).ToList();
                grid.Positions[theTile.X - 2, theTile.Y].Intentions = grid.Positions[theTile.X - 2, theTile.Y].Intentions.Where(i => i.Meaning != Meaning.GroundLevel).ToList();
            }
        }

        //really big
        public static void AddConveyorJumpToHiddenPlace(IntentionGrid grid, Random random)
        {
            //WARNING: REMOVES GROUND LEVELS FOR BIG JUMPS
            var bottomCircuit = grid.GetByMeaning(Meaning.Circuit).Max(m => m.Y);
            var tile = grid.GetByMeaning(Meaning.GroundLevel).Union(grid.GetByMeaning(null))
                .Where(t =>
                    t.X > 3 && t.X < grid.Width - 3 && t.Y > 2 && t.Y < bottomCircuit
                    && OnlyContainsCircuitAndGround(grid.Positions[t.X, t.Y].Intentions)
                    && grid.Positions[t.X, t.Y - 1].Intentions.Count == 0
                    && grid.Positions[t.X, t.Y + 1].Intentions.Count == 0
                );
            bool cangoright = false;
            bool cangoleft = false;
            if (tile.Any(t =>
                 OnlyContainsCircuitAndGround(grid.Positions[t.X + 1, t.Y].Intentions)
                 && OnlyContainsCircuitAndGround(grid.Positions[t.X + 2, t.Y].Intentions)
                 && OnlyContainsCircuitAndGround(grid.Positions[t.X + 3, t.Y].Intentions)
                 && grid.Positions[t.X + 1, t.Y-1].Intentions.Count == 0
                 && grid.Positions[t.X + 2, t.Y-1].Intentions.Count == 0
                 && OnlyContainsCircuitAndGround(grid.Positions[t.X + 4, t.Y].Intentions)
                 && grid.Positions[t.X + 3, t.Y-1].Intentions.Count == 0
                 && !grid.Positions[t.X + 4, t.Y + 1].Intentions.Any(m=>m.Meaning == Meaning.VerticalExitWay || m.Meaning == Meaning.VTraversable || m.Meaning == Meaning.Ladder)
                 && !grid.Positions[t.X + 3, t.Y - 2].Intentions.Any(m => m.Meaning == Meaning.Ledge)
            ))
            {
                cangoright = true;
            }
            if (tile.Any(t =>
                 OnlyContainsCircuitAndGround(grid.Positions[t.X - 1, t.Y].Intentions)
                 && OnlyContainsCircuitAndGround(grid.Positions[t.X - 2, t.Y].Intentions)
                 && OnlyContainsCircuitAndGround(grid.Positions[t.X - 3, t.Y].Intentions)
                 && grid.Positions[t.X - 1, t.Y - 1].Intentions.Count == 0
                 && grid.Positions[t.X - 2, t.Y - 1].Intentions.Count == 0
                 && OnlyContainsCircuitAndGround(grid.Positions[t.X - 4, t.Y].Intentions)
                 && grid.Positions[t.X - 3, t.Y - 1].Intentions.Count == 0
                 && !grid.Positions[t.X - 4, t.Y + 1].Intentions.Any(m => m.Meaning == Meaning.VerticalExitWay || m.Meaning == Meaning.VTraversable || m.Meaning == Meaning.Ladder)
                 && !grid.Positions[t.X - 3, t.Y - 2].Intentions.Any(m => m.Meaning == Meaning.Ledge)
            ))
            {
                cangoleft = true;
            }

            if (!cangoright && !cangoleft)
            {
                return;
            }
            if (cangoleft && cangoright)
            {
                if (random.NextDouble() < .5f)
                    cangoleft = false;
                else
                    cangoright = false;
            }

            IntentionTiles theTile = null;
            
            if (cangoright)
            {
                theTile = tile.Where(t =>
                    OnlyContainsCircuitAndGround(grid.Positions[t.X + 1, t.Y].Intentions)
                    && OnlyContainsCircuitAndGround(grid.Positions[t.X + 2, t.Y].Intentions)
                    && OnlyContainsCircuitAndGround(grid.Positions[t.X + 3, t.Y].Intentions)
                    && grid.Positions[t.X + 1, t.Y - 1].Intentions.Count == 0
                    && grid.Positions[t.X + 2, t.Y - 1].Intentions.Count == 0
                    && OnlyContainsCircuitAndGround(grid.Positions[t.X + 4, t.Y].Intentions)
                    && grid.Positions[t.X + 3, t.Y - 1].Intentions.Count == 0
                    && !grid.Positions[t.X + 4, t.Y + 1].Intentions.Any(m => m.Meaning == Meaning.VerticalExitWay || m.Meaning == Meaning.VTraversable || m.Meaning == Meaning.Ladder)
                    && !grid.Positions[t.X + 3, t.Y - 2].Intentions.Any(m => m.Meaning == Meaning.Ledge)
               ).GetRandomOrDefault(random);

                theTile.Intentions.Add(Intention.ConveyorIntention("right"));
                grid.Positions[theTile.X + 1, theTile.Y].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X + 2, theTile.Y].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X + 1, theTile.Y - 1].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X + 2, theTile.Y - 1].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X + 4, theTile.Y].Intentions.Add(Intention.SolidIntention());
                grid.Positions[theTile.X + 3, theTile.Y - 1].Intentions.Add(Intention.SolidIntention());
                grid.Positions[theTile.X + 3, theTile.Y].Intentions.Add(Intention.HiddenIntention());
                grid.Positions[theTile.X + 3, theTile.Y].Intentions.Add(Intention.WalkableIntention());
                grid.Positions[theTile.X + 1, theTile.Y].Intentions = grid.Positions[theTile.X + 1, theTile.Y].Intentions.Where(i => i.Meaning != Meaning.GroundLevel).ToList();
                grid.Positions[theTile.X + 2, theTile.Y].Intentions = grid.Positions[theTile.X + 2, theTile.Y].Intentions.Where(i => i.Meaning != Meaning.GroundLevel).ToList();
                grid.Positions[theTile.X + 4, theTile.Y].Intentions = grid.Positions[theTile.X + 4, theTile.Y].Intentions.Where(i => i.Meaning != Meaning.GroundLevel).ToList();
            }
            else
            {
                theTile = tile.Where(t =>
                    OnlyContainsCircuitAndGround(grid.Positions[t.X - 1, t.Y].Intentions)
                    && OnlyContainsCircuitAndGround(grid.Positions[t.X - 2, t.Y].Intentions)
                    && OnlyContainsCircuitAndGround(grid.Positions[t.X - 3, t.Y].Intentions)
                    && grid.Positions[t.X - 1, t.Y - 1].Intentions.Count == 0
                    && grid.Positions[t.X - 2, t.Y - 1].Intentions.Count == 0
                    && OnlyContainsCircuitAndGround(grid.Positions[t.X - 4, t.Y].Intentions)
                    && grid.Positions[t.X - 3, t.Y - 1].Intentions.Count == 0
                    && !grid.Positions[t.X - 4, t.Y + 1].Intentions.Any(m => m.Meaning == Meaning.VerticalExitWay || m.Meaning == Meaning.VTraversable || m.Meaning == Meaning.Ladder)
                    && !grid.Positions[t.X - 3, t.Y - 2].Intentions.Any(m => m.Meaning == Meaning.Ledge)
                ).GetRandomOrDefault(random);

                theTile.Intentions.Add(Intention.ConveyorIntention("left"));
                grid.Positions[theTile.X - 1, theTile.Y].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X - 2, theTile.Y].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X - 1, theTile.Y - 1].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X - 2, theTile.Y - 1].Intentions.Add(Intention.EmptyIntention());
                grid.Positions[theTile.X - 4, theTile.Y].Intentions.Add(Intention.SolidIntention());
                grid.Positions[theTile.X - 3, theTile.Y - 1].Intentions.Add(Intention.SolidIntention());
                grid.Positions[theTile.X - 3, theTile.Y].Intentions.Add(Intention.HiddenIntention());
                grid.Positions[theTile.X - 3, theTile.Y].Intentions.Add(Intention.WalkableIntention());
                grid.Positions[theTile.X - 1, theTile.Y].Intentions = grid.Positions[theTile.X - 1, theTile.Y].Intentions.Where(i => i.Meaning != Meaning.GroundLevel).ToList();
                grid.Positions[theTile.X - 2, theTile.Y].Intentions = grid.Positions[theTile.X - 2, theTile.Y].Intentions.Where(i => i.Meaning != Meaning.GroundLevel).ToList();
                grid.Positions[theTile.X - 4, theTile.Y].Intentions = grid.Positions[theTile.X - 4, theTile.Y].Intentions.Where(i => i.Meaning != Meaning.GroundLevel).ToList();
            }
        }

        public static void AddConveyor(IntentionGrid grid, Random random)
        {
            var bottomCircuit = grid.GetByMeaning(Meaning.Circuit).Max(m => m.Y);
            var tile = grid.GetByMeaning(Meaning.GroundLevel).Union(grid.GetByMeaning(null))
                .Where(t =>
                    t.X > 2 && t.X < grid.Width - 2 && t.Y > 2 && t.Y <= bottomCircuit
                    && OnlyContainsCircuitAndGround(grid.Positions[t.X, t.Y].Intentions)
                    && grid.Positions[t.X, t.Y - 1].Intentions.Count == 0
                    && grid.Positions[t.X, t.Y + 1].Intentions.Count == 0
                ).GetRandomOrDefault(random);

            if (tile == null)
                return;
            //var dir = "right";
            //if (random.NextDouble() >= .5d)
            //    dir = "left";
            tile.Intentions.Add(Intention.ConveyorIntention(""));
        }

        
        public static void AddConveyorToBoxPath(IntentionGrid grid, Random random)
        {
            var tiles = grid.GetByMeaning(Meaning.BoxPath)
                .Where(x =>
                    x.Intentions.Any(i => i.Meaning == Meaning.GroundLevel)
                    &&
                    x.Intentions.Any(i => i.Meaning == Meaning.Walkable)
                    &&
                    !x.Intentions.Any(i => i.Meaning == Meaning.Box)
                    &&
                    !x.Intentions.Any(i => i.Meaning == Meaning.Conveyor)
                    &&
                    !x.Intentions.Any(i => i.Meaning == Meaning.Button)
                /*&& 
                x.Intentions.Count == 3*/
                );
           var tile = tiles.GetRandomOrDefault(random);

            if (tile == null)
                return;

            

            string dir = tile.Intentions.First(x => x.Meaning == Meaning.BoxPath).Info;
            /*var dir = "right";
            if (random.NextDouble() >= .5d)
                dir = "left";*/

            tile.Intentions.Add(Intention.ConveyorIntention(dir));
            //remove box path, walkable, non dynamic
            tile.Intentions = tile.Intentions.Where(x => x.Meaning != Meaning.Walkable && x.Meaning != Meaning.NonDynamnic && x.Meaning != Meaning.BoxPath && x.Meaning != Meaning.BoxPathVertical).ToList();
            //had non dynamic removal heree before above line was added
        }

        public static void AddConveyorToggleButton(IntentionGrid grid, Random random)
        {
            IntentionTiles conveyorTilePos = grid.GetByMeaning(Meaning.Conveyor)
                .Where(x =>
                    x.Intentions.Where(i=>i.Meaning == Meaning.Conveyor).First().RelatedTileMeaning == null
                    &&
                    !grid.Positions[x.X - 1, x.Y].Intentions.Any(m => m.Meaning == Meaning.Conveyor)
                ).GetRandomOrDefault(random);
            if (conveyorTilePos == null)
                return;
            
            Intention conveyorIntention = conveyorTilePos.Intentions.Where(x => x.Meaning == Meaning.Conveyor).First();

            IntentionTiles buttonPos = grid.GetByMeaning(Meaning.GroundLevel)
                .Where(x => 
                    OnlyContainsCircuitAndGround(x.Intentions)
                )
                .GetRandomOrDefault(random);

            if (buttonPos == null)
                return;

            var buttonIntention = Intention.ButtonIntention();
            buttonIntention.Info = "toggle";
            buttonIntention.RelatedTileMeaning = conveyorIntention;
            buttonIntention.RelatedTilePosition = new Point(conveyorTilePos.X, conveyorTilePos.Y);
            buttonIntention.RelatedTileMeaning.RelatedTileMeaning = buttonIntention;
            buttonIntention.RelatedTileMeaning.RelatedTilePosition = new Point(buttonPos.X, buttonPos.Y);
            grid.Positions[buttonPos.X, buttonPos.Y].Intentions.Add(buttonIntention);
        }

        public static void AddShooterToggleButton(IntentionGrid grid, Random random)
        {
            IntentionTiles shooterTilePos = grid.GetByMeaning(Meaning.Shooter)
                .Where(x =>
                    x.Intentions.Where(i => i.Meaning == Meaning.Shooter).First().RelatedTileMeaning == null
                    &&
                    !grid.Positions[x.X - 1, x.Y].Intentions.Any(m => m.Meaning == Meaning.Shooter)
                ).GetRandomOrDefault(random);
            if (shooterTilePos == null)
                return;

            Intention shooterIntention = shooterTilePos.Intentions.Where(x => x.Meaning == Meaning.Shooter).First();

            IntentionTiles buttonPos = grid.GetByMeaning(Meaning.GroundLevel)
                .Where(x =>
                    OnlyContainsCircuitAndGround(x.Intentions)
                )
                .GetRandomOrDefault(random);

            if (buttonPos == null)
                return;

            var buttonIntention = Intention.ButtonIntention();
            buttonIntention.Info = "toggle";
            buttonIntention.RelatedTileMeaning = shooterIntention;
            buttonIntention.RelatedTilePosition = new Point(shooterTilePos.X, shooterTilePos.Y);
            buttonIntention.RelatedTileMeaning.RelatedTileMeaning = buttonIntention;
            buttonIntention.RelatedTileMeaning.RelatedTilePosition = new Point(buttonPos.X, buttonPos.Y);
            grid.Positions[buttonPos.X, buttonPos.Y].Intentions.Add(buttonIntention);
        }

        public static void LengthenExistingConveyor(IntentionGrid grid, Random random)
        {
            var tile = grid.GetByMeaning(Meaning.GroundLevel).Union(grid.GetByMeaning(null))
                .Where(t =>
                    t.X < grid.Width - 1 && t.Y < grid.Height - 1 && t.X > 0 && t.Y > 0 
                    &&
                    (
                        grid.Positions[t.X + 1, t.Y].Intentions.Any(i => i.Meaning == Meaning.Conveyor)
                        ||
                        grid.Positions[t.X - 1, t.Y].Intentions.Any(i => i.Meaning == Meaning.Conveyor)
                    )
                    &&
                    OnlyContainsCircuitAndGround(grid.Positions[t.X, t.Y].Intentions)
                ).GetRandomOrDefault(random);

            if (tile == null)
                return;

            string dir;
            if (grid.Positions[tile.X + 1, tile.Y].Intentions.Any(i => i.Meaning == Meaning.Conveyor))
                dir = grid.Positions[tile.X + 1, tile.Y].Intentions.Where(i => i.Meaning == Meaning.Conveyor).First().Info;
            else
                dir = grid.Positions[tile.X - 1, tile.Y].Intentions.Where(i => i.Meaning == Meaning.Conveyor).First().Info;
            tile.Intentions.Add(Intention.ConveyorIntention(dir));
        }

        public static void CompleteConveyors(IntentionGrid grid, Random random)
        {
            var tiles = grid.GetByMeaning(Meaning.Conveyor).Where(x =>
                x.Intentions.Where(i => i.Meaning == Meaning.Conveyor).First().Info == "");

            Intention boxTile = null;
            Intention influenceConveyor = null;
            foreach(var tile in tiles.OrderBy(x=>x.X))
            {
                boxTile = null;
                boxTile = grid.Positions[tile.X, tile.Y].Intentions.Where(a => a.Meaning == Meaning.BoxPath).FirstOrDefault();
                if(boxTile != null)
                {
                    tile.Intentions.Where(x => x.Meaning == Meaning.Conveyor).First().Info = boxTile.Info;
                    continue;
                }

                influenceConveyor = null;
                if(tile.X > 0)
                {
                    influenceConveyor = grid.Positions[tile.X - 1, tile.Y].Intentions.Where(a => a.Meaning == Meaning.Conveyor).FirstOrDefault();
                    if(influenceConveyor != null)
                    {
                        tile.Intentions.Where(x => x.Meaning == Meaning.Conveyor).First().Info = influenceConveyor.Info;
                        continue;
                    }
                }
                influenceConveyor = null;
                if (tile.X < grid.Width - 1)
                {
                    influenceConveyor = grid.Positions[tile.X + 1, tile.Y].Intentions.Where(a => a.Meaning == Meaning.Conveyor).FirstOrDefault();
                    if (influenceConveyor != null && influenceConveyor.Info != "")
                    {
                        tile.Intentions.Where(x => x.Meaning == Meaning.Conveyor).First().Info = influenceConveyor.Info;
                        continue;
                    }
                }

                if (random.NextDouble() < .5f)
                    tile.Intentions.Where(x => x.Meaning == Meaning.Conveyor).First().Info = "right";
                else
                    tile.Intentions.Where(x => x.Meaning == Meaning.Conveyor).First().Info = "left";

            }

           
        }

        public static void PlaceKey(IntentionGrid grid, Random random, int keyId)
        {
            IntentionTiles itemTile = GetFreeDeadEndRoom(grid, random);
            if(itemTile == null)
            {
                //anywhere
                itemTile = grid.GetByMeaning(Meaning.GroundLevel).Where(x => OnlyContainsCircuitAndGround(x.Intentions)).GetRandomOrDefault(random);
            }
            if(itemTile == null)
            {
                throw new Exception("No positions free!");
            }
            itemTile.Intentions.Add(Intention.KeyIntention(keyId));
        }

        public static void PlaceLoot(IntentionGrid grid, Random random)
        {
            IntentionTiles itemTile = GetFreeDeadEndRoom(grid, random);
            if (itemTile == null)
            {
                //anywhere
                itemTile = grid.GetByMeaning(Meaning.GroundLevel).Where(x => OnlyContainsCircuitAndGround(x.Intentions)).GetRandomOrDefault(random);
            }
            itemTile.Intentions.Add(Intention.LootIntention());
        }

        public static void PlaceAbility(IntentionGrid grid, Random random)
        {
            IntentionTiles itemTile = GetFreeDeadEndRoom(grid, random);
            if (itemTile == null)
            {
                //anywhere
                itemTile = grid.GetByMeaning(Meaning.GroundLevel).Where(x => OnlyContainsCircuitAndGround(x.Intentions)).GetRandomOrDefault(random);
            }
            itemTile.Intentions.Add(Intention.AbilityIntention());
        }

        private static IntentionTiles GetFreeDeadEndRoom(IntentionGrid grid, Random random)
        {
            int mincircX = grid.GetByMeaning(Meaning.Circuit).Min(i => i.X);
            int maxcircX = grid.GetByMeaning(Meaning.Circuit).Max(i => i.X);
            IEnumerable<IntentionTiles> xtraDoorTile = grid.GetByMeaning(Meaning.ToggleDoor).Where(x => x.X < mincircX && x.X > maxcircX && !x.Intentions.Any(i => i.Meaning == Meaning.ExitPath || i.Meaning == Meaning.EntrancePath));
            IntentionTiles itemTile = null;
            //annoying extra door deadends
            if (xtraDoorTile.Count() != 0)
            {
                IntentionTiles xtradoor = xtraDoorTile.GetRandomOrDefault(random);
                if (xtradoor.X > maxcircX)
                {
                    itemTile = grid.GetByMeaning(Meaning.GroundLevel).Where(x => (x.Y == xtradoor.Y) && (x.X > xtradoor.X) && x.Intentions.Count == 1).FirstOrDefault();
                }
                else
                {
                    //must be less than
                    itemTile = grid.GetByMeaning(Meaning.GroundLevel).Where(x => (x.Y == xtradoor.Y) && (x.X < xtradoor.X) && x.Intentions.Count == 1).FirstOrDefault();
                }
            }
            return itemTile;
        }

        public static void BlanketNonDynamic(IntentionGrid grid, Random random)
        {
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if (/*grid.Positions[x, y].Intentions.Count == 0 ||
                        (grid.Positions[x, y].Intentions.Count == 1 && grid.Positions[x, y].Intentions.Any(i => i.Meaning == Meaning.GroundLevel)
                        )*/
            !IntentionListHasDynamics(grid.Positions[x,y].Intentions)
                        &&
                        NotTerminalLadder(x,y,grid)
                    )
                    {
                        grid.Positions[x, y].Intentions.Add(Intention.NonDynamicStrict());
                    }
                }
            }

            DebugPrintMeaning(grid, Meaning.NonDynamicStrict);
        }

        public static void CleanUpBoxPathShit(IntentionGrid grid, Random random)
        {
            for(var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if(grid.Positions[x,y].Intentions.Any(i=>
                        i.Meaning == Meaning.Button ||
                        i.Meaning == Meaning.Conveyor ||
                        i.Meaning == Meaning.ToggleDoor
                     ))
                    {
                        //remove box path
                        grid.Positions[x, y].Intentions = grid.Positions[x, y].Intentions.Where(i =>
                              i.Meaning != Meaning.BoxPath
                              && i.Meaning != Meaning.NonDynamnic
                              && i.Meaning != Meaning.NonDynamicStrict
                              && i.Meaning != Meaning.Walkable
                              && i.Meaning != Meaning.BoxPathVertical
                        ).ToList();
                    } 
                }
            }
        }

        public static void DebugPrintMeaning(IntentionGrid grid, Meaning meaning)
        {
            //Debug
            Console.WriteLine(meaning.ToString() + ":  ");
            for (var y = 0; y < grid.Height; y++)
            {
                var str = "";
                for (var x = 0; x < grid.Width; x++)
                {

                    if (grid.Positions[x, y].Intentions.Any(i => i.Meaning == meaning))
                        str = str + "X";
                    else
                        str = str + "-";
                }
                Console.WriteLine(str);
            }
            Console.WriteLine("");
        }

        private static bool IntentionListHasDynamics(List<Intention> list)
        {//conveyor issue with all this ladder stuff
            return list.Any(intent =>
                intent.Meaning == Meaning.Box
                /*||
                intent.Meaning == Meaning.BoxPath*/
                ||
                intent.Meaning == Meaning.Button
                ||
                intent.Meaning == Meaning.Conveyor
                ||
                intent.Meaning == Meaning.Elevator
                ||
                (intent.Meaning == Meaning.Ladder)
                ||
                intent.Meaning == Meaning.Rope
                ||
                intent.Meaning == Meaning.Shooter
                ||
                intent.Meaning == Meaning.ToggleDoor
                ||
                intent.Meaning == Meaning.Enemy
                ||
                intent.Meaning == Meaning.VerticalExitWay
                
                );
        }

        private static bool NotTerminalLadder(int x, int y, IntentionGrid grid)
        {
            if (y - 1 >= 0 && (
                    grid.Positions[x, y - 1].Intentions.Any(i => i.Meaning == Meaning.Ladder)
                    ||
                    grid.Positions[x, y - 1].Intentions.Any(i => i.Meaning == Meaning.VerticalExitWay)
                )
            )
                return false;
            if (y + 1 < grid.Height && (
                    grid.Positions[x, y + 1].Intentions.Any(i => i.Meaning == Meaning.Ladder)
                    ||
                    grid.Positions[x, y + 1].Intentions.Any(i => i.Meaning == Meaning.VerticalExitWay)
                    ||
                    grid.Positions[x, y + 1].Intentions.Any(i => i.Meaning == Meaning.ToggleDoor)
                )
            )
                return false;

            if (y + 2 < grid.Height && (
                    /*grid.Positions[x, y + 2].Intentions.Any(i => i.Meaning == Meaning.Ladder)
                    ||*/
                    grid.Positions[x, y + 2].Intentions.Any(i => i.Meaning == Meaning.VerticalExitWay)
                )
            )
                return false;
            return true;
        }
    }


    public class RopeCubeCreator : CreatorAbstractor
    {
        public override Intention Execute(IntentionGrid grid, Random random, Intention inputIntention = null)
        {
            BasicCircuitCreators.CreateRopeCube(grid, random);
            return null;
        }
    }

    public class BoxButtonSectionCreator : CreatorAbstractor
    {
        public override Intention Execute(IntentionGrid grid, Random random, Intention inputIntention = null)
        {
            BasicCircuitCreators.CreateBoxButtonSection(grid, random);
            return null;
        }
    }

    public class ConveyorJumpCreator : CreatorAbstractor
    {
        public override Intention Execute(IntentionGrid grid, Random random, Intention inputIntention = null)
        {
            BasicCircuitCreators.AddConveyorJump(grid, random);
            return null;
        }
    }

    public class ConveyorJumpToHiddenPlaceCreator : CreatorAbstractor
    {
        public override Intention Execute(IntentionGrid grid, Random random, Intention inputIntention = null)
        {
            BasicCircuitCreators.AddConveyorJumpToHiddenPlace(grid, random);
            return null;
        }
    }


    public static class CreatorAbstractorFactory
    {
        public static List<CreatorAbstractor> GetAll(Random random)
        {
            if (random != null)
                return (new List<CreatorAbstractor>()
                {
                    new RopeCubeCreator(),
                    new BoxButtonSectionCreator(),
                    new ConveyorJumpToHiddenPlaceCreator(),
                }).Shuffle(random).ToList();
            else
                return (new List<CreatorAbstractor>()
                {
                    new RopeCubeCreator(),
                    new BoxButtonSectionCreator(),
                    new ConveyorJumpToHiddenPlaceCreator(),
                });
        }
    }
}
