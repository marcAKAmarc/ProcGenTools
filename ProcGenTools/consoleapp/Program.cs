using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcGenTools.DataStructures;
namespace consoleapp
{
    class Program
    {
        static void Main(string[] args)
        {
            var tl = new OpinionatedItem<String>("┌", "top-left");
            var tr = new OpinionatedItem<String>("┐", "top-right");
            var bl = new OpinionatedItem<String>("└", "bot-left");
            var br = new OpinionatedItem<String>("┘", "bot-right");
            var h = new OpinionatedItem<String>("-", "horizontal");
            var v = new OpinionatedItem<String>("|", "vertical");
            var em = new OpinionatedItem<String>(" ", "empty");
            var no = new OpinionatedItem<String>("X", "emptyNoFloor");
            var ladder = new OpinionatedItem<String>("L", "ladder");
            var all = new List<IOpinionatedItem> { tl, tr, br, bl, em, h, v, no, ladder};
            
            em.SetAcceptableInDirection(new List<IOpinionatedItem>() {tl, bl}, 1, 0, 0);
            em.SetAcceptableInDirection(new List<IOpinionatedItem>() {tr,br }, -1, 0, 0);
            em.SetAcceptableInDirection(new List<IOpinionatedItem>() {tl, tr }, 0, 1, 0);
            em.SetAcceptableInDirection(new List<IOpinionatedItem>() {bl, br }, 0, -1, 0);
            
            tl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tr, br }, 1, 0, 0);
            tl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tr, br }, -1, 0, 0);
            tl.SetAcceptableInDirection(new List<IOpinionatedItem>() { bl, br }, 0, 1, 0);
            tl.SetAcceptableInDirection(new List<IOpinionatedItem>() { bl, br }, 0, -1, 0);

            tr.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, bl }, 1, 0, 0);
            tr.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, bl }, -1, 0, 0);
            tr.SetAcceptableInDirection(new List<IOpinionatedItem>() { bl, br }, 0, 1, 0);
            tr.SetAcceptableInDirection(new List<IOpinionatedItem>() { bl, br }, 0, -1, 0);

            bl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tr, br }, 1, 0, 0);
            bl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tr, br }, -1, 0, 0);
            bl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, tr }, 0, 1, 0);
            bl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, tr }, 0, -1, 0);

            br.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, bl }, 1, 0, 0);
            br.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, bl }, -1, 0, 0);
            br.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, tr }, 0, 1, 0);
            br.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, tr }, 0, -1, 0);

            h.SetAcceptableInDirection(new List<IOpinionatedItem>() {tr, br,h}, 1, 0, 0);
            h.SetAcceptableInDirection(new List<IOpinionatedItem>() {tl, bl, h}, -1, 0, 0);
            h.SetAcceptableInDirection(new List<IOpinionatedItem>() {em, tr, tl }, 0, 1, 0);
            h.SetAcceptableInDirection(new List<IOpinionatedItem>() { em, br, bl }, 0, -1, 0);

            v.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, bl, em }, 1, 0, 0);
            v.SetAcceptableInDirection(new List<IOpinionatedItem>() { tr, br, em }, -1, 0, 0);
            v.SetAcceptableInDirection(new List<IOpinionatedItem>() { br, bl, v }, 0, 1, 0);
            v.SetAcceptableInDirection(new List<IOpinionatedItem>() { tr, tl, v }, 0, -1, 0);

            no.SetAcceptableInAllDirection(new List<IOpinionatedItem>() { em });
            no.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, bl, v }, 1, 0, 0);
            no.SetAcceptableInDirection(new List<IOpinionatedItem>() { tr, br ,v }, -1, 0, 0);
            no.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, tr, h }, 0, 1, 0);
            no.SetAcceptableInDirection(new List<IOpinionatedItem>() { bl, br, h }, 0, -1, 0);

            ladder.SetAcceptableInAllDirection(new List<IOpinionatedItem>() { em });
            ladder.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, bl, v }, 1, 0, 0);
            ladder.SetAcceptableInDirection(new List<IOpinionatedItem>() { tr, br, v }, -1, 0, 0);
            ladder.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, tr, v }, 0, 1, 0);
            ladder.SetAcceptableInDirection(new List<IOpinionatedItem>() { bl, br, h }, 0, -1, 0);
            ladder.requirements.Add(new Tuple<int, Guid, RequirementComparison>(1, h.Id, RequirementComparison.GreaterThanOrEqualTo));

            /*var tl = new OpinionatedItem<String>("_", "top-left");
            var tr = new OpinionatedItem<String>(" ", "top-right");
            var t = new OpinionatedItem<String>(".", "terminator");
            tl.SetAcceptableInAllDirection(new List<IOpinionatedItem>() { tr, t, tl });
            tr.SetAcceptableInAllDirection(new List<IOpinionatedItem>() { tr, t, tl });
            t.SetAcceptableInAllDirection(new List<IOpinionatedItem>() { tr, t, tl });

            tl.ClearAcceptableInDirection(1, 0, 0);
            tl.ClearAcceptableInDirection(-1, 0, 0);
            tl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, t }, 1, 0, 0);
            tl.SetAcceptableInDirection(new List<IOpinionatedItem>() { tl, t }, -1, 0, 0);

            tr.ClearAcceptableInDirection(1, 0, 0);
            tr.ClearAcceptableInDirection(-1, 0, 0);
            tr.SetAcceptableInDirection(new List<IOpinionatedItem>() { t, tr },1,0,0);
            tr.SetAcceptableInDirection(new List<IOpinionatedItem>() { t, tr }, -1, 0, 0);

            var wcf = new WcfGrid(0);
            wcf.Init(10,10,1,new List<IOpinionatedItem>() { tl, tr, t });*/

            var keyVal = ' ';
            var seed = 21;
            while (keyVal != 'Q' && keyVal != 'q')
            {
                var wcf = new WcfGrid(seed++);
                wcf.Init(20, 10, 1, all);
                var shape = new List<WcfVector>().Cross3dShape();
                wcf.SetInfluenceShape(shape);

                wcf.CollapseAll();

                wcf.PrintStatesToConsole2d();
                Console.WriteLine("Press any key to quit.");
                keyVal = Console.ReadKey().KeyChar;
            }
        }
    }
}
