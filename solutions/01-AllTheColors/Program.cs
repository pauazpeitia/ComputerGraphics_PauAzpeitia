using System;
using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing; 
using System.Collections.Generic;

public class Options
{
    [Option('t', "trivialMode", Required = false, Default = true, HelpText = "Trivial mode?")]
    public bool TrivialMode { get; set; }

    [Option('r', "randomMode", Required = false, Default = true, HelpText = "Random mode?")]
    public bool RandomMode { get; set; }

    [Option('o', "ornamentMode", Required = false, Default = true,  HelpText = "Pattern?")]
    public bool OrnamentMode { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
        .WithParsed<Options>(o =>
        {
            if (o.TrivialMode) // Trivial mode
            {
                int width = 4096;
                int height = 4096;
                using (var image1 = new Image<Rgba32>(width, height))
                {
                    int pixelIndex = 0;
                    for (int r = 0; r < 256; r++)
                    {
                        for (int g = 0; g < 256; g++)
                        {
                            for (int b = 0; b < 256; b++)
                            {
                            
                                int x = pixelIndex % width;
                                int y = pixelIndex / width;

                                
                                image1[x, y] = new Rgba32((byte)r, (byte)g, (byte)b);
                                pixelIndex++;

                                
                            }
                        }
                    }

                    image1.Save("trivial.png");
                }

                Console.WriteLine("Image trivial.png created successfully.");
            }
            if(o.RandomMode) //Random mode
            {
                
                int width = 4096;
                int height = 4096;
                
                
                List<Rgba32> colors = new List<Rgba32>();
                for (int r = 0; r < 256; r++)
                {
                    for (int g = 0; g < 256; g++)
                    {
                        for (int b = 0; b < 256; b++)
                        {
                            colors.Add(new Rgba32((byte)r, (byte)g, (byte)b));
                        }
                    }
                }
                
                Random rng = new Random();
                int n = colors.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    var value = colors[k];
                    colors[k] = colors[n];
                    colors[n] = value;
                }

                
                using (var image1 = new Image<Rgba32>(width, height))
                {
                    int colorIndex = 0;
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            image1[x, y] = colors[colorIndex];
                            colorIndex++;
                        }
                    }
                    image1.Save("random.png");

                }
                Console.WriteLine("Image random.png created successfully.");

            }
            if (o.OrnamentMode) // Ornament mode
            {
                int width = 4096;
                int height = 4096;

                List<Rgba32> colors = new List<Rgba32>();
                for (int r = 0; r < 256; r++)
                {
                    for (int g = 0; g < 256; g++)
                    {
                        for (int b = 0; b < 256; b++)
                        {
                            colors.Add(new Rgba32((byte)r, (byte)g, (byte)b));
                        }
                    }
                }

               // Ordenar los colores por luminosidad (más oscuros a más claros)
            colors.Sort((c1, c2) =>
            {
                // Calcular la luminosidad
                double luminance1 = 0.2126 * c1.R + 0.7152 * c1.G + 0.0722 * c1.B;
                double luminance2 = 0.2126 * c2.R + 0.7152 * c2.G + 0.0722 * c2.B;

                // Ordenar de menor a mayor luminosidad
                return luminance1.CompareTo(luminance2);
            });



                using (var image1 = new Image<Rgba32>(width, height))
                {
                    int colorIndex = 0;
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            image1[x, y] = colors[colorIndex];
                            colorIndex++;
                        }
                    }
                    image1.Save("ornament.png");

                }
                Console.WriteLine("Image ornament.png created successfully.");
            }


        
        });
    }
}
