using System;
using System.Collections.Generic;
using System.Linq;
namespace ProcGenTools.DataStructures
{
    public static class Extensions{

        public static List<WcfVector> CubeShape(this List<WcfVector> shape)
        {
            var _influenceShape = new List<WcfVector>();
            for (var x = -1; x < 2; x++)
            {
                for (var y = -1; y < 2; y++)
                {
                    for (var z = -1; z < 2; z++)
                    {
                        if (!(x == 0 && y == 0 && z == 0))
                        {
                            _influenceShape.Add(new WcfVector(x, y, z));
                        }
                    }
                }
            }
            shape = _influenceShape;
            return shape;
        }
        public static List<WcfVector> Cross3dShape(this List<WcfVector> shape)
        {
            var _influenceShape = new List<WcfVector> {
                new WcfVector(1, 0, 0),
                new WcfVector(-1, 0, 0),
                new WcfVector(0, 1, 0),
                new WcfVector(0, -1, 0),
                new WcfVector(0, 0, 1),
                new WcfVector(0, 0, -1)
            };
            shape = _influenceShape;
            return shape;
        }
    }
    public class WcfVector{
        public int x = 0;
        public int y = 0;
        public int z = 0;

        public WcfVector(int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
    }

    public class WcfGrid
    {
        private int width;
        private int height;
        private int depth;
        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public int Depth { get { return depth; } }
        private WcfVector[] InfluenceShape;
        public WcfSuperPosition[,,] SuperPositions;
        public List<WcfSuperPosition> SuperPositionsFlat;
        private Random random = new Random();

        public WcfGrid(int seed)
        {
            random = new Random(seed);
        }

        public void Init(int _width, int _height, int _depth, IEnumerable<IOpinionatedItem> items) 
        {
            width = _width;
            height = _height;
            depth = _depth;
            SuperPositions = new WcfSuperPosition[_width,_height,_depth];
            SuperPositionsFlat = new List<WcfSuperPosition>();
            for (var x = 0; x < _width; x++)
            {
                for(var y = 0; y < _height; y++)
                {
                    for(var z = 0; z < _depth; z++)
                    {
                        SuperPositions[x, y, z] = new WcfSuperPosition(items,random,x,y,z);
                        SuperPositionsFlat.Add(SuperPositions[x, y, z]);
                    }
                }
            }

            var _influenceShape = new List<WcfVector>();
            for(var x = -1; x < 2; x++)
            {
                for(var y = -1; y < 2; y++)
                {
                    for(var z = -1; z < 2; z++)
                    {
                        if(!(x==0 && y == 0 && z == 0))
                        {
                            _influenceShape.Add(new WcfVector(x,y,z));
                        }
                    }
                }
            }
            InfluenceShape = _influenceShape.ToArray();
        }

        public void SetInfluenceShape(List<WcfVector> shape)
        {
            InfluenceShape = shape.ToArray();
        } 

