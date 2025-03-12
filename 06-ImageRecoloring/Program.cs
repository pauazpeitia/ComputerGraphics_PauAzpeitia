using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using CommandLine;
namespace _06_ImageRecoloring
{
    public class Options
    {
        [Option('i', "input", Required = true, Default = "output.png", HelpText = "Specify the input image")]
        public string Input { get; set; }
        
        [Option('o', "output", Required = false, Default = "output.png", HelpText = "Output file-name (.png).")]
        public string Output { get; set; } = "output.jpg";

        [Option('h', "hue", Required = false, Default = 0, HelpText = "Number to use for Hue recoloring")]
        public float Hue { get; set; } = 0;
    }
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    if(!(o.Output.EndsWith(".png") || o.Output.EndsWith(".jpg")))
                    {
                        throw new ArgumentException("Name of the output file should have the extension .png or .jpg");
                    }
                    string inputPath = o.Input;
                    string outputPath = o.Output;
                    float hueShift = o.Hue;
                    try
                    {
                        using (var image = Image.Load<Rgba32>(inputPath))
                        {
                            for (int y = 0; y < image.Height; y++)
                            {
                                for (int x = 0; x < image.Width; x++)
                                {
                                    var pixel = image[x, y];

                                    float r = pixel.R / 255f;
                                    float g = pixel.G / 255f;
                                    float b = pixel.B / 255f;

                                    var hsv = RgbToHsv(r, g, b);

                                    if (IsSkinTone(hsv))
                                    {
                                        //skin
                                        continue;
                                    }
                                    else
                                    {
                                        hsv.H = (hsv.H + hueShift) % 360;
                                        if (hsv.H < 0) hsv.H += 360; //negativecase
                                        var newRgb = HsvToRgb(hsv.H, hsv.S, hsv.V);
                                        image[x, y] = new Rgba32((byte)(newRgb.R * 255), (byte)(newRgb.G * 255), (byte)(newRgb.B * 255));
                                    }
                                }
                            }
                            image.Save(outputPath);
                        }
                    }catch (Exception ex)
                    {
                        Console.WriteLine($"Error on storing the image: {ex.Message}");
                    }
                    Console.WriteLine($"Image saved as {outputPath}");
                });
            }
        static bool IsSkinTone(Hsv hsv)
        {
            return hsv.H >= 0f && hsv.H <= 40f || hsv.H >= 340f && hsv.H <= 360f &&
                   hsv.S >= 0.2f && hsv.S <= 0.7f &&
                   hsv.V >= 0.3f && hsv.V <= 0.9f;
        }
        static Hsv RgbToHsv(float r, float g, float b)
        {
            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            float h = 0f;
            float s = (max == 0) ? 0 : (delta / max);
            float v = max;

            if (delta != 0)
            {
                if (max == r)
                {
                    h = (g - b) / delta;
                }
                else if (max == g)
                {
                    h = (b - r) / delta + 2;
                }
                else if (max == b)
                {
                    h = (r - g) / delta + 4;
                }

                h *= 60;
                if (h < 0) h += 360;
            }
            return new Hsv(h, s, v);
        }
        static (float R, float G, float B) HsvToRgb(float h, float s, float v)
        {
            float c = v * s;
            float x = c * (1 - Math.Abs(((h / 60) % 2) - 1));
            float m = v - c;
            float r = 0, g = 0, b = 0;

            if (0 <= h && h < 60)
            {
                r = c; g = x; b = 0;
            }
            else if (60 <= h && h < 120)
            {
                r = x; g = c; b = 0;
            }
            else if (120 <= h && h < 180)
            {
                r = 0; g = c; b = x;
            }
            else if (180 <= h && h < 240)
            {
                r = 0; g = x; b = c;
            }
            else if (240 <= h && h < 300)
            {
                r = x; g = 0; b = c;
            }
            else if (300 <= h && h < 360)
            {
                r = c; g = 0; b = x;
            }
            r += m;g += m;b += m;
            return (r, g, b);
        }
    }
    public struct Hsv
    {
        public float H { get; set; } //0-360
        public float S { get; set; } // 0-1
        public float V { get; set; } // 0-1
        public Hsv(float h, float s, float v)
        {
            H = h;S = s;V = v;
        }
    }
}
