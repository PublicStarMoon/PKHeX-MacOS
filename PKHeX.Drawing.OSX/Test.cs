using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace PKHeX.Drawing.OSX
{
    public static class Test
    {
        public static void Main()
        {
            Color c = ColorUtil.ColorBaseStatTotal(600);

            Console.WriteLine(c.ToPixel<Argb32>());
        }
    }
}