        public void CollapseAll()
        {
            /*PrintStatesToConsole2d();*/
            var updatedUpdateables = new List<WcfSuperPosition>();
            WcfSuperPosition forcedCollapse = null;
            //while some are not collapsed
            while (SuperPositionsFlat.Where(x => !x.hasCollapsed).Count() > 0)
            {
                var unresolvedSuperPositions = SuperPositionsFlat.Where(x => !x.hasCollapsed).OrderBy(x=>x.slots.Where(y=>!y.Collapsed).Count()).ToArray();
                var previouslyUpdatedSuperPositions = SuperPositionsFlat.Where(x => x.hasUpdated).ToList();

                foreach (var superPosition in SuperPositionsFlat)
                {
                    superPosition.hasUpdated = false;
                }

                //handle collapse random spot;
                if (previouslyUpdatedSuperPositions.Count() == 0)
                {
                    /*PrintStatesToConsole2d();
                    Console.WriteLine("Press key to collapse smallest entropy.");
                    Console.ReadKey();*/
                    updatedUpdateables = new List<WcfSuperPosition>();
                    forcedCollapse = null;

                    foreach (var superPosition in SuperPositionsFlat)
                    {
                        superPosition.hasPropagated = false;
                    }

                    var minimumEntropy = unresolvedSuperPositions.FirstOrDefault().slots.Where(y => !y.Collapsed).Count();
                    var superPositionsWithMinimumEntropy = unresolvedSuperPositions.Where(x => x.slots.Where(y => !y.Collapsed).Count() == minimumEntropy).ToArray();
                    var toBeResolved = superPositionsWithMinimumEntropy[random.Next(superPositionsWithMinimumEntropy.Count())];
                    //Console.WriteLine("Resolving (" + toBeResolved.x.ToString() + "," + toBeResolved.y.ToString() + "," + toBeResolved.z.ToString() + ")...");
                    toBeResolved.CollapseToRandomItem(random);
                    forcedCollapse = toBeResolved;
                    //Console.WriteLine("Collapsed to item " + toBeResolved.slots.Where(x => !x.Collapsed).First().item.GetItem().ToString());

                    //recalc this for propagation step
                    previouslyUpdatedSuperPositions = SuperPositionsFlat.Where(x => x.hasUpdated).ToList();
                }


                //handle propagation
                
                while (previouslyUpdatedSuperPositions.Count() > 0)
                {
                    var abort = false;
                    foreach (var superPosition in SuperPositionsFlat)
                    {
                        superPosition.hasUpdated = false;
                    }

                    /*PrintStatesToConsole2d();
                    Console.WriteLine("There are items that were previously updated, so we propagate.");
                    Console.ReadKey();*/

                    foreach (var updated in previouslyUpdatedSuperPositions)
                    {
                        
                        //Console.WriteLine("Propagating from (" + updated.x.ToString() + ", " + updated.y.ToString() + ", " + updated.z.ToString() + ")...");
                        var neighbors = GetSuperPositionNeighbors(updated.x, updated.y, updated.z)
                            .Where(x=>!x.hasCollapsed && !x.hasUpdated);

                        foreach (var neighbor in neighbors)
                        {
                            //Console.WriteLine(" ...to (" + neighbor.x.ToString() + ", " + neighbor.y.ToString() + ", " + neighbor.z.ToString() + ")...");
                            var resultCollapse = neighbor.Collapse(updated);
                            updatedUpdateables.Add(neighbor);

                            if (resultCollapse == null)
                            {
                                //Console.WriteLine("Failure.");
                                handleCollapseFailure(neighbor, updatedUpdateables, forcedCollapse);
                                abort = true;
                                break;
                            }
                            /*PrintStatesToConsole2d();
                            Console.WriteLine("Press other key go to next neighbor.");
                            Console.ReadKey();*/
                        }

                       /* PrintStatesToConsole2d();
                        Console.WriteLine("Press other key go to next previously updated.");
                        Console.ReadKey();
                        */
                        if (abort == true)
                            break;
                    }
                    if (abort == true)
                        break;

                    //should we add updated to previously updated here?
                    previouslyUpdatedSuperPositions = SuperPositionsFlat.Where(x => x.hasUpdated).ToList();
                }

                PrintStatesToConsole2d();
                /*Console.WriteLine("Press space key to print current grid. Press other key go to next iteration.");
                var resultKey = Console.ReadKey();
                if (resultKey.KeyChar == ' ')
                    PrintValuesToConsole2d();*/
            }

           
        }
        private void handleCollapseFailure(WcfSuperPosition failedSuperPosition, List<WcfSuperPosition> propagatedSuperPositions, WcfSuperPosition manuallyCollapsedSuperPosition)
        {
            failedSuperPosition.hasCollapsed = false;
            failedSuperPosition.Uncollapse();
            foreach(var item in propagatedSuperPositions)
            {
                item.hasCollapsed = false;
                item.Uncollapse();
            }
            manuallyCollapsedSuperPosition.hasCollapsed = false;
            failedSuperPosition.Uncollapse();
            /*failedSuperPosition.RestorePreviousStates();
            if(manuallyCollapsedSuperPosition != null)
                manuallyCollapsedSuperPosition.RestorePreviousStates();
            foreach (var propagatedSuperPosition in propagatedSuperPositions)
                propagatedSuperPosition.RestorePreviousStates();*/

        }
        public List<WcfSuperPosition> GetSuperPositionNeighbors(int _x, int _y, int _z)
        {
            var neighbors = new List<WcfSuperPosition>();

            foreach(var shapeNode in InfluenceShape)
            {
                var resultx = shapeNode.x + _x;
                var resulty = shapeNode.y + _y;
                var resultz = shapeNode.z + _z;
                if (
                       (resultx < 0 || resultx >= width)
                    || (resulty < 0 || resulty >= height)
                    || (resultz < 0 || resultz >= depth)
                    || (_x == 0 && _y == 0 && _z == 0)
                )
                    continue;
                neighbors.Add(SuperPositions[resultx, resulty, resultz]);
            }
            return neighbors;
        }

