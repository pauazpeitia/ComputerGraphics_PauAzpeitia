using System;                      
using System.IO;                   
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using CommandLine;

public class Options
{
  [Option('w', "width", Required = false, Default = 800, HelpText = "Width of the image")]
  public int Width { get; set; } = 800;
  
  [Option('h', "height", Required = false, Default = 800, HelpText = "Height of the image")]
  public int Height { get; set; } = 800;

}

namespace _05_Animation
{
    class Program
    {
            static void Main(string[] args)
            {
                Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    int width = o.Width;
                    int height = o.Height;

                    using var image = new Image<Rgba32>(width, height, Color.White);
                    PointF center = new PointF(width / 2f, height / 2f);

                    int frameIndex = 0;

                    Mandalas mandala = new Mandalas();
                    mandala.Flower(image, center, width, height, ref frameIndex);
                });
            }
    }
    class Mandalas
    {
        public void Flower(Image<Rgba32> image, PointF center, int width, int height, ref int frameIndex)
        {
            int hexaCount = 8;
            int segmentCount = 24;
            float maxRadius = Math.Min(width, height) / 2f - 10f;

            //circulo central
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.HotPink, new EllipsePolygon(center, 20));
            });
            SaveFrame(image, ref frameIndex);

            // rayos
            float startRadius = maxRadius / hexaCount;
            for (int j = 0; j < segmentCount; j++)
            {
                var pen = Pens.Solid(
                    Color.FromRgb((byte)(j * 20), 100, 200),
                    3
                );
                double angle = (Math.PI * 2 / segmentCount) * j;

                PointF start = new PointF(
                    center.X + (float)(startRadius * Math.Cos(angle)),
                    center.Y + (float)(startRadius * Math.Sin(angle))
                );
                PointF end = new PointF(
                    center.X + (float)(maxRadius * Math.Cos(angle)),
                    center.Y + (float)(maxRadius * Math.Sin(angle))
                );

                var pathBuilder = new PathBuilder();
                pathBuilder.AddLine(start, end);
                IPath line = pathBuilder.Build();

                image.Mutate(ctx =>
                {
                    ctx.Draw(pen, line);
                });
                SaveFrame(image, ref frameIndex);
            }

            //hexagonos 
            for (int i = 1; i <= hexaCount; i++)
            {
                float radius = (maxRadius / hexaCount) * i;
                PointF[] points = GenerateHex(center, radius, 0);
                image.Mutate(ctx =>
                {
                    ctx.DrawPolygon(
                        Color.FromRgb((byte)(i * 40), 100, 200),
                        3,
                        points
                    );
                });
                SaveFrame(image, ref frameIndex);
            }

            for (int i = 1; i <= hexaCount; i++)
            {
                float radius = (maxRadius / hexaCount) * i;
                PointF[] points = GenerateHex(center, radius, MathF.PI / 4);
                image.Mutate(ctx =>
                {
                    ctx.DrawPolygon(
                        Color.FromRgb((byte)(i * 40), 200, 100),
                        3,
                        points
                    );
                });
                SaveFrame(image, ref frameIndex);
            }

            for (int i = 1; i <= hexaCount; i++)
            {
                float radius = (maxRadius / hexaCount) * i;
                PointF[] points = GenerateHex(center, radius, MathF.PI / 2);
                image.Mutate(ctx =>
                {
                    ctx.DrawPolygon(
                        Color.FromRgb((byte)(i * 40), 100, 200),
                        3,
                        points
                    );
                });
                SaveFrame(image, ref frameIndex);
            }

            for (int i = 1; i <= hexaCount; i++)
            {
                float radius = (maxRadius / hexaCount) * i;
                PointF[] points = GenerateHex(center, radius, -MathF.PI / 4);
                image.Mutate(ctx =>
                {
                    ctx.DrawPolygon(
                        Color.FromRgb((byte)(i * 40), 200, 100),
                        3,
                        points
                    );
                });
                SaveFrame(image, ref frameIndex);
            }
        }
        private PointF[] GenerateHex(PointF center, float radius, float rotationAngle)
        {
            var points = new PointF[6];
            for (int j = 0; j < 6; j++)
            {
                float angle = j * MathF.PI / 3f; 
                float x = center.X + radius * MathF.Cos(angle);
                float y = center.Y + radius * MathF.Sin(angle);

                if (rotationAngle != 0)
                {
                    float dx = x - center.X;
                    float dy = y - center.Y;

                    float rotatedX = dx * MathF.Cos(rotationAngle) - dy * MathF.Sin(rotationAngle);
                    float rotatedY = dx * MathF.Sin(rotationAngle) + dy * MathF.Cos(rotationAngle);

                    x = center.X + rotatedX;
                    y = center.Y + rotatedY;
                }

                points[j] = new PointF(x, y);
            }

            return points;
        }
        private void SaveFrame(Image<Rgba32> image, ref int frameIndex)
        {
            string folderPath = "Frames";

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string fileName = System.IO.Path.Combine(folderPath, $"out{frameIndex:0000}.png");
            image.Save(fileName);
            Console.WriteLine($"Frame {frameIndex} -> {fileName} generated");
            frameIndex++;
        }
    }
}
