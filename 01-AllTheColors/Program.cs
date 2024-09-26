using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;

namespace _01_AllTheColors;

public class Options
{
  [Option('w', "width", Required = false, Default = 400, HelpText = "Image width in pixels.")]
  public int Width { get; set; }

  [Option('h', "height", Required = false, Default = 300, HelpText = "Image height in pixels.")]
  public int Height { get; set; }

  [Option('t', "tile-size", Required = false, Default = 10, HelpText = "Tile size in pixels.")]
  public int TileSize { get; set; }

  [Option('o', "output", Required = false, Default = "output.png", HelpText = "Output file-name.")]
  public string FileName { get; set; } = "output.png";

  [Option("hsv1", Required = false, HelpText = "Color1 in HSV format (Hue in degrees, Sat 0.0 to 1.0, Val 0.0 to 1.0).")]
  public IEnumerable<float>? Hsv1 { get; set; }

  [Option("hsv2", Required = false, HelpText = "Color2 in HSV format (Hue in degrees, Sat 0.0 to 1.0, Val 0.0 to 1.0).")]
  public IEnumerable<float>? Hsv2 { get; set; }
}

internal class Program
{
  // Constants for tile colors
  private static Rgba32 FirstColor  = new Rgba32(0x20, 0x20, 0xFF); // default is Blue (#2020ff)
  private static Rgba32 SecondColor = new Rgba32(0xFF, 0x20, 0x20); // default is Red  (#ff2020)

  static void Main (string[] args)
  {
    Parser.Default.ParseArguments<Options>(args)
       .WithParsed<Options>(o =>
       {
         if (o.Hsv1 != null && o.Hsv1.Count() >= 3)
         {
           var hsvList = o.Hsv1.ToList();
           var hsvColor = new Hsv(hsvList[0], hsvList[1], hsvList[2]);
           var rgbColor = ColorSpaceConverter.ToRgb(hsvColor);
           FirstColor = new Rgba32(rgbColor.R, rgbColor.G, rgbColor.B);
         }

         if (o.Hsv2 != null && o.Hsv2.Count() >= 3)
         {
           var hsvList = o.Hsv2.ToList();
           var hsvColor = new Hsv(hsvList[0], hsvList[1], hsvList[2]);
           var rgbColor = ColorSpaceConverter.ToRgb(hsvColor);
           SecondColor = new Rgba32(rgbColor.R, rgbColor.G, rgbColor.B);
         }

         // Create a new image with the specified dimensions
         using (var image = new Image<Rgba32>(o.Width, o.Height))
         {
           for (int y = 0; y < o.Height; y++)
           {
             for (int x = 0; x < o.Width; x++)
             {
               // Determine the tile color based on position
               Rgba32 tileColor = ((x / o.TileSize) + (y / o.TileSize)) % 2 == 0
                ? FirstColor   // even tiles
                : SecondColor; // odd tiles

               // Set the pixel color
               image[x, y] = tileColor;
             }
           }

           // Save the image to a file with the specified filename
           image.Save(o.FileName);

           Console.WriteLine($"Image '{o.FileName}' created successfully.");
         }
       });
  }
}
