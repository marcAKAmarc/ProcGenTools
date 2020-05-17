using PuzzleBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcGenTools.Helper;

namespace Test.Puzzle._01
{
    public static class Creators
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

        public static void CreateCircuit(IntentionGrid grid, Random random)
        {

            var groundLevels = grid.GetByMeaning(Meaning.GroundLevel).Select(x => x.Y).Distinct().ToArray();

            var verticals = new int[2];
            verticals[0] = (int)Math.Floor(grid.Width / 4.0f);
            verticals[1] = (int)Math.Floor(3.0f * grid.Width / 4.0f);



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
            DebugPrintMeaning(grid, Meaning.Circuit);
        }

        public static void CreateCircuitFractured(IntentionGrid grid, Random random)
        {
            var groundLevels = grid.GetByMeaning(Meaning.GroundLevel).Select(x => x.Y).Distinct().ToArray();

            var verticals = new int[2];
            verticals[0] = (int)Math.Floor(grid.Width / 4.0f);
            verticals[1] = (int)Math.Floor(3.0f * grid.Width / 4.0f);

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
            for (var y = 0; y < groundLevels.Max() -1; y++)
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
                    if(
                        (
                            y > 0
                            &&
                            bufferGrid[x,y-1]
                        )||(
                            y < grid.Height -1
                            &&
                            bufferGrid[x,y+1]
                        )
                    )
                    {
                        isLadder = true;
                    }

                    if(isLadder)
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
            }
            if (entranceY > circuits.Select(t => t.Y).Max() || entranceY < circuits.Select(t => t.Y).Min())
            {
                grid.Positions[xStop, entranceY].Intentions.Add(Intention.LadderIntention());
                grid.Positions[xStop, entranceY].Intentions.Add(Intention.NonDynamic());
            }
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
            }
            if (exitY > circuits.Select(t => t.Y).Max() || exitY < circuits.Select(t => t.Y).Min())
            {
                grid.Positions[xStop, exitY].Intentions.Add(Intention.LadderIntention());
                grid.Positions[xStop, exitY].Intentions.Add(Intention.NonDynamic());
            }

            DebugPrintMeaning(grid, Meaning.ExitPath);
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
                yStart = grid.Width - 1;
                yStop = bottomMost - 1;
            }

            for (var y = yStart; y != yStop; y += yDir)
            {
                grid.Positions[exitX, y].Intentions.Add(Intention.ExitPathIntention());
                if (y != yStop - yDir)//not the last one
                {
                    grid.Positions[exitX, y].Intentions.Add(Intention.VerticalExitIntention());
                }
            }

            DebugPrintMeaning(grid, Meaning.ExitPath);
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
                yStart = grid.Width - 1;
                yStop = bottomMost - 1;
            }

            for (var y = yStart; y != yStop; y += yDir)
            {
                grid.Positions[exitX, y].Intentions.Add(Intention.EntrancePathIntention());
                if(y != yStop- yDir)
                {
                    grid.Positions[exitX, y].Intentions.Add(Intention.VerticalExitIntention());
                }
            }

            DebugPrintMeaning(grid, Meaning.ExitPath);
        }

        public static void CreateToggleExitDoor(IntentionGrid grid)
        {
            var tile = grid.GetByMeaning(Meaning.ExitPath).Where(t => t.X == grid.Width - 1 || t.X == 0 || t.Y == 0 || t.Y == grid.Height - 1).FirstOrDefault();

            grid.Positions[tile.X, tile.Y].Intentions.Add(Intention.ToggleDoorIntention());

            DebugPrintMeaning(grid, Meaning.ToggleDoor);
        }

        public static void CreateToggleExitDoorButton(IntentionGrid grid, Random random)
        {
            //get a humble GroundLevel Tile
            var tile = grid.GetByMeaning(Meaning.GroundLevel).Where(t => grid.Positions[t.X, t.Y].Intentions.Count == 1).GetRandomOrDefault(random);
            var buttonIntention = Intention.ButtonIntention();
            buttonIntention.Info = "toggle";
            var exit = grid.GetByMeaning(Meaning.ExitPath).Where(t => grid.Positions[t.X, t.Y].Intentions.Any(i => i.Meaning == Meaning.ToggleDoor)).First().Intentions.Where(i => i.Meaning == Meaning.ToggleDoor).First();
            buttonIntention.RelatedTileMeaning = exit;
            exit.RelatedTileMeaning = buttonIntention;
            grid.Positions[tile.X, tile.Y].Intentions.Add(Intention.ButtonIntention());

            DebugPrintMeaning(grid, Meaning.Button);
        }

        public static void CreateRopeSection(IntentionGrid grid, Random random)
        {
            //TODO - RANDOMIZE THIS

            //get a humble groundlevel tile
            var tiles = grid.GetByMeaning(Meaning.GroundLevel).Where(t => grid.Positions[t.X, t.Y].Intentions.Count == 1).ToList();

            //find 3 in a row
            var middle = tiles.Where(t => tiles.Any(tleft => tleft.Y == t.Y && tleft.X == t.X - 1) && tiles.Any(tright => tright.Y == t.Y && tright.X == t.X + 1)).GetRandomOrDefault(random);
            if(middle != null)
            {
                grid.Positions[middle.X, middle.Y].Intentions.Add(Intention.RopeIntention());
            }

            DebugPrintMeaning(grid, Meaning.Rope);
        }

        public static void CreateShooterSection(IntentionGrid grid, Random random)
        {
            //counts on having a border of width of 1

            //get a humble groundlevel tile at edge
            var tile = grid.GetByMeaning(Meaning.GroundLevel).Where(t => 
                (t.X == grid.Width -2 || t.X == 1) 
                && grid.Positions[t.X, t.Y].Intentions.Count == 1
            )
            //not next to exit or entrance path
            .Where(t=>
                !grid.Positions[t.X + 1, t.Y].Intentions.Any(n=>n.Meaning == Meaning.EntrancePath || n.Meaning == Meaning.ExitPath)
                &&
                !grid.Positions[t.X - 1, t.Y].Intentions.Any(n=> n.Meaning == Meaning.EntrancePath || n.Meaning == Meaning.ExitPath)
            )
            .ToList().GetRandomOrDefault(random);

            if (tile != null)
            {
                grid.Positions[tile.X, tile.Y].Intentions.Clear();
                grid.Positions[tile.X, tile.Y].Intentions.Add(Intention.ShooterIntention());
            }
            
            DebugPrintMeaning(grid, Meaning.Shooter);
        }

        public static void CreateBorder(IntentionGrid grid, Random random)
        {
            var groundLevelMax = grid.GetByMeaning(Meaning.GroundLevel).Select(t => t.Y).Max();
            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    if(
                        
                        (x == 0 || y == 0 || x == grid.Width -1 || y > groundLevelMax)
                        &&
                        grid.Positions[x,y].Intentions.Count() == 0
                    )
                    {
                        grid.Positions[x, y].Intentions.Add(Intention.SolidIntention());
                    }
                }
            }

            DebugPrintMeaning(grid, Meaning.Solid);
        }

        public static void BlanketNonDynamic(IntentionGrid grid, Random random)
        {
            for(var y = 0; y < grid.Height; y++)
            {
               for(var x = 0; x < grid.Width; x++)
                {
                    if (grid.Positions[x, y].Intentions.Count == 0 ||
                        (grid.Positions[x, y].Intentions.Count == 1 && grid.Positions[x, y].Intentions.Any(i => i.Meaning == Meaning.GroundLevel))
                    )
                    {
                        grid.Positions[x, y].Intentions.Add(Intention.NonDynamic());
                    }
                }
            }

            DebugPrintMeaning(grid, Meaning.NonDynamnic);
        }

        private static void DebugPrintMeaning(IntentionGrid grid, Meaning meaning)
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
    }
}
