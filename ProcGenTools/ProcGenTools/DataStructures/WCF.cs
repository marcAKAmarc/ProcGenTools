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
        public WcfGrid(Random _random)
        {
            random = _random;
        }
        public WcfGrid()
        {
            random = new Random();
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
                        SuperPositions[x, y, z] = new WcfSuperPosition(items,random,x,y,z, this);
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

            //SuperPositionsFlat.ForEach(sp => sp.RecordPreviousStates());
        }

        public void SetInfluenceShape(List<WcfVector> shape)
        {
            InfluenceShape = shape.ToArray();

        }
        
        private int GetNeighborhoodEntropy(WcfSuperPosition superposition)
        {
            var neighborhood = GetSuperPositionNeighbors(superposition.x, superposition.y, superposition.z);
            neighborhood.Add(superposition);
            return neighborhood.Sum(x => x.slots.Where(y => !y.Collapsed).Count());
        }

        private WcfSuperPosition CollapseMinimumEntropyPosition()
        {
            //handle collapse random spot;
            var entropyOrderedSuperPositions = SuperPositionsFlat.Where(x => !x.hasCollapsed).OrderBy(x => GetNeighborhoodEntropy(x)).ToArray();//.OrderBy(x => x.slots.Where(y => !y.Collapsed).Count()).ToArray();

            var minimumEntropy = entropyOrderedSuperPositions.FirstOrDefault().slots.Where(y => !y.Collapsed).Count();
            var superPositionsWithMinimumEntropy = entropyOrderedSuperPositions.Where(x => x.slots.Where(y => !y.Collapsed).Count() == minimumEntropy).ToArray();
            var toBeResolved = superPositionsWithMinimumEntropy[random.Next(superPositionsWithMinimumEntropy.Count())];

            toBeResolved.CollapseToRandomItem(random);

            return toBeResolved;            
        }

        int failures = 0;
        public bool CollapseAllRecursive()
        {
            failures = 0;
            //SuperPositionsFlat.ForEach(sp => sp.RecordPreviousStates());

            //while some are not collapsed
            var dbCount = 0;
            while (SuperPositionsFlat.Any(x => !x.hasCollapsed))
            {
                dbCount += 1;
                if(dbCount == 17)
                {
                    var breaka = "here";
                }
                var collapsed = CollapseMinimumEntropyPosition();
                if (!SuperPositionsFlat.Any(x => !x.hasCollapsed))
                {
                    var breaka = "here";
                }
                var propagationResult = handlePropagation(collapsed);
                if (propagationResult == false)
                    return false;
                //var propagationResult = collapsed.Propagate();
                //if(propagationResult == false)
                //{
                //    SuperPositionsFlat.ForEach(sp => sp.RestorePreviousStates());
                //    failures++;
                //    if (failures > 100)
                //        return false;
                //}
                //else
                //{
                //    failures = 0;
                //}
                //SuperPositionsFlat.ForEach(sp => sp.RecordPreviousStates());
               // PrintStatesToConsole2d();
            }
            PrintStatesToConsole2d();
            return true;
            
        }

        public bool handlePropagation(WcfSuperPosition previouslyCollapsed)
        {
            var propagationResult = previouslyCollapsed.Propagate();
            //if (propagationResult == false)
            //{
            //    SuperPositionsFlat.ForEach(sp => sp.RestorePreviousStates());
            //    failures++;
            //    if (failures > 100)
            //        return false;
            //}
            //else
            //{
            //    failures = 0;
            //}
            //SuperPositionsFlat.ForEach(sp => sp.RecordPreviousStates());
            return propagationResult;
        }

        //private void handleCollapseFailure(WcfSuperPosition failedSuperPosition, List<WcfSuperPosition> propagatedSuperPositions, WcfSuperPosition manuallyCollapsedSuperPosition)
        //{
        //    failedSuperPosition.hasCollapsed = false;
        //    failedSuperPosition.Uncollapse();
        //    foreach(var item in propagatedSuperPositions)
        //    {
        //        item.hasCollapsed = false;
        //        item.Uncollapse();
        //    }
        //    manuallyCollapsedSuperPosition.hasCollapsed = false;
        //    failedSuperPosition.Uncollapse();

        //}
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
                        Console.Write(resolutions.FirstOrDefault().item.Name);
                    else
                        Console.Write(" ");
                }
                Console.WriteLine("");
            }
            Console.WriteLine("Press Enter to continue.");
            Console.ReadLine();
        }
        public void PrintStatesToConsole2d()
        {
            //var updatedSuperPosition = SuperPositionsFlat.Where(x => x.hasUpdated);
            Console.WriteLine("");
            for (var y = 0; y < height; y++)
            {
                for(var x = 0; x < width; x++)
                {
                    var thisSuperPosition = SuperPositions[x, y, 0];
                    //var isUpdated = updatedSuperPosition.Any(usp => usp.x == thisSuperPosition.x && usp.y == thisSuperPosition.y && thisSuperPosition.z == 0);
                    var numberOfPossibilities = thisSuperPosition.slots.Where(s => !s.Collapsed).Count();
                    var firstOpenSlot = thisSuperPosition.slots.Where(s => !s.Collapsed).FirstOrDefault();

                    var color = ConsoleColor.White;
                    //if ( thisSuperPosition.hasUpdated)
                    //    color = ConsoleColor.Cyan;
                    //if ( thisSuperPosition.hasUpdated)
                    //    color = ConsoleColor.DarkMagenta; //should never happen?
                    //if ( !thisSuperPosition.hasUpdated)
                    //    color = ConsoleColor.Yellow;
                    

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
                        Console.Write(thisSuperPosition.slots.Where(_x=>!_x.Collapsed).First().item.Name);
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
            //Console.WriteLine("Press Enter to continue.");
            //Console.ReadLine();

        }

    }
    public class WcfSuperPosition
    {
        private Random random;
        public bool hasUpdated = false;
        public bool hasCollapsed { get{return HasCollapsed(); } }
        private bool hasUpdatedPrevious = false;
        private bool hasCollapsedPrevious = false;
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public List<WcfCollapsableSlot> slots;
        private List<bool> previousStates;
        private List<bool> previousStatesTemp;
        internal WcfGrid parent;

        public bool Propagate()
        {
            //parent.PrintValuesToConsole2d();
            //true = OK!
            //false = failure, revert!
            var neighbors = parent.GetSuperPositionNeighbors(x, y, z);
            foreach(var neighbor in neighbors)
            {
                var resultCollapse = neighbor.Collapse(this);
                
                if(resultCollapse == null)
                {
                    //failure
                    Console.WriteLine("Couldn't Collapse neighbor");
                    return false;
                }

                if(resultCollapse == true)
                {
                    //neighbor updated because of this, we then propagate onto them.
                    var propagationResult = neighbor.Propagate();
                    //parent.PrintStatesToConsole2d();
                    if (propagationResult == false)
                    {
                        Console.WriteLine("Propagation failed");
                        //failure
                        return false;
                    }
                }

                if(resultCollapse == false)
                {
                    //No change was made to them, do nothing!
                }
            }

            return true;
        }

        public void RecordPreviousStates()
        {
            previousStates = new List<bool>();
            foreach (var slot in slots)
                previousStates.Add(slot.Collapsed);
            //hasUpdatedPrevious = hasUpdated;
            //hasCollapsedPrevious = hasCollapsed;
        }
        public void RecordPreviousStatesTemp()
        {
            previousStatesTemp = new List<bool>();
            foreach (var slot in slots)
                previousStatesTemp.Add(slot.Collapsed);
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
            //hasCollapsed = hasCollapsedPrevious;
            //hasUpdated = hasUpdatedPrevious;

        }
        private bool HasCollapsed()
        {
            var result = slots.Count(x => !x.Collapsed) <= 1;
            if(result == true)
            {
                var breka = "asdf";
            }
            return slots.Count(x => !x.Collapsed) <= 1;
        }
        public bool? Collapse(WcfSuperPosition context) {

            RecordPreviousStatesTemp();

            int remaining = 0;
            foreach(var slot in slots)
            {
                var remains = !slot.TryCollapse(context);
                if (remains)
                    remaining += 1;
            }
            //var remaining = slots.Select(x => x.TryCollapse(context)).Where(x=>x==false).Count();

            var justChanged = false;
            for (var i = 0; i < slots.Count; i++)
            {

                if (previousStatesTemp[i] != slots[i].Collapsed)
                {
                    justChanged = true;
                    break;
                }
            }

            if (remaining > 0)
                return justChanged;
            //undefined, we have no options left!
            return null; 
        } 

        public void CollapseToItem (Guid itemId)
        {
            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i].item.Id != itemId)
                {
                    slots[i].Collapse();
                }
                else
                {
                    slots[i].UnCollapse();
                }
            }
            //hasCollapsed = true;
            //hasUpdated = true;
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
            //hasCollapsed = true;
            //hasUpdated = true;
        }

        public void CollapseToItems(List<Guid> itemIds, bool doNotUncollapse = false)
        {
            for (var i = 0; i < slots.Count; i++)
            {
                if (!itemIds.Any(x=>x == slots[i].item.Id))
                {
                    slots[i].Collapse();
                }
                else if(!doNotUncollapse)
                {
                    slots[i].UnCollapse();
                }
            }
            //hasCollapsed = true;
            //hasUpdated = true;
        }

        public void PreventItems(List<Guid> itemIds, bool haltIfAllOthersCollapsed = true, bool uncollapseOthers = false)
        {
            if (uncollapseOthers)
            {
                for (var i = 0; i < slots.Count; i++)
                {
                    slots[i].UnCollapse();
                }
            }

            for (var i = 0; i < slots.Count; i++)
            {
                if (haltIfAllOthersCollapsed && hasCollapsed)
                    break;

                if (itemIds.Any(x => x == slots[i].item.Id))
                {
                    slots[i].Collapse();
                }
            }   
        }

        public void CollapseToRandomItem(Random random)
        {
            var available = slots.Where(x => !x.Collapsed).ToList();
            if (available.Count() == 1)
            {
                //hasCollapsed = true;
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
            //hasCollapsed = true;
            //hasUpdated = true;
        }
        public void Uncollapse()
        {
            for (var i = 0; i < slots.Count; i++)
                slots[i].UnCollapse();
        }
        public T FirstRemainingPayload<T>() where T : class
        {
            if (slots.Where(_x => !_x.Collapsed).FirstOrDefault() == null)
                return slots.First().item.GetItem() as T;
            return slots.Where(_x => !_x.Collapsed).First().item.GetItem() as T;
        }
        public T FinalPayloadIfExists<T>() where T : class
        {
            if (slots.Count(_x=> !_x.Collapsed) != 1)
            {
                return null as T;
            }

            return slots.First(x => !x.Collapsed).item.GetItem() as T;
        }
        public T FinalPayloadIfExists<T>(T ifTooMany, T ifNone) where T : class
        {
            if (slots.Count(_x => !_x.Collapsed) > 1)
            {
                return ifTooMany;
            }
            if (slots.Count(_x => !_x.Collapsed) == 0)
            {
                return ifNone;
            }
            return slots.First(x => !x.Collapsed).item.GetItem() as T;
        }
        public WcfSuperPosition(IEnumerable<IOpinionatedItem> items, Random _random, int _x, int _y, int _z, WcfGrid _parentGrid)
        {
            x = _x;
            y = _y;
            z = _z;
            parent = _parentGrid;
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
            //returns true if collapsed to 1 or less...
            //returns false if more options left

            var relativex = _superPosition.x - superPosition.x;
            var relativey = _superPosition.y - superPosition.y;
            var relativez = _superPosition.z - superPosition.z;

            var foundAcceptableItem = false;
            foreach (var superPositionItem in _superPosition.slots.Where(x => !x.Collapsed).Select(x => x.item))
            {
                var accepted = item.AcceptsInDirection(superPositionItem, relativex, relativey, relativez);

                if (accepted)
                {
                    foundAcceptableItem = true;
                    break;
                }
            }
            if (!foundAcceptableItem) { 
                collapsed = true;
                return collapsed;
            }
            //check requirements are met
            var requirementsMet = RequirementsMet();

            if (!requirementsMet)
            {
                collapsed = true;
                return collapsed;
            }
            return collapsed;
        }

        
        public bool RequirementsMet()
        {
            var requirementsMet = true;
            var neighbors = superPosition.parent.GetSuperPositionNeighbors(superPosition.x, superPosition.y, superPosition.z);
            foreach (var requirement in item.requirements)
            {
                var countInNeighbors = neighbors.SelectMany(sp => sp.slots.Where(s => !s.collapsed && s.item.Id == requirement.Item2)).Count();
                var countInNeighborsCollapsed = neighbors.SelectMany(sp => sp.slots.Where(s => !s.collapsed && s.item.Id == requirement.Item2)).Where(sp=>sp.collapsed).Count();
                var countInNeighborsNotCollapsed = neighbors.SelectMany(sp => sp.slots.Where(s => !s.collapsed && s.item.Id == requirement.Item2)).Where(sp => !sp.collapsed).Count();

                var pass = false;
                var goal = requirement.Item1;
                switch (requirement.Item3)
                {
                    case RequirementComparison.EqualTo:
                        pass = countInNeighbors >= goal  && countInNeighborsCollapsed <= goal;
                        break;
                    case RequirementComparison.GreaterThan:
                        pass = countInNeighbors > goal;
                        break;
                    case RequirementComparison.GreaterThanOrEqualTo:
                        pass = countInNeighbors >= goal;
                        break;
                    case RequirementComparison.LessThan:
                        pass = countInNeighborsCollapsed < goal;
                        break;
                    case RequirementComparison.LessThanOrEqualTo:
                        pass = countInNeighborsCollapsed <= goal;
                        break;
                    case RequirementComparison.NotEqualTo:
                        pass = countInNeighbors != 0 || countInNeighborsCollapsed != goal;
                        break;
                }
                if (!pass)
                {
                    requirementsMet = false;
                    break;
                }
            }
            return requirementsMet;
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

    public enum RequirementComparison { LessThan, LessThanOrEqualTo, EqualTo, GreaterThan, GreaterThanOrEqualTo, NotEqualTo};
    public interface IOpinionatedItem
    {
        Guid Id { get; }
        List<List<List<List<IOpinionatedItem>>>> acceptableItems { get; set; }
        List<IOpinionatedItem>[,,] acceptableItemsArray { get; set; }
        List<Tuple<int, Guid, RequirementComparison>> requirements { get; set; }
        void SetAcceptableInDirection(IOpinionatedItem item, int x, int y, int z, bool mutual = true);
        List<IOpinionatedItem> GetAcceptableInDirection(int x, int y, int z);
        bool AcceptsInDirection(IOpinionatedItem item, int x, int y, int z);
        string Name { get; }
        object GetItem();
    }
    public class OpinionatedItem<T> : IOpinionatedItem
    {
        public List<IOpinionatedItem>[,,] acceptableItemsArray { get; set; }
        public List<List<List<List<IOpinionatedItem>>>> acceptableItems { get; set; } //[x][y][z][i]
        public List<Tuple<int, Guid, RequirementComparison>> requirements { get; set; }
        public T actualItem;
        public string actualItemName;
        public string Name {
            get{ return actualItemName; } }
        private Guid id;
        private int offsetX, offsetY, offsetZ;

        public Guid Id { get { return id; } }

        public object GetItem()
        {
            return actualItem;
        }
        public OpinionatedItem(T _actualItem, string _actualItemName, List<WcfVector> neighborhoodShape ){
            actualItem = _actualItem;
            actualItemName = _actualItemName;
            Init(neighborhoodShape);
        }
        public void AddRequirement(IOpinionatedItem item, RequirementComparison comparison, int amount)
        {
            requirements.Add(new Tuple<int, Guid, RequirementComparison>(amount, item.Id, comparison));
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
            //TODO: MAKE THIS DYNAMIC
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
            return acceptableItemsArray[x+offsetX,y+offsetY,z+offsetZ].Any(i => i.Id == item.Id);
        }
        public List<IOpinionatedItem> GetAcceptableInDirection(int x, int y, int z)
        {
            return acceptableItemsArray[x+offsetX,y+offsetY,z+offsetZ];
        }

        public void AddMutualAcceptionInDirection(IOpinionatedItem item, int x, int y, int z)
        {
            acceptableItemsArray[x + offsetX,y + offsetY,z + offsetZ].Add(item);
            item.acceptableItemsArray[(x *-1) + offsetX,(y * -1) + offsetY,(z * -1) + offsetZ].Add(this);
        }
        public void AddExclusiveAcceptionInDirection(IOpinionatedItem item, int x, int y, int z)
        {
            acceptableItemsArray[x + offsetX,y + offsetY,z + offsetZ].Add(item);
        }
        public void RemoveMutualAcceptionInDirection(IOpinionatedItem item, int x, int y, int z)
        {
            acceptableItemsArray[x + offsetX,y + offsetY,z + offsetZ] = acceptableItemsArray[x + offsetX,y + offsetY,z + offsetZ].Where(_x => _x.Id != item.Id).ToList();
            item.acceptableItemsArray[(x * -1) + offsetX,(y * -1) + offsetY,(z * -1) + offsetZ] = item.acceptableItemsArray[(x * -1) + offsetX,(y * -1) + offsetY,(z * -1) + offsetZ].Where(_x => _x.Id == item.Id).ToList();
        }
        public void RemoveExclusiveAcceptionInDirection(IOpinionatedItem item, int x, int y, int z)
        {
            acceptableItemsArray[x + offsetX,y + offsetY,z + offsetZ] = acceptableItemsArray[x + offsetX,y + offsetY,z + offsetZ].Where(_x => _x.Id != item.Id).ToList();
        }
        public void ClearMutualAcceptionInDirection(int x, int y, int z)
        {
            foreach(var item in acceptableItemsArray[x + offsetX,y + offsetY,z + offsetZ])
            {
                item.acceptableItemsArray[(x * -1) + offsetX,(y * -1) + offsetY,(z * -1) + offsetZ] = item.acceptableItemsArray[(x * -1) + offsetX,(y * -1) + offsetY,(z * -1) + offsetZ].Where(_x => _x.Id == item.Id).ToList();
            }
            acceptableItemsArray[x + offsetX,y + offsetY,z + offsetZ] = new List<IOpinionatedItem>();
        }
        public void ClearExclusiveAcceptionInDirection(int x, int y, int z)
        {
            acceptableItemsArray[x + offsetX,y + offsetY,z + offsetZ] = new List<IOpinionatedItem>();
        }
        private bool AcceptItemIndexExists(int x, int y, int z)
        {
            var indexX = x + offsetX;
            var indexY = y + offsetY;
            var indexZ = z + offsetZ;

            return (indexX > 0 && acceptableItems.Count() > indexX)
                && (indexY > 0 && acceptableItems[indexX].Count() > indexY)
                && (indexZ > 0 && acceptableItems[indexX][indexY].Count() > indexZ);
        }
        private void Init( List<WcfVector> neighborhoodShape)
        {
            requirements = new List<Tuple<int, Guid, RequirementComparison>>();
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

            int minx, miny, minz, maxx, maxy, maxz;
            minx = 0; miny = 0; minz = 0;
            maxx = 0; maxy = 0; maxz = 0;
            foreach(var shapeNode in neighborhoodShape)
            {
                if (shapeNode.x < minx)
                    minx = shapeNode.x;
                if (shapeNode.y < miny)
                    miny = shapeNode.y;
                if (shapeNode.z < minz)
                    minz = shapeNode.z;

                if (shapeNode.x > maxx)
                    maxx = shapeNode.x;
                if (shapeNode.y > maxy)
                    maxy = shapeNode.y;
                if (shapeNode.z > maxz)
                    maxz = shapeNode.z;
            }

            int lengthX, lengthY, lengthZ;
            lengthX = (maxx - minx)+1;
            lengthY = (maxy - miny)+1;
            lengthZ = (maxz - minz)+1;

            //int offsetX, offsetY, offsetZ;
            offsetX = minx * -1;
            offsetY = miny * -1;
            offsetZ = minz * -1;

            acceptableItemsArray = new List<IOpinionatedItem>[lengthX, lengthY, lengthZ];

            acceptableItems = new List<List<List<List<IOpinionatedItem>>>>();
            for(var _x = 0; _x < lengthX; _x++)
            {
                acceptableItems.Add(new List<List<List<IOpinionatedItem>>>());
                for( var _y = 0; _y < lengthY; _y++)
                {
                    acceptableItems[_x].Add(new List<List<IOpinionatedItem>>());
                    for( var _z = 0; _z < lengthZ; _z++)
                    {
                        acceptableItems[_x][_y].Add(new List<IOpinionatedItem>());
                        acceptableItemsArray[_x, _y, _z] = new List<IOpinionatedItem>();
                    }
                }
            }

        }
    }

    public class Tuple<T1>
    {
        public Tuple(T1 item1)
        {
            Item1 = item1;
        }

        public T1 Item1 { get; set; }
    }

    public class Tuple<T1, T2> : Tuple<T1>
    {
        public Tuple(T1 item1, T2 item2) : base(item1)
        {
            Item2 = item2;
        }

        public T2 Item2 { get; set; }
    }

    public class Tuple<T1, T2, T3> : Tuple<T1, T2>
    {
        public Tuple(T1 item1, T2 item2, T3 item3) : base(item1, item2)
        {
            Item3 = item3;
        }

        public T3 Item3 { get; set; }
    }

    public static class Tuple
    {
        public static Tuple<T1> Create<T1>(T1 item1)
        {
            return new Tuple<T1>(item1);
        }

        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }

        public static Tuple<T1, T2, T3> Create<T1, T2, T3>(T1 item1, T2 item2, T3 item3)
        {
            return new Tuple<T1, T2, T3>(item1, item2, item3);
        }
    }
}

