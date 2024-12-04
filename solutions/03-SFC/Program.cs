﻿using CommandLine;
using System.Globalization;
using System.Xml;

namespace _03_SFC;

public class Options
{
  [Option('o', "output", Required = false, Default = "output.svg", HelpText = "Output file-name (SVG).")]
  public string FileName { get; set; } = "output.svg";

  [Option('w', "width", Required = false, Default = 400, HelpText = "Image width.")]
  public int Width { get; set; } = 400;

  [Option('h', "height", Required = false, Default = 400, HelpText = "Image height.")]
  public int Height { get; set; } = 400;

  [Option('d', "recursion depth", Required = false, Default = 1, HelpText = "Order of the Curve.")]
  public int Order { get; set; } = 1;

  [Option('c', "mode", Required = true, HelpText = "Choose curve (1,2,3).")]
  public int Mode { get; set; }

  [Option('t', "tone", Required = false, Default = 1, HelpText = "Choose color set (1-11).")]
  public int Color { get; set; } = 1;
}

class Program
{
  static void Main(string[] args)
  {
    Parser.Default.ParseArguments<Options>(args)
      .WithParsed<Options>(o =>
      {
        ColorRandomizer color = new ColorRandomizer(o.Color);

        int size = Math.Max(Math.Min(o.Width, o.Height) - 10, 5); 
        int order = o.Order;
        SVGHandler handler = new();
        XmlDocument svgDoc = handler.CreateSvg(size.ToString());
        handler.Contorno(svgDoc, size.ToString());
        XmlElement initialGroup = svgDoc.CreateElement("g");
        switch(o.Mode)
        {
          case 1:
            Console.WriteLine("Hilbert Curve"); 
            Hilbert hilbert = new();
            double decimalsize = (double)size;
            hilbert.HilbertFuncion(svgDoc, initialGroup, decimalsize/2, decimalsize/2, decimalsize, order, 0, o.Color);
            svgDoc.DocumentElement.AppendChild(initialGroup);
            svgDoc.Save(o.FileName);
            Console.WriteLine($"File saved as {o.FileName}");
            return;
          case 2:
            Console.WriteLine("Dragon Curve");
            Dragon dragon = new();

            
            int centerX = size / 2;
            int centerY = size / 2;
            int length = size / 4; 

            
            int x0 = centerX - length / 2;
            int y0 = centerY;
            int x1 = centerX + length / 2;
            int y1 = centerY;

            
            dragon.DragonCurve(svgDoc, initialGroup, x0, y0, x1, y1, order, o.Color);

            svgDoc.DocumentElement.AppendChild(initialGroup);
            svgDoc.Save(o.FileName);
            Console.WriteLine($"File saved as {o.FileName}");
            return;
          case 3:
            Console.WriteLine("Sierpinski Curve");
            Sierpinski sierpinski = new();
            
            int x0Sierpinski = size / 2;
            int y0Sierpinski = 10;
            int x1Sierpinski = 10;
            int y1Sierpinski = size - 10;
            int x2Sierpinski = size - 10;
            int y2Sierpinski = size - 10;

            sierpinski.SierpinskiCurve(svgDoc, initialGroup, x0Sierpinski, y0Sierpinski, x1Sierpinski, y1Sierpinski, x2Sierpinski, y2Sierpinski, order, "black", o.Color);
            svgDoc.DocumentElement.AppendChild(initialGroup);
            svgDoc.Save(o.FileName);
            Console.WriteLine($"File saved as {o.FileName}");
            return;
        
          
          default:
            throw new ArgumentException("Choose a valid mode (between 1-3)");
        }
      });
  }

}
class Hilbert
{
  public void HilbertFuncion(XmlDocument svgDoc, XmlElement parentGroup, double cx, double cy ,double size, int order, int angle, int numcolor)
  {
    XmlElement group = svgDoc.CreateElement("g");
    group.SetAttribute("transform", $"translate({cx.ToString(CultureInfo.InvariantCulture)}, {cy.ToString(CultureInfo.InvariantCulture)}) rotate({angle})");
    ColorRandomizer ran = new(numcolor);
    string color = ran.GetRandomColor(); 
    if(order > 1)
    {
      HilbertFuncion(svgDoc, group, -(size / 4), -(size / 4), size / 2, order - 1, 0, numcolor);
      HilbertFuncion(svgDoc, group, size / 4, -(size / 4), size / 2, order - 1, 0, numcolor);
      HilbertFuncion(svgDoc, group, -(size / 4), size / 4, size / 2, order - 1, 90, numcolor);
      HilbertFuncion(svgDoc, group, size / 4, size / 4, size / 2, order - 1, -90, numcolor);
    }

    int n = (int)Math.Pow(2, order +1); //key
    
    XmlElement line1 = svgDoc.CreateElement("line");
    line1.SetAttribute("x1", $"-{(size/2 - size/n).ToString(CultureInfo.InvariantCulture)}"); 
    line1.SetAttribute("y1", $"-{(size/n).ToString(CultureInfo.InvariantCulture)}"); 
    line1.SetAttribute("x2", $"-{(size/2 - size/n).ToString(CultureInfo.InvariantCulture)}"); 
    line1.SetAttribute("y2", $"{(size/n).ToString(CultureInfo.InvariantCulture)}"); 
    line1.SetAttribute("stroke", color); 
    line1.SetAttribute("stroke-width", "3");
    group.AppendChild(line1);
    
    XmlElement line2 = svgDoc.CreateElement("line");
    line2.SetAttribute("x1", $"-{(size/n).ToString(CultureInfo.InvariantCulture)}"); 
    line2.SetAttribute("y1", $"-{(size/n).ToString(CultureInfo.InvariantCulture)}"); 
    line2.SetAttribute("x2", $"{(size/n).ToString(CultureInfo.InvariantCulture)}"); 
    line2.SetAttribute("y2", $"-{(size/n).ToString(CultureInfo.InvariantCulture)}"); 
    line2.SetAttribute("stroke", color); 
    line2.SetAttribute("stroke-width", "3"); 
    group.AppendChild(line2);
    
    XmlElement line3 = svgDoc.CreateElement("line");
    line3.SetAttribute("x1", $"{(size/2 - size/n).ToString(CultureInfo.InvariantCulture)}"); 
    line3.SetAttribute("y1", $"-{(size/n).ToString(CultureInfo.InvariantCulture)}"); 
    line3.SetAttribute("x2", $"{(size/2 - size/n).ToString(CultureInfo.InvariantCulture)}"); 
    line3.SetAttribute("y2", $"{(size/n).ToString(CultureInfo.InvariantCulture)}"); 
    line3.SetAttribute("stroke", color);
    line3.SetAttribute("stroke-width", "3");
    group.AppendChild(line3);
    //Guardar
    parentGroup.AppendChild(group);
  }
}
class Sierpinski
{
  public void SierpinskiCurve(XmlDocument svgDoc, XmlElement parentGroup, int x0, int y0, int x1, int y1, int x2, int y2, int iterations, string colors, int numcolor)
  {
    ColorRandomizer ran = new(numcolor);
    string color = ran.GetRandomColor();
    if (iterations == 0)
    {
      DrawTriangle(svgDoc, parentGroup, x0, y0, x1, y1, x2, y2, color);
    }
    else
    {
      int midX01 = (x0 + x1) / 2;
      int midY01 = (y0 + y1) / 2;

      int midX12 = (x1 + x2) / 2;
      int midY12 = (y1 + y2) / 2;

      int midX20 = (x2 + x0) / 2;
      int midY20 = (y2 + y0) / 2;

      SierpinskiCurve(svgDoc, parentGroup, x0, y0, midX01, midY01, midX20, midY20, iterations - 1, color, numcolor);
      SierpinskiCurve(svgDoc, parentGroup, midX01, midY01, x1, y1, midX12, midY12, iterations - 1, color, numcolor);
      SierpinskiCurve(svgDoc, parentGroup, midX20, midY20, midX12, midY12, x2, y2, iterations - 1, color, numcolor);
    }
  }

