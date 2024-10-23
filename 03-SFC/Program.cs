using CommandLine;
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
}

class Program
{
  static void Main(string[] args)
  {
    Parser.Default.ParseArguments<Options>(args)
      .WithParsed<Options>(o =>
      {
        if (o.FileName.EndsWith(".svg"))
        {
          // SVG output for debugging...
          // Create an XML document to represent the SVG
          XmlDocument svgDoc = new();

          // Create the SVG root element
          XmlElement svgRoot = svgDoc.CreateElement("svg");
          svgRoot.SetAttribute("xmlns", "http://www.w3.org/2000/svg");
          svgRoot.SetAttribute("width", o.Width.ToString());
          svgRoot.SetAttribute("height", o.Height.ToString());
          svgDoc.AppendChild(svgRoot);

          // Largest square size
          int size = Math.Max(Math.Min(o.Width, o.Height) - 10, 5);

          // Create a group element to contain the square ornament in the middle of the rectangle area..
          XmlElement group = svgDoc.CreateElement("g");
          group.SetAttribute("transform", $"translate({(o.Width-size)/2},{(o.Height-size)/2})");
          svgRoot.AppendChild(group);

          // Create a square ornament - you have to put your own SFC curve rendering code here
          for (int s = size; s > 5; s -= 10)
          {
            // Rectangle element for simplicity
            XmlElement rect = svgDoc.CreateElement("rect");
            int offset = (size - s) / 2;
            rect.SetAttribute("x", offset.ToString());
            rect.SetAttribute("y", offset.ToString());
            rect.SetAttribute("width", s.ToString());
            rect.SetAttribute("height", s.ToString());
            rect.SetAttribute("stroke", $"#202020");
            rect.SetAttribute("fill", "none");
            group.AppendChild(rect);
          }

          // Save the SVG document to a file
          svgDoc.Save(o.FileName);

          Console.WriteLine($"SVG saved to {o.FileName}");
        }
      });
  }
}
