using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcGenTools.DataStructures;
using System.Configuration;
namespace Test.HierarchicalMaps
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Presse Enter");
            Console.ReadKey();
            HierarchicalMap.RelativeScales = new int[] { 2,2 };
            var map = new HierarchicalMap(10, 10, new Random(0));
            //map.SetTestPortals();
            //map.CreatePaths();
            //map.CoverPathsWithZones(3,2);
            map.SpawnZoneAtClusterPosition(3,2);
            map.PrintToConsole();
            Console.ReadLine();
            map.SpawnZoneAtClusterPosition(3, 2, null, true);
            map.PrintToConsole();
            Console.ReadLine();
            map.SpawnZoneAtClusterPosition(3, 2, null, true);
            map.PrintToConsole();
            Console.ReadLine();
            map.SpawnZoneAtClusterPosition(3, 2, null, true);
            map.PrintToConsole();
            Console.ReadLine();
            map.CreateSubMaps();
            map.PrintMasterToConsole();
            map.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"]);
            for(var i = 0; i < map.flatZones.Count(); i++)
            {
                var submap = map.flatZones[i].SubMap;
                submap.PrintMasterToBitmap(ConfigurationManager.AppSettings["BitmapOutput"].Replace(".bmp", i.ToString() + ".bmp"));

            }
        }
    }
}
