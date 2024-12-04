using System;
using System.Xml;
using System.Collections.Generic;
using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace _02_ImagePalette;

public class Options
{
  [Option('o', "output", Required = false, Default = "", HelpText = "Output file-name (SVG).")]   //Default '' sera para que lo devuelva en RGB, si pone .svg que lo devuelva de esa manera.
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
        var image = Image.Load<Rgba32>(o.InputFileName);    //Leer imagen y guardar en variable image

        // Lista para almacenar los colores en espacio HSV
        List<Hsv> hsvPixels = new List<Hsv>();

        for (int y = 0; y < image.Height; y++)
        {
          for (int x = 0; x < image.Width; x++)
          {
              var pixel = image[x, y];
              var hsv = Rgba32ToHsv(pixel);
              hsvPixels.Add(hsv);
          }
        }


        //Aplicar algorithmo
        var clusters = KMeansCluster(hsvPixels, o.NumberofColors);
        // Convertir clusters a RGB
        var rgbColors = clusters.Select(HsvToRgba32).ToList();

        if (string.IsNullOrEmpty(o.OutputFileName))
        {
            // Devolver colores como texto en formato RGB
            foreach (var color in rgbColors)
            {
                Console.WriteLine($"RGB({color.R}, {color.G}, {color.B})");
            }
        }
        else
        {
            // Generar archivo SVG
            GenerateSvg(o.OutputFileName, rgbColors);
            Console.WriteLine($"Palette saved to {o.OutputFileName}");
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
    
    // Obtener el valor máximo y mínimo de los tres colores
    float max = Math.Max(r, Math.Max(g, b));
    float min = Math.Min(r, Math.Min(g, b));
    float delta = max - min;

    // Matiz (Hue)
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

    // Saturación (Saturation)
    float s = (max == 0) ? 0 : delta / max;

    // Valor (Value)
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
          r = g = b = v; // Gris
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
  
  //Algorithmo K-means cluster
  static List<Hsv> KMeansCluster(List<Hsv> colors, int k)
  {
    Random rand = new Random();
    // Seleccionar K colores aleatorios como centros iniciales
    var centroids = colors.OrderBy(x => rand.Next()).Take(k).ToList();
    var previousCentroids = new List<Hsv>();

    List<int> assignments = new List<int>();

    while (!centroids.SequenceEqual(previousCentroids))
    {
        // Asignar cada color al centroide más cercano
        assignments.Clear();
        for (int i = 0; i < colors.Count; i++)
        {
            var closestCentroid = GetClosestCentroid(colors[i], centroids);
            assignments.Add(closestCentroid);
        }

        // Guardar los centroids anteriores
        previousCentroids = new List<Hsv>(centroids);

        // Recalcular los nuevos centroids como el promedio de los colores asignados
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
    // Usamos la distancia euclidiana en el espacio HSV
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
    using (XmlWriter writer = XmlWriter.Create(outputFileName, new XmlWriterSettings { Indent = true }))
    {
        writer.WriteStartDocument();
        writer.WriteStartElement("svg", "http://www.w3.org/2000/svg");
        writer.WriteAttributeString("width", "100");
        writer.WriteAttributeString("height", (50 * colors.Count).ToString());

        for (int i = 0; i < colors.Count; i++)
        {
            var color = colors[i];
            writer.WriteStartElement("rect");
            writer.WriteAttributeString("x", "0");
            writer.WriteAttributeString("y", (i * 50).ToString());
            writer.WriteAttributeString("width", "100");
            writer.WriteAttributeString("height", "50");
            writer.WriteAttributeString("fill", $"rgb({color.R},{color.G},{color.B})");
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        writer.WriteEndDocument();
    }
  }

}