  private void DrawTriangle(XmlDocument svgDoc, XmlElement parentGroup, int x0, int y0, int x1, int y1, int x2, int y2,string color)
  {
    DrawLine(svgDoc, parentGroup, x0, y0, x1, y1, color);
    DrawLine(svgDoc, parentGroup, x1, y1, x2, y2, color);
    DrawLine(svgDoc, parentGroup, x2, y2, x0, y0, color);
  }
  private void DrawLine(XmlDocument svgDoc, XmlElement parentGroup, int x0, int y0, int x1, int y1, string color)
  {    
    XmlElement line = svgDoc.CreateElement("line");
    line.SetAttribute("x1", x0.ToString());
    line.SetAttribute("y1", y0.ToString());
    line.SetAttribute("x2", x1.ToString());
    line.SetAttribute("y2", y1.ToString());
    line.SetAttribute("stroke", color);
    line.SetAttribute("stroke-width", "2");
    parentGroup.AppendChild(line);
  }
}
class ColorRandomizer
{
  private string[] Colors;

  public ColorRandomizer(int numeroColor)
  {
      
    switch (numeroColor)
    {
      case 1:
          Colors = new string[] { "#006D6F", "#A0A0A0", "#D79F32" };
          break;
      case 2:
          Colors = new string[] { "#FF5733", "#C70039", "#900C3F" };
          break;
      case 3:
          Colors = new string[] { "#1F3A93", "#4A90E2", "#7B8D8E" }; 
          break;
      case 4:
          Colors = new string[] { "#28A745", "#155724", "#F1C40F" }; 
          break;
      case 5:
          Colors = new string[] { "#8E44AD", "#9B59B6", "#F39C12" };  
          break;
      case 6:
          Colors = new string[] { "#FFC107", "#FF9800", "#FF5722" };  
          break;
      case 7:
          Colors = new string[] { "#2C3E50", "#34495E", "#7F8C8D" };  
          break;
      case 8:
          Colors = new string[] { "#FF6347", "#FFD700", "#ADFF2F" };  
          break;
      default:
          throw new ArgumentException("Número de color no válido.");
      }
  }

