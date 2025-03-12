using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using CommandLine;

namespace _04_Mandala;

public class Options
{
  [Option('o', "output", Required = false, Default = "output.png", HelpText = "Output file-name (.png).")]
  public string FileName { get; set; } = "output.svg";

}
class Program
{
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
      .WithParsed<Options>(o =>
      {
            if(!(o.FileName.EndsWith(".png") || o.FileName.EndsWith(".jpg")))
            {
                throw new ArgumentException("Name of the output file should have the extension .png or .jpg");
            }

            int width = 800;
            int height = 800;
            using var image = new Image<Rgba32>(width, height, Color.White);
            PointF center = new PointF(width / 2, height / 2);
            
            Mandalas mandala1 = new Mandalas();
            mandala1.Flower(image, center, width, height);
            image.Save(o.FileName);
            Console.WriteLine($"Mandala {o.FileName} generated");
      });
    }
}
class Mandalas
{    
    public void Flower(Image image,PointF center, int width, int height)
    {
        int hexaCount = 8;
        int segmentCount = 24;
        float maxRadius = Math.Min(width, height) / 2 - 10;
        var points = new PointF[6];
        float startradius = maxRadius / hexaCount;
        image.Mutate(ctx => ctx.Fill(Color.HotPink, new EllipsePolygon(center, 20)));
        for (int j = 0; j < segmentCount; j++)
        {
            var pen = Pens.Solid(Color.FromRgb((byte)(j * 20), 100, 200), 3);
            double angle = (Math.PI * 2 / segmentCount) * j;
        
            PointF start = new PointF(
                center.X + (float)(startradius * Math.Cos(angle)),
                center.Y + (float)(startradius * Math.Sin(angle))
            );
            PointF end = new PointF(
                center.X + (float)(maxRadius * Math.Cos(angle)),
                center.Y + (float)(maxRadius * Math.Sin(angle))
            );
            
            var pathBuilder = new PathBuilder();
            pathBuilder.AddLine(start, end);
            IPath line = pathBuilder.Build();
            image.Mutate(ctx => ctx.Draw(pen, line));   //Dibujar
        }
        for (int i = 1; i <= hexaCount; i++)
        {
            float radius = (maxRadius / hexaCount) * i; 
            points = GenerateHex(image, center, radius, 0);
            image.Mutate(ctx => ctx.DrawPolygon(Color.FromRgb((byte)(i * 40), 100, 200), 3, points));
        }
        
        for (int i = 1; i <= hexaCount; i++)
        {
            float radius = (maxRadius / hexaCount) * i; 
            points = GenerateHex(image, center, radius,  MathF.PI / 4);
            image.Mutate(ctx => ctx.DrawPolygon(Color.FromRgb((byte)(i * 40), 200, 100), 3, points));
        }
        for (int i = 1; i <= hexaCount; i++)
        {
            float radius = (maxRadius / hexaCount) * i; 
            points = GenerateHex(image, center, radius,  MathF.PI / 2);
            image.Mutate(ctx => ctx.DrawPolygon(Color.FromRgb((byte)(i * 40), 100, 200), 3, points));
        }
        for (int i = 1; i <= hexaCount; i++)
        {
            float radius = (maxRadius / hexaCount) * i; 
            points = GenerateHex(image, center, radius,  -MathF.PI/4);
            image.Mutate(ctx => ctx.DrawPolygon(Color.FromRgb((byte)(i * 40), 200, 100), 3, points));
        }
    }
    public PointF[] GenerateHex(Image image, PointF center, float radius, float rotationAngle)
    {
        var points = new PointF[6];
        for (int j = 0; j < 6; j++)
        {
            float angle = (float)(j * Math.PI / 3); // 60 grados
            points[j] = new PointF(center.X + radius * (float)Math.Cos(angle), center.Y + radius * (float)Math.Sin(angle));
            float x = center.X + radius * MathF.Cos(angle);
            float y = center.Y + radius * MathF.Sin(angle);
            if(rotationAngle != 0)
            {
                points[j] = new PointF(
                center.X + (x - center.X) * MathF.Cos(rotationAngle) - (y - center.Y) * MathF.Sin(rotationAngle),
                center.Y + (x - center.X) * MathF.Sin(rotationAngle) + (y - center.Y) * MathF.Cos(rotationAngle));
            }
        }
        return points;
    }
}