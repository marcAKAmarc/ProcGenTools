using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcGenTools.Helper
{
    public class DifferentColors
    {
        private int r;
        private int g;
        private int b;

        private double minSum = 300;
        private double maxSum = 500;
        public DifferentColors()
        {
            r = 0;
            g = 127;
            b = 255;
        }
        public Color GetVeryDifferentColor()
        {
            r = (r + (10 * 7)) % 255;
            g = (g + (10 * 3)) % 255;
            b = (b + (10 * 10)) % 255;
            return Color.FromArgb(r, g, b);
            var maxColor = Math.Max(b, Math.Max(r, g));
            var ratio = ((double)(maxSum - minSum)) / Math.Max(b, Math.Max(r, g));
            return Color.FromArgb(
                (int)((r * ratio) + (minSum / 3)),
                (int)((g * ratio) + (minSum / 3)),
                (int)((b * ratio) + (minSum / 3))
            );
        }
    }
}