  public string GetRandomColor()
  {
      Random random = new Random();
      int index = random.Next(Colors.Length);
      return Colors[index];
  }
}

class Dragon
{
  public void DragonCurve(XmlDocument svgDoc, XmlElement parentGroup, int x0, int y0, int x1, int y1, int iterations, int numcolor)
  {
    ColorRandomizer ran = new(numcolor);
    string color = ran.GetRandomColor();
    if (iterations == 0)
    {
      XmlElement line = svgDoc.CreateElement("line");
      line.SetAttribute("x1", x0.ToString());
      line.SetAttribute("y1", y0.ToString());
      line.SetAttribute("x2", x1.ToString());
      line.SetAttribute("y2", y1.ToString());
      line.SetAttribute("stroke", color);
      line.SetAttribute("stroke-width", "2");

      parentGroup.AppendChild(line);
    }
    else
    {
      int midX = (x0 + x1) / 2 - (y1 - y0) / 2;
      int midY = (y0 + y1) / 2 + (x1 - x0) / 2;

      DragonCurve(svgDoc, parentGroup, x0, y0, midX, midY, iterations - 1, numcolor);
      DragonCurve(svgDoc, parentGroup, midX, midY, x1, y1, iterations - 1, numcolor);
    }
  }
}
class SVGHandler
  {
  public XmlDocument CreateSvg(string size)
  {
    XmlDocument svgDoc = new();
    XmlElement svgRoot = svgDoc.CreateElement("svg");
    svgRoot.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
    svgRoot.SetAttribute("width", size);
    svgRoot.SetAttribute("height", size);
    svgDoc.AppendChild(svgRoot);
    return svgDoc;
  }

  public void Contorno(XmlDocument svgDoc, string size)
  {
    XmlElement quad = svgDoc.CreateElement("rect");
    quad.SetAttribute("width", size);
    quad.SetAttribute("height", size);
    quad.SetAttribute("stroke", "red");
    quad.SetAttribute("fill", "none");
    svgDoc.DocumentElement.AppendChild(quad);
  }
}