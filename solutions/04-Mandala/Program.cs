using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.IO;


class Program
{
    static void Main(string[] args)
    {
        int width = 800;
        int height = 800;
        PointF center = new PointF(width / 2, height / 2);
        
        Mandalas mandala1 = new Mandalas();
        mandala1.FirstImplementation(center, width, height);

        Mandalas mandala2 = new Mandalas();
        mandala2.Flower(center, width, height);

        Mandalas mandala3 = new Mandalas();
        



    }

}
class Mandalas
{
    public void FirstImplementation(PointF center, int width, int height)
    {
        using var image = new Image<Rgba32>(width, height, Color.White);
        int circleCount = 20;
        int segmentCount = 12;
        float maxRadius = Math.Min(width, height) / 2 - 10;

        for (int i = 1; i <= circleCount; i++)
        {
            float radius = (maxRadius / circleCount) * i; 

    
            var pen = Pens.Solid(Color.FromRgb((byte)(i * 20), 100, 200), 2);
            image.Mutate(ctx => ctx.Draw(pen, new EllipsePolygon(center, radius)));

            for (int j = 0; j < segmentCount; j++)
            {
                
                double angle = (Math.PI * 2 / segmentCount) * j;
            
                PointF start = new PointF(
                    center.X + (float)(radius * Math.Cos(angle)),
                    center.Y + (float)(radius * Math.Sin(angle))
                );
                PointF end = new PointF(
                    center.X + (float)(maxRadius * Math.Cos(angle)),
                    center.Y + (float)(maxRadius * Math.Sin(angle))
                );
                // Crear la geometría de la línea
                var pathBuilder = new PathBuilder();
                pathBuilder.AddLine(start, end);
                IPath line = pathBuilder.Build();
                image.Mutate(ctx => ctx.Draw(pen, line));   //Dibujar
            }
        }
        image.Save("first.png");
        Console.WriteLine("Mandala first.png generated");
        //Process.Start(new ProcessStartInfo("first.png") { UseShellExecute = true });
    }
    
    public void Flower(PointF center, int width, int height)
    {
        int hexaCount = 4;
        using var image = new Image<Rgba32>(width, height, Color.White);
        float maxRadius = Math.Min(width, height) / 2 - 10;
        var points = new PointF[6];
        for (int i = 1; i <= hexaCount; i++)
        {
            
            float radius = (maxRadius / hexaCount) * i; 
            float smallradius = radius * (float)(1 / Math.Sqrt(2));
            //Genera Hexagono
            for (int j = 0; j < 6; j++)
            {
                float angle = (float)(j * Math.PI / 3); // 60 grados
                points[j] = new PointF(center.X + radius * (float)Math.Cos(angle), center.Y + radius * (float)Math.Sin(angle));
            }
            image.Mutate(ctx => ctx.DrawPolygon(Color.Black, 3, points));
            image.Mutate(ctx => 
            {
                for (int i = 1; i <= hexaCount; i++)
                {
                    float radius = (maxRadius / hexaCount) * i; 
                    var points = new PointF[6];
                    for (int j = 0; j < 6; j++)
                    {
                        float angle = (float)(j * Math.PI / 3); // 60 grados para hexágonos
                        points[j] = new PointF(
                            center.X + radius * (float)Math.Cos(angle),
                            center.Y + radius * (float)Math.Sin(angle)
                        );
                    }
                    float angleOfRotation = (float)(Math.PI / 2); // 90 grados en radianes
                    for (int j = 0; j < 6; j++)
                    {
                        float x = points[j].X;
                        float y = points[j].Y;

                        points[j] = new PointF(
                            center.X + (x - center.X) * (float)Math.Cos(angleOfRotation) - (y - center.Y) * (float)Math.Sin(angleOfRotation),
                            center.Y + (x - center.X) * (float)Math.Sin(angleOfRotation) + (y - center.Y) * (float)Math.Cos(angleOfRotation)
                        );
                    }
                    ctx.DrawPolygon(Color.Black, 3, points);
                }
            });
        }
        image.Save("flower.png");
        Console.WriteLine("Mandala flower.png generated");
        Process.Start(new ProcessStartInfo("flower.png") { UseShellExecute = true });
    }
    
}
