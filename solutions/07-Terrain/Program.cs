using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public class TerrainGenerator : Form
{
    private float hausdorffCoefficient = 1.0f;
    private List<Vector3> vertices = new List<Vector3>();
    private List<Triangle> triangles = new List<Triangle>();
    private bool shadingMode = false;

    public TerrainGenerator()
    {
        this.Text = "Terrain Generation";
        this.Size = new Size(800, 600);
        this.KeyDown += OnKeyDown;
        this.Paint += OnPaint;
        GenerateTerrain();
    }

    // Llama al algoritmo de generación de terreno Diamond-Square
    private void GenerateTerrain()
    {
        // Inicializa la malla base de triángulos (2x2)
        vertices.Clear();
        triangles.Clear();

        // Vértices iniciales para el triángulo
        vertices.Add(new Vector3(0, 0, 0));
        vertices.Add(new Vector3(1, 0, 0));
        vertices.Add(new Vector3(0, 1, 0));
        vertices.Add(new Vector3(1, 1, 0));

        // Triángulos iniciales
        triangles.Add(new Triangle(0, 1, 2));
        triangles.Add(new Triangle(1, 2, 3));
    }

    // Subdivisión de la malla (Diamond-Square)
    private void DiamondSquareSubdivision()
    {
        var newTriangles = new List<Triangle>();
        var newVertices = new List<Vector3>();

        foreach (var tri in triangles)
        {
            var v0 = vertices[tri.Vertex0];
            var v1 = vertices[tri.Vertex1];
            var v2 = vertices[tri.Vertex2];

            // Calcular puntos medios
            var mid01 = new Vector3((v0.X + v1.X) / 2, (v0.Y + v1.Y) / 2, (v0.Z + v1.Z) / 2);
            var mid12 = new Vector3((v1.X + v2.X) / 2, (v1.Y + v2.Y) / 2, (v1.Z + v2.Z) / 2);
            var mid20 = new Vector3((v2.X + v0.X) / 2, (v2.Y + v0.Y) / 2, (v2.Z + v0.Z) / 2);

            // Aplicar desplazamiento aleatorio a los puntos medios
            mid01.Z += Random.Range(-hausdorffCoefficient, hausdorffCoefficient);
            mid12.Z += Random.Range(-hausdorffCoefficient, hausdorffCoefficient);
            mid20.Z += Random.Range(-hausdorffCoefficient, hausdorffCoefficient);

            // Añadir los nuevos vértices
            newVertices.Add(mid01);
            newVertices.Add(mid12);
            newVertices.Add(mid20);

            // Crear nuevos triángulos
            newTriangles.Add(new Triangle(tri.Vertex0, newVertices.Count - 3, newVertices.Count - 1));
            newTriangles.Add(new Triangle(tri.Vertex1, newVertices.Count - 2, newVertices.Count - 3));
            newTriangles.Add(new Triangle(tri.Vertex2, newVertices.Count - 1, newVertices.Count - 2));
        }

        vertices.AddRange(newVertices);
        triangles.AddRange(newTriangles);
    }

    // Función para reducir la malla (Updivide)
    private void Updivide()
    {
        vertices = vertices.Take(4).ToList();
        triangles.Clear();
        triangles.Add(new Triangle(0, 1, 2));
        triangles.Add(new Triangle(1, 2, 3));
    }

    // Cambiar el coeficiente Hausdorff
    private void ChangeHausdorffCoefficient(float newCoefficient)
    {
        hausdorffCoefficient = newCoefficient;
    }

    // Calcular normales de los triángulos
    private List<Vector3> CalculateNormals()
    {
        var normals = new List<Vector3>();

        foreach (var tri in triangles)
        {
            var v0 = vertices[tri.Vertex0];
            var v1 = vertices[tri.Vertex1];
            var v2 = vertices[tri.Vertex2];

            var vector1 = new Vector3(v1.X - v0.X, v1.Y - v0.Y, v1.Z - v0.Z);
            var vector2 = new Vector3(v2.X - v0.X, v2.Y - v0.Y, v2.Z - v0.Z);

            var normal = vector1.CrossProduct(vector2);
            normal.Normalize();
            normals.Add(normal);
        }

        return normals;
    }

    // Cambiar modo de sombreado (Shading)
    private void ToggleShading()
    {
        shadingMode = !shadingMode;
    }

    // Dibujar terreno en pantalla
    private void OnPaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        var normals = CalculateNormals();

        for (int i = 0; i < triangles.Count; i++)
        {
            var tri = triangles[i];
            var v0 = vertices[tri.Vertex0];
            var v1 = vertices[tri.Vertex1];
            var v2 = vertices[tri.Vertex2];

            // Si está activado el modo sombreado, usa las normales
            Color color;
            if (shadingMode)
            {
                var normal = normals[i];
                color = Color.FromArgb((int)(Math.Abs(normal.X) * 255), (int)(Math.Abs(normal.Y) * 255), (int)(Math.Abs(normal.Z) * 255));
            }
            else
            {
                color = Color.White;
            }

            g.FillPolygon(new SolidBrush(color), new[] { new PointF(v0.X * 300, v0.Y * 300), new PointF(v1.X * 300, v1.Y * 300), new PointF(v2.X * 300, v2.Y * 300) });
        }
    }

    // Controlar teclas para interacción
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.S) // Subdividir
        {
            DiamondSquareSubdivision();
            Invalidate();
        }
        else if (e.KeyCode == Keys.U) // Updividir
        {
            Updivide();
            Invalidate();
        }
        else if (e.KeyCode == Keys.H) // Cambiar coeficiente Hausdorff
        {
            ChangeHausdorffCoefficient(hausdorffCoefficient + 0.1f);
            Invalidate();
        }
        else if (e.KeyCode == Keys.I) // Cambiar modo de sombreado
        {
            ToggleShading();
            Invalidate();
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new TerrainGenerator());
    }
}

public class Vector3
{
    public float X, Y, Z;

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public void Normalize()
    {
        float length = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        X /= length;
        Y /= length;
        Z /= length;
    }

    public Vector3 CrossProduct(Vector3 other)
    {
        return new Vector3(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X
        );
    }
}

public class Triangle
{
    public int Vertex0, Vertex1, Vertex2;

    public Triangle(int vertex0, int vertex1, int vertex2)
    {
        Vertex0 = vertex0;
        Vertex1 = vertex1;
        Vertex2 = vertex2;
    }
}
