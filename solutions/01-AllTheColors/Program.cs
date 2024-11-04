using System;
using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing; 
using System.Collections.Generic;

public class Options
{
    [Option('m', "Mode", Required = true, HelpText = "Trivial mode: 1, Random mode: 2, Ornament mode: 3")]
    public int Mode { get; set; }

    [Option('o', "Output", Required = true, HelpText = "Out put file name with .png extension")]
    public string OutputFileName { get; set; }

    [Option('w', "width", Required = false, Default = 4096, HelpText = "Image width in pixels.")]
    public int Width { get; set; }

    [Option('h', "height", Required = false, Default = 4096, HelpText = "Image height in pixels.")]
    public int Height { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
        .WithParsed<Options>(o =>
        {
            if (o.Mode < 1 || o.Mode > 3)
            {
                throw new ArgumentException("Mode should be 1 (Trivial mode), 2(Random mode) or 3(ornamentmode)");
            }
            if (!o.OutputFileName.EndsWith(".png"))
            {
                throw new ArgumentException("Name of the output file should have the extension .png.");
            }
            //Indicate wich Mode is activated
            Console.WriteLine($"Mode {o.Mode} activated");

            int width = o.Width;
            int height = o.Height;
            int maxcombinationcolors = 16777216;

            if (width * height < maxcombinationcolors)
            {
                throw new ArgumentException("The image must contain at least 2^24 pixels.");
            }
            switch (o.Mode)
            {
                case 1: // Mode Trivial
                    CreateTrivialImage(width, height, o.OutputFileName);
                    break;
                case 2: // Mode Random
                    CreateRandomImage(width, height, o.OutputFileName);
                    break;
                case 3: // Mode Ornament
                    CreateOrnamentImage(width, height, o.OutputFileName);
                    break;
            }

        });
    }     

    static void CreateTrivialImage(int width,int height,string fileName)
    {
        using (var image = new Image<Rgba32>(width, height))
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
                                image[x, y] = new Rgba32((byte)r, (byte)g, (byte)b);
                                pixelIndex++;
                            }
                        }
                    }
                    image.Save(fileName);
                }

                Console.WriteLine($"Image {fileName} created successfully.");
    }
    
    static void CreateRandomImage(int width, int height, string fileName)
    {
        int maxcombinationcolors = 16777216;
        int colorIndex = 0;
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

                using (var image = new Image<Rgba32>(width, height))
                {
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            image[x, y] = colors[colorIndex];
                            if (colorIndex < maxcombinationcolors - 1)
                            {
                                colorIndex++;
                            }
                            else
                            {
                                colorIndex = 0;
                            }
                        }
                    }
                    image.Save(fileName);
                }
                Console.WriteLine($"Image {fileName} created successfully.");
    }

    static void CreateOrnamentImage(int width, int height, string fileName)
    {
        int colorIndex = 0;
        List<Rgba32> colors1 = new List<Rgba32>();
                for (int r = 0; r < 256; r++)
                {
                    for (int g = 0; g < 256; g++)
                    {
                        for (int b = 0; b < 256; b++)
                        {
                            colors1.Add(new Rgba32((byte)r, (byte)g, (byte)b));
                        }
                    }
                }

                using (var image = new Image<Rgba32>(width, height))
                {
                    int x = width / 2;
                    int y = height / 2;
                    colorIndex = 0; 

                    image[x, y] = colors1[colors1.Count - 1]; 
                    FillAroundPixel(image, x, y, colors1);

                    image.Save(fileName);
                }
                Console.WriteLine($"Image {fileName} created successfully.");
    }
    
    static void FillAroundPixel(Image<Rgba32> image, int startX, int startY, List<Rgba32> colors)
    {
        int width = image.Width;
        int height = image.Height;

        // Create an array to keep track of processed pixels
        bool[,] processed = new bool[width, height];

        // Initialize the list of pixels to process
        var pixelsToProcess = new Queue<(int x, int y)>();
        pixelsToProcess.Enqueue((startX, startY));

        // Directions for adjacent pixels
        var directions = new (int x, int y)[]
        {
            (0, -1), // Up
            (1, 0),  // Right
            (0, 1),  // Down
            (-1, 0)  // Left
        };

        int colorIndex = 0; // Start the color index

        // Process until there are no more pixels to process
        while (pixelsToProcess.Count > 0)
        {
            var (x, y) = pixelsToProcess.Dequeue();

            // Check boundaries
            if (x < 0 || x >= width || y < 0 || y >= height || processed[x, y])
                continue;

            // Set the color of the current pixel
            image[x, y] = colors[colorIndex % colors.Count]; // Assign color from the list
            processed[x, y] = true;

            // Update the color index
            colorIndex++;
            
            // Add adjacent pixels to the queue
            foreach (var (dx, dy) in directions)
            {
                pixelsToProcess.Enqueue((x + dx, y + dy));
            }
        }
    }
}
