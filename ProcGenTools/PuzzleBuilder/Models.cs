﻿using ProcGenTools.DataStructures;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PuzzleBuilder
{
    public class PuzzleInfo
    {
        public IntentionGrid Grid;
        public Bitmap TileMap;
    } 
    public enum Meaning
    {
        Jump,
        Ladder,
        HTraversable,
        HTraversablePlain,
        VTraversable,
        Conveyor,
        Elevator,
        Rope,
        ToggleDoor,
        Button,
        Box,
        BoxPath,
        BoxPathVertical,
        Shooter,
        ShooterAlley,
        Circuit,
        GroundLevel,
        EntrancePath,
        ExitPath,
        VerticalExit,
        Solid,
        NonDynamnic,
        NonDynamicStrict,
        Empty,
        SolidOrEmpty,
        Walkable
    }

    public interface iMeaningConverter
    {
        List<OpinionatedItem<Bitmap>> MeaningToTiles(Meaning meaning, TilesetConfiguration config);
    }

    public class Intention
    {
        public Meaning Meaning;
        public Intention RelatedTileMeaning;
        public Point RelatedTilePosition;
        public string Info;

        public static Intention HorizontalPlainIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.HTraversablePlain
            };
        }

        public static Intention EmptyIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.Empty
            };
        }
        public static Intention SolidOrEmptyIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.SolidOrEmpty
            };
        }

        public static Intention SolidIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.Solid
            };
        }

        public static Intention LadderIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.Ladder
            };
        }

        public static Intention EntrancePathIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.EntrancePath
            };
        }

        public static Intention ExitPathIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.ExitPath
            };
        }
        public static Intention VerticalExitIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.VerticalExit
            };
        }
        public static Intention GroundLevelIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.GroundLevel
            };
        }

        public static Intention CircuitIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.Circuit
            };
        }

        public static Intention JumpIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.Jump
            };
        }

        public static Intention HTraversableIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.HTraversable
            };
        }

        public static Intention VTraversableIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.VTraversable
            };
        }

        public static Intention ConveyorIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.Conveyor
            };
        }

        public static Intention ElevatorIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.Elevator
            };
        }

        public static Intention RopeIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.Rope
            };
        }

        public static Intention ToggleDoorIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.ToggleDoor
            };
        }

        public static Intention ButtonIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.Button
            };
        }

        public static Intention BoxIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.Box
            };
        }

        public static Intention BoxPathVerticalIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.BoxPathVertical
            };
        }

        public static Intention BoxPathIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.BoxPath
            };
        }

        public static Intention ShooterIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.Shooter
            };
        }

        public static Intention NonDynamic()
        {
            return new Intention()
            {
                Meaning = Meaning.NonDynamnic
            };
        }

        public static Intention NonDynamicStrict()
        {
            return new Intention()
            {
                Meaning = Meaning.NonDynamicStrict
            };
        }

        public static Intention WalkableIntention()
        {
            return new Intention()
            {
                Meaning = Meaning.Walkable
            };
        }
    }

    public class IntentionTiles
    {
        public List<Intention> Intentions;
        public int X;
        public int Y;

        public IntentionTiles()
        {
            Intentions = new List<Intention>();
        }
    }

    public class IntentionGrid
    {
        public int Width;
        public int Height;
        public IntentionTiles[,] Positions;

        public IntentionGrid(int width, int height)
        {
            Width = width;
            Height = height;
            Positions = new IntentionTiles[width, height];
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    Positions[x, y] = new IntentionTiles();
                    Positions[x, y].X = x;
                    Positions[x, y].Y = y;
                }
            }
        }

        public List<IntentionTiles> listed()
        {
            var result = new List<IntentionTiles>();
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    result.Add(Positions[x, y]);
                }
            }

            return result;
        }

        public IEnumerable<IntentionTiles> GetByMeaning(Meaning meaning)
        {
            return listed().Where(x => x.Intentions.Any(i => i.Meaning == meaning));
        }
    }

}

