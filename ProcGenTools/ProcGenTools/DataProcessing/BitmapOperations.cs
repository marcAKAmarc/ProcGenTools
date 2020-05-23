using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ProcGenTools.DataProcessing
{
    public static class BitmapOperations
    {
        public static List<List<Bitmap>> GetBitmapTiles(Bitmap originMap, int TileWidth, int TileHeight, bool hasSpacing = false)
        {
            List<List<Bitmap>> results = new List<List<Bitmap>>();
            for (var x = 0; x < originMap.Width; x += TileWidth)
            {
                for (var y = 0; y < originMap.Height; y += TileHeight)
                {
                    if (y == 0)
                        results.Add(new List<Bitmap>());
                    /*if (y == 0)
                        results.Last().Add();*/

                    Bitmap nb = new Bitmap(TileWidth, TileHeight);
                    Graphics g = Graphics.FromImage(nb);
                    g.DrawImage(originMap, new Point(-x, -y));
                    results.Last().Add(nb);

                    if (hasSpacing)
                        y += 1;

                }
                if (hasSpacing)
                    x += 1;
            }
            return results;
        }

        public static Bitmap CreateBitmapFromTiles(List<List<Bitmap>> grid, bool createSpacing = false)
        {
            var tileWidth = grid[0][0].Width;
            var tileHeight = grid[0][0].Height;
            var xpos = 0;
            var ypos = 0;
            var newBitmapWidth = grid[0][0].Width * grid.Count();
            var newBitmapHeight = grid[0][0].Height * grid[0].Count();
            if (createSpacing)
            {
                newBitmapWidth += grid.Count() - 1;
                newBitmapHeight += grid[0].Count() - 1;
            }
            Bitmap nb = new Bitmap(newBitmapWidth, newBitmapHeight);
            Graphics g = Graphics.FromImage(nb);
            foreach (var xRow in grid)
            {
                foreach (var yBitmap in xRow)
                {
                    if (yBitmap == null)
                        continue;
                    g.DrawImage(yBitmap, new Point(xpos, ypos));
                    ypos += tileHeight;
                    if (createSpacing)
                        ypos += 1;
                }
                xpos += tileWidth;
                if (createSpacing)
                    xpos += 1;
                ypos = 0;
            }

            return nb;
        }

        public static void SaveBitmapToFile(string filepath, Bitmap bitmap)
        {
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                //SAVING TO FILE
                g.Flush();
                bitmap.Save(filepath, System.Drawing.Imaging.ImageFormat.Bmp);
            }
        }

        public static bool Compare(this Bitmap bmp1, Bitmap bmp2)
        {
            
            if (bmp1 == null || bmp2 == null)
                return false;
            if (object.Equals(bmp1, bmp2))
                return true;
            if (!bmp1.Size.Equals(bmp2.Size))
                return false;
            /*if (!bmp1.PixelFormat.Equals(bmp2.PixelFormat))
                return false;*/

            //Compare bitmaps using GetPixel method
            for (int column = 0; column < bmp1.Width; column++)
            {
                for (int row = 0; row < bmp1.Height; row++)
                {
                    if (!bmp1.GetPixel(column, row).Equals(bmp2.GetPixel(column, row)))
                        return false;
                }
            }

            return true;         
        }

        public static Bitmap AddHorizontalMirror(this Bitmap bmp, bool hasSpacing = false)
        {
            Bitmap mirror = new Bitmap(bmp);
            mirror.RotateFlip(RotateFlipType.RotateNoneFlipX);

            Bitmap result = new Bitmap(bmp.Width * 2, bmp.Height);
            Graphics g = Graphics.FromImage(result);
            g.DrawImage(bmp, new Point(0, 0));
            if(!hasSpacing)
                g.DrawImage(mirror, new Point(bmp.Width, 0));
            else
                g.DrawImage(mirror, new Point(bmp.Width-1, 0));

            return result;
        }
    }
}