        public void PrintValuesToConsole2d()
        {
            Console.WriteLine("");
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var resolutions = SuperPositions[x, y, 0].slots.Where(o => !o.Collapsed);
                    if (resolutions.Count() == 1)
                        Console.Write(resolutions.FirstOrDefault().item.GetItem().ToString());
                    else
                        Console.Write(" ");
                }
                Console.WriteLine("");
            }
        }
        public void PrintStatesToConsole2d()
        {
            var updatedSuperPosition = SuperPositionsFlat.Where(x => x.hasUpdated);
            Console.WriteLine("");
            for (var y = 0; y < height; y++)
            {
                for(var x = 0; x < width; x++)
                {
                    var thisSuperPosition = SuperPositions[x, y, 0];
                    var isUpdated = updatedSuperPosition.Any(usp => usp.x == thisSuperPosition.x && usp.y == thisSuperPosition.y && thisSuperPosition.z == 0);
                    var numberOfPossibilities = thisSuperPosition.slots.Where(s => !s.Collapsed).Count();
                    var firstOpenSlot = thisSuperPosition.slots.Where(s => !s.Collapsed).FirstOrDefault();

                    var color = ConsoleColor.White;
                    if (thisSuperPosition.hasPropagated && thisSuperPosition.hasUpdated)
                        color = ConsoleColor.Cyan;
                    if (!thisSuperPosition.hasPropagated && thisSuperPosition.hasUpdated)
                        color = ConsoleColor.DarkMagenta; //should never happen?
                    if (thisSuperPosition.hasPropagated && !thisSuperPosition.hasUpdated)
                        color = ConsoleColor.Yellow;
                    

                    if (numberOfPossibilities == 0)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.ForegroundColor = color;
                        Console.Write(" ");
                        Console.ResetColor();
                        continue;
                    }
                    if(numberOfPossibilities == 1)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                        Console.ForegroundColor = color;
                        Console.Write(thisSuperPosition.slots.Where(_x=>!_x.Collapsed).First().item.GetItem().ToString());
                        Console.ResetColor();
                        continue;
                    }

                    //Number of possibilities > 1
                    Console.ForegroundColor = color;
                    Console.Write(numberOfPossibilities.ToString());
                    Console.ResetColor();
                }
                Console.WriteLine("");
            }
            
        }

    }
    public class WcfSuperPosition
    {
        private Random random;
        public bool hasPropagated = false;
        public bool hasUpdated = false;
        public bool hasCollapsed = false;
        private bool hasPropagatedPrevious = false;
        private bool hasUpdatedPrevious = false;
        private bool hasCollapsedPrevious = false;
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public List<WcfCollapsableSlot> slots;
        private List<bool> previousStates;

        private void RecordPreviousStates()
        {
            previousStates = new List<bool>();
            foreach (var slot in slots)
                previousStates.Add(slot.Collapsed);
            hasPropagatedPrevious = hasPropagated;
            hasUpdatedPrevious = hasUpdated;
            hasCollapsedPrevious = hasCollapsed;
        }
        public void RestorePreviousStates()
        {
            for (var i = 0; i < previousStates.Count(); i++)
            {
                if (previousStates[i])
                    slots[i].Collapse();
                else
                    slots[i].UnCollapse();
            }
            hasCollapsed = hasCollapsedPrevious;
            hasUpdated = hasUpdatedPrevious;
            hasPropagated = hasPropagatedPrevious;

        }
        public bool? Collapse(WcfSuperPosition context) {
            //if (!hasUpdated)
                RecordPreviousStates();

            var remaining = slots.Select(x => x.TryCollapse(context)).Where(x=>x==false).Count();

            //determine if updated
            var foundDifference = false;
            for (var i = 0; i < slots.Count(); i++)
            {
                
                    if (previousStates[i] != slots[i].Collapsed)
                    {
                        foundDifference = true;
                        break;
                    }
                
                if (foundDifference)
                    break;
            }
            hasUpdated = foundDifference;

            if (remaining == 1)
            {
                //hasUpdated = true;
                hasPropagated = true;
                hasCollapsed = true;
                return true;
            }

            if (remaining > 1)
            {
                hasCollapsed = false;
                //hasUpdated = true;
                hasPropagated = true;
                return false;
            }
            //undefined, we have no options left!
            hasCollapsed = true;
            //hasUpdated = true;
            hasPropagated = true;
            return null; 
        } 

        public void CollapseToItem(int index)
        {
            for(var i = 0; i < slots.Count; i++)
            {
                if(index != i)
                {
                    slots[i].Collapse();
                }
                else
                {
                    slots[i].UnCollapse();
                }
            }
            hasCollapsed = true;
            hasUpdated = true;
            hasPropagated = true;
        }
        public void CollapseToRandomItem(Random random)
        {
            var available = slots.Where(x => !x.Collapsed).ToList();
            if (available.Count() == 1)
            {
                hasCollapsed = true;
                return;
            }
            var keepIndex = random.Next(available.Count());
            for(var i = 0; i < available.Count(); i++)
            {
                if (i != keepIndex)
                {
                    available[i].Collapse();
                }
            }
            hasCollapsed = true;
            hasUpdated = true;
            hasPropagated = true;
        }
        public void Uncollapse()
        {
            for (var i = 0; i < slots.Count; i++)
                slots[i].UnCollapse();
        }
        //true = has completely collapsed and is resolved
        //false = has options left
        //null = no options left
        //looks out to neighbors
        public void IterationFailedReset()
        {
            hasPropagated = false;
        }

        public WcfSuperPosition(IEnumerable<IOpinionatedItem> items, Random _random, int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
            slots = new List<WcfCollapsableSlot>();
            foreach(var item in items)
            {
                slots.Add(new WcfCollapsableSlot(item, 1, this));
            }

            random = _random;
        }
    }


    public class WcfCollapsableSlot
    {
        public IOpinionatedItem item;
        private int weight = 1;
        public int Weight { get { return weight; } }
        private bool collapsed = false;
        public bool Collapsed { get { return collapsed; } }
        public WcfSuperPosition superPosition;

        public bool TryCollapse(WcfSuperPosition _superPosition)
        {
            var relativex = _superPosition.x - superPosition.x;
            var relativey = _superPosition.y - superPosition.y; 
            var relativez = _superPosition.z - superPosition.z;

            var foundAcceptableItem = false;
            foreach (var superPositionItem in _superPosition.slots.Where(x=>!x.Collapsed).Select(x => x.item))
            {
                var accepted = item.AcceptsInDirection(superPositionItem, relativex, relativey, relativez);
                if (accepted)
                {
                    foundAcceptableItem = true;
                    break;
                }
            }
            if (!foundAcceptableItem)
                collapsed = true;
            
            return collapsed;
        }

        public void Collapse()
        {
            collapsed = true;
        }
        public void UnCollapse()
        {
            collapsed = false;
        }

        public WcfCollapsableSlot(IOpinionatedItem _item, int _weight, WcfSuperPosition _superPosition)
        {
            weight = _weight;
            item = _item;
            superPosition = _superPosition;
        }
    }

    public interface IOpinionatedItem
    {
        Guid Id { get; }
        List<List<List<List<IOpinionatedItem>>>> acceptableItems { get; set; }
        void SetAcceptableInDirection(IOpinionatedItem item, int x, int y, int z, bool mutual = true);
        List<IOpinionatedItem> GetAcceptableInDirection(int x, int y, int z);
        bool AcceptsInDirection(IOpinionatedItem item, int x, int y, int z);
        object GetItem();
    }
    public class OpinionatedItem<T> : IOpinionatedItem
    {
        public List<List<List<List<IOpinionatedItem>>>> acceptableItems { get; set; } //[x][y][z][i]
        //x,y,z,i
        public T actualItem;
        public string actualItemName;
        private Guid id;
        public Guid Id { get { return id; } }

        public object GetItem()
        {
            return actualItem;
        }
        public OpinionatedItem(T _actualItem, string _actualItemName){
            actualItem = _actualItem;
            actualItemName = _actualItemName;
            Init();
        }
        public void SetAcceptableInDirection(IOpinionatedItem item, int x, int y, int z, bool mutual = true)
        {
            if (mutual)
                AddMutualAcceptionInDirection(item, x, y, z);
            else
                AddExclusiveAcceptionInDirection(item, x, y, z);
        }
        public void SetAcceptableInDirection(List<IOpinionatedItem> items, int x, int y, int z, bool mutual = true)
        {
            if (mutual)
                foreach (var item in items)
                    AddMutualAcceptionInDirection(item, x, y, z);
            else
                foreach (var item in items)
                    AddExclusiveAcceptionInDirection(item, x, y, z);
        }
        public void SetAcceptableInAllDirection(List<IOpinionatedItem> items, bool mutual = true)
        {
            for(var x = -1; x < 2; x++)
                for(var y = -1; y < 2; y++)
                    for(var z = -1; z < 2; z++)
                    {
                        if (x == 0 && y == 0 && z == 0)
                            continue;
                        if (mutual)
                            foreach (var item in items)
                                AddMutualAcceptionInDirection(item, x, y, z);
                        else
                            foreach (var item in items)
                                AddExclusiveAcceptionInDirection(item, x, y, z);
                    }
        }
        public void ClearAcceptableInDirection(int x, int y, int z, bool mutual = true)
        {
            if (mutual)
                ClearMutualAcceptionInDirection(x, y, z);
            else
                ClearExclusiveAcceptionInDirection(x, y, z);
        }
        public void RemoveAcceptableInDirection(IOpinionatedItem item, int x, int y, int z, bool mutual)
        {
            if (mutual)
                RemoveMutualAcceptionInDirection(item, x, y, z);
            else
                RemoveExclusiveAcceptionInDirection(item, x, y, z);
        }
        public void RemoveAcceptableInDirection(IList<IOpinionatedItem> items, int x, int y, int z, bool mutual)
        {
            if (mutual)
                foreach (var item in items)
                    RemoveMutualAcceptionInDirection(item, x, y, z);
            else
                foreach (var item in items)
                    RemoveExclusiveAcceptionInDirection(item, x, y, z);
        }
        public bool AcceptsInDirection(IOpinionatedItem item, int x, int y, int z)
        {
            return acceptableItems[x+1][y+1][z+1].Any(i => i.Id == item.Id);
        }
        public List<IOpinionatedItem> GetAcceptableInDirection(int x, int y, int z)
        {
            return acceptableItems[x+1][y+1][z+1];
        }


        public void AddMutualAcceptionInDirection(IOpinionatedItem item, int x, int y, int z)
        {
            acceptableItems[x + 1][y + 1][z + 1].Add(item);
            item.acceptableItems[(x *-1) + 1][(y * -1) + 1][(z * -1) + 1].Add(this);
        }
        public void AddExclusiveAcceptionInDirection(IOpinionatedItem item, int x, int y, int z)
        {
            acceptableItems[x + 1][y + 1][z + 1].Add(item);
        }
        public void RemoveMutualAcceptionInDirection(IOpinionatedItem item, int x, int y, int z)
        {
            acceptableItems[x + 1][y + 1][z + 1] = acceptableItems[x + 1][y + 1][z + 1].Where(_x => _x.Id != item.Id).ToList();
            item.acceptableItems[(x * -1) + 1][(y * -1) + 1][(z * -1) + 1] = item.acceptableItems[(x * -1) + 1][(y * -1) + 1][(z * -1) + 1].Where(_x => _x.Id == item.Id).ToList();
        }
        public void RemoveExclusiveAcceptionInDirection(IOpinionatedItem item, int x, int y, int z)
        {
            acceptableItems[x + 1][y + 1][z + 1] = acceptableItems[x + 1][y + 1][z + 1].Where(_x => _x.Id != item.Id).ToList();
        }
        public void ClearMutualAcceptionInDirection(int x, int y, int z)
        {
            foreach(var item in acceptableItems[x + 1][y + 1][z + 1])
            {
                item.acceptableItems[(x * -1) + 1][(y * -1) + 1][(z * -1) + 1] = item.acceptableItems[(x * -1) + 1][(y * -1) + 1][(z * -1) + 1].Where(_x => _x.Id == item.Id).ToList();
            }
            acceptableItems[x + 1][y + 1][z + 1] = new List<IOpinionatedItem>();
        }
        public void ClearExclusiveAcceptionInDirection(int x, int y, int z)
        {
            acceptableItems[x + 1][y + 1][z + 1] = new List<IOpinionatedItem>();
        }
        private void Init()
        {
            id = Guid.NewGuid();
            acceptableItems = new List<List<List<List<IOpinionatedItem>>>>();
            acceptableItems.Add(new List<List<List<IOpinionatedItem>>>());
            acceptableItems.Add(new List<List<List<IOpinionatedItem>>>());
            acceptableItems.Add(new List<List<List<IOpinionatedItem>>>());
            acceptableItems[0].Add(new List<List<IOpinionatedItem>>());
            acceptableItems[0].Add(new List<List<IOpinionatedItem>>());
            acceptableItems[0].Add(new List<List<IOpinionatedItem>>());
            acceptableItems[1].Add(new List<List<IOpinionatedItem>>());
            acceptableItems[1].Add(new List<List<IOpinionatedItem>>());
            acceptableItems[1].Add(new List<List<IOpinionatedItem>>());
            acceptableItems[2].Add(new List<List<IOpinionatedItem>>());
            acceptableItems[2].Add(new List<List<IOpinionatedItem>>());
            acceptableItems[2].Add(new List<List<IOpinionatedItem>>());
            acceptableItems[0][0].Add(new List<IOpinionatedItem>());
            acceptableItems[0][0].Add(new List<IOpinionatedItem>());
            acceptableItems[0][0].Add(new List<IOpinionatedItem>());
            acceptableItems[0][1].Add(new List<IOpinionatedItem>());
            acceptableItems[0][1].Add(new List<IOpinionatedItem>());
            acceptableItems[0][1].Add(new List<IOpinionatedItem>());
            acceptableItems[0][2].Add(new List<IOpinionatedItem>());
            acceptableItems[0][2].Add(new List<IOpinionatedItem>());
            acceptableItems[0][2].Add(new List<IOpinionatedItem>());
            acceptableItems[1][0].Add(new List<IOpinionatedItem>());
            acceptableItems[1][0].Add(new List<IOpinionatedItem>());
            acceptableItems[1][0].Add(new List<IOpinionatedItem>());
            acceptableItems[1][1].Add(new List<IOpinionatedItem>());
            acceptableItems[1][1].Add(new List<IOpinionatedItem>());
            acceptableItems[1][1].Add(new List<IOpinionatedItem>());
            acceptableItems[1][2].Add(new List<IOpinionatedItem>());
            acceptableItems[1][2].Add(new List<IOpinionatedItem>());
            acceptableItems[1][2].Add(new List<IOpinionatedItem>());
            acceptableItems[2][0].Add(new List<IOpinionatedItem>());
            acceptableItems[2][0].Add(new List<IOpinionatedItem>());
            acceptableItems[2][0].Add(new List<IOpinionatedItem>());
            acceptableItems[2][1].Add(new List<IOpinionatedItem>());
            acceptableItems[2][1].Add(new List<IOpinionatedItem>());
            acceptableItems[2][1].Add(new List<IOpinionatedItem>());
            acceptableItems[2][2].Add(new List<IOpinionatedItem>());
            acceptableItems[2][2].Add(new List<IOpinionatedItem>());
            acceptableItems[2][2].Add(new List<IOpinionatedItem>());
        }
    }
}

