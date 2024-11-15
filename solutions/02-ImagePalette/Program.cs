using System;
using System.Xml;
using System.Collections.Generic;
using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _02_ImagePalette;

public class Options
{
  [Option('o', "output", Required = false, Default = null, HelpText = "Output file-name (SVG).")]   // Default '' will return colors in RGB; if '.svg' is provided, it will return colors in that format.
  public string OutputFileName { get; set; } = string.Empty;
  [Option('i', "input", Required = true, HelpText = "Input image file-name.")]
  public string InputFileName { get; set; }
  [Option('c', "colors", Required = true, HelpText = "Desired number of colors.")]
  public int NumberofColors { get; set; } 
}

class Program
{
  static void Main (string[] args)
  {
    Parser.Default.ParseArguments<Options>(args)
      .WithParsed<Options>(o =>
      {
        if(o.NumberofColors < 3 || o.NumberofColors > 10)
        {
          throw new ArgumentException("Program provides only 3 to 10 characteristic colors");
        }
        if(!o.OutputFileName.EndsWith(".svg") && !string.IsNullOrEmpty(o.OutputFileName))
        {
          throw new ArgumentException("Name of the output file should have the extension .svg, or be empty if RGB description wanted.");
        }
        try
        {
          var  image = Image.Load<Rgba32>(o.InputFileName);    // Load image and save to the variable `image`.
          List<Hsv> hsvPixels = new List<Hsv>();               // List to store colors in HSV space.
          int step = Math.Max(1, Math.Min(image.Width, image.Height) / 500); // Define dynamic step.
          for (int y = 0; y < image.Height; y += step)
          {
            for (int x = 0; x < image.Width; x += step)
            {
                var pixel = image[x, y];
                var hsv = Rgba32ToHsv(pixel);
                hsvPixels.Add(hsv);
            }
          }
          var clusters = KMeansCluster(hsvPixels, o.NumberofColors);
          // Convert clusters to RGB
          var rgbColors = clusters.Select(HsvToRgba32).ToList();

          if (string.IsNullOrEmpty(o.OutputFileName))
          {
              // Sort colors by spectrum (hue, saturation, value)
              rgbColors = rgbColors
                  .Select(color => (Color: color, Hsv: Rgba32ToHsv(color))) // Convert to HSV for sorting
                  .OrderBy(tuple => tuple.Hsv.H) // Sort by hue
                  .ThenBy(tuple => tuple.Hsv.S) // Secondary criterion: saturation
                  .ThenBy(tuple => tuple.Hsv.V) // Tertiary criterion: value
                  .Select(tuple => tuple.Color) // Convert back to RGB
                  .ToList();

              // Return colors as text in RGB format
              foreach (var color in rgbColors)
              {
                  Console.WriteLine($"RGB({color.R}, {color.G}, {color.B})");
              }
          }
          else
          {
              // Sort colors by spectrum before generating the SVG
              rgbColors = rgbColors
                  .Select(color => (Color: color, Hsv: Rgba32ToHsv(color)))
                  .OrderBy(tuple => tuple.Hsv.H)
                  .ThenBy(tuple => tuple.Hsv.S)
                  .ThenBy(tuple => tuple.Hsv.V)
                  .Select(tuple => tuple.Color)
                  .ToList();

              // Generate SVG file
              GenerateSvg(o.OutputFileName, rgbColors);
              Console.WriteLine($"Palette saved to {o.OutputFileName}");
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Error on storing the image: {ex.Message}");
        }
      });
  }

  public struct Hsv
  {
    public float H { get; }
    public float S { get; }
    public float V { get; }

    public Hsv(float h, float s, float v)
    {
        H = h;
        S = s;
        V = v;
    }
  }

  static Hsv Rgba32ToHsv(Rgba32 rgba)
  {
    float r = rgba.R / 255f;
    float g = rgba.G / 255f;
    float b = rgba.B / 255f;
    
    // Get the maximum and minimum values of the three colors
    float max = Math.Max(r, Math.Max(g, b));
    float min = Math.Min(r, Math.Min(g, b));
    float delta = max - min;

    // Hue
    float h = 0f;
    if (delta != 0)
    {
        if (max == r)
            h = (g - b) / delta;
        else if (max == g)
            h = (b - r) / delta + 2f;
        else
            h = (r - g) / delta + 4f;
        h *= 60f;
        if (h < 0f) h += 360f;
    }

    // Saturation
    float s = (max == 0) ? 0 : delta / max;

    // Value
    float v = max;
    return new Hsv(h, s, v);
  }

  static Rgba32 HsvToRgba32(Hsv hsv)
  {
      float h = hsv.H / 360f;
      float s = hsv.S;
      float v = hsv.V;

      float r = 0f, g = 0f, b = 0f;
      if (s == 0)
      {
          r = g = b = v; // Gray
      }
      else
      {
          float i = (float)Math.Floor(h * 6);
          float f = h * 6 - i;
          float p = v * (1 - s);
          float q = v * (1 - f * s);
          float t = v * (1 - (1 - f) * s);

          i = i % 6;
          if (i == 0) { r = v; g = t; b = p; }
          else if (i == 1) { r = q; g = v; b = p; }
          else if (i == 2) { r = p; g = v; b = t; }
          else if (i == 3) { r = p; g = q; b = v; }
          else if (i == 4) { r = t; g = p; b = v; }
          else { r = v; g = p; b = q; }
      }

      return new Rgba32((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
  }
  
  // K-means clustering algorithm
  static List<Hsv> KMeansCluster(List<Hsv> colors, int k)
  {
    Random rand = new Random();
    // Select K random colors as initial centroids
    var centroids = colors.OrderBy(x => rand.Next()).Take(k).ToList();
    var previousCentroids = new List<Hsv>();

    List<int> assignments = new List<int>();

    while (!centroids.SequenceEqual(previousCentroids))
    {
        // Assign each color to the closest centroid
        assignments.Clear();
        for (int i = 0; i < colors.Count; i++)
        {
            var closestCentroid = GetClosestCentroid(colors[i], centroids);
            assignments.Add(closestCentroid);
        }

        // Save the previous centroids
        previousCentroids = new List<Hsv>(centroids);

        // Recalculate new centroids as the average of assigned colors
        for (int i = 0; i < k; i++)
        {
            var assignedColors = colors.Where((c, idx) => assignments[idx] == i).ToList();
            if (assignedColors.Count > 0)
            {
                centroids[i] = CalculateCentroid(assignedColors);
            }
        }
      }

      return centroids;
  }

  static int GetClosestCentroid(Hsv color, List<Hsv> centroids)
  {
    int closestIndex = -1;
    float closestDistance = float.MaxValue;

    for (int i = 0; i < centroids.Count; i++)
    {
        float distance = CalculateDistance(color, centroids[i]);
        if (distance < closestDistance)
        {
            closestDistance = distance;
            closestIndex = i;
        }
    }

    return closestIndex;
  }

  static float CalculateDistance(Hsv c1, Hsv c2)
  {
    // Use Euclidean distance in HSV space
    return (float)Math.Sqrt(Math.Pow(c1.H - c2.H, 2) + Math.Pow(c1.S - c2.S, 2) + Math.Pow(c1.V - c2.V, 2));
  }

  static Hsv CalculateCentroid(List<Hsv> colors)
  {
    float avgH = colors.Average(c => c.H);
    float avgS = colors.Average(c => c.S);
    float avgV = colors.Average(c => c.V);
    return new Hsv(avgH, avgS, avgV);
  }
  static void GenerateSvg(string outputFileName, List<Rgba32> colors)
  {
    using XmlWriter writer = XmlWriter.Create(outputFileName);
    writer.WriteStartDocument();
    writer.WriteStartElement("svg", "http://www.w3.org/2000/svg");
    writer.WriteAttributeString("width", "800");
    writer.WriteAttributeString("height", "200");

    int barWidth = 800 / colors.Count;
    for (int i = 0; i < colors.Count; i++)
    {
        writer.WriteStartElement("rect");
        writer.WriteAttributeString("x", (i * barWidth).ToString());
        writer.WriteAttributeString("y", "0");
        writer.WriteAttributeString("width", barWidth.ToString());
        writer.WriteAttributeString("height", "200");
        writer.WriteAttributeString("fill", $"rgb({colors[i].R}, {colors[i].G}, {colors[i].B})");
        writer.WriteEndElement();
    }

    writer.WriteEndElement();
    writer.WriteEndDocument();
  }
}
