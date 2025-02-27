using System;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Input;
using System.Linq;

class TerrainGeneration
{
    private static GL GL;
    private static IWindow window;
    private static uint vao, vbo, ebo;
    private static int program;
    private static float[,] terrain;
    private static int size;
    private static int stepSize;
    private static float hausdorffCoefficient = 1.0f;
    private static bool isPhongShading = false;

    static void Main()
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        window = Window.Create(options);
        window.Load += OnLoad;
        window.Render += OnRender;
        window.Resize += OnResize;
        window.Run();
    }

    private static void OnLoad()
    {
        GL = window.CreateOpenGL();
        GL.Enable(EnableCap.DepthTest);

        program = CreateShaderProgram();
        GL.UseProgram(program);

        size = 33; // Grid size (2^n + 1)
        stepSize = size / 2;
        terrain = new float[size, size];
        GenerateTerrain(terrain, stepSize, hausdorffCoefficient);

        InitializeBuffers();

        // Crear el contexto de entrada
        IInputContext input = window.CreateInput();

        // Suscribirse a los eventos de teclas para cada teclado disponible
        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += OnKeyDown;
            input.Keyboards[i].KeyUp += OnKeyUp;
        }
    }

    private static void OnRender(double delta)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Use your shaders and draw the terrain
        GL.UseProgram(program);
        GL.BindVertexArray(vao);
        GL.DrawElements(PrimitiveType.Triangles, 3 * (size - 1) * (size - 1), DrawElementsType.UnsignedInt, 0);

        window.SwapBuffers();
    }

    private static void OnResize(Vector2D<int> size)
    {
        GL.Viewport(0, 0, size.X, size.Y);
    }

    // Manejar eventos de KeyDown
    private static void OnKeyDown(IKeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.S:
                Subdivide(); // Increase recursion level
                break;
            case Key.U:
                Updivide(); // Decrease recursion level
                break;
            case Key.H:
                hausdorffCoefficient = hausdorffCoefficient == 1.0f ? 0.5f : 1.0f; // Toggle Hausdorff coefficient
                GenerateTerrain(terrain, stepSize, hausdorffCoefficient);
                InitializeBuffers();
                break;
            case Key.I:
                isPhongShading = !isPhongShading; // Toggle Phong shading
                break;
        }
    }

    // Manejar eventos de KeyUp (tecla liberada)
    private static void OnKeyUp(IKeyEventArgs e)
    {
        // Aquí podrías añadir lógica para manejar la liberación de teclas si es necesario
    }

    private static void Subdivide()
    {
        stepSize = Math.Max(1, stepSize / 2);
        GenerateTerrain(terrain, stepSize, hausdorffCoefficient);
        InitializeBuffers();
    }

    private static void Updivide()
    {
        stepSize *= 2;
        GenerateTerrain(terrain, stepSize, hausdorffCoefficient);
        InitializeBuffers();
    }

    private static void GenerateTerrain(float[,] terrain, int stepSize, float range)
    {
        // Inicializar las esquinas del terreno con valores aleatorios
        terrain[0, 0] = Random.Range(-range, range);
        terrain[0, size - 1] = Random.Range(-range, range);
        terrain[size - 1, 0] = Random.Range(-range, range);
        terrain[size - 1, size - 1] = Random.Range(-range, range);

        // Aplicar el algoritmo Diamond-Square recursivamente
        GenerateDiamondSquare(terrain, stepSize, range);
    }

    private static void GenerateDiamondSquare(float[,] terrain, int stepSize, float range)
    {
        if (stepSize < 1) return;

        // Paso Diamond
        for (int y = stepSize; y < size; y += stepSize * 2)
        {
            for (int x = stepSize; x < size; x += stepSize * 2)
            {
                DiamondStep(x, y, stepSize, terrain, range);
            }
        }

        // Paso Square
        for (int y = 0; y < size; y += stepSize)
        {
            for (int x = (y + stepSize) % (stepSize * 2); x < size; x += stepSize * 2)
            {
                SquareStep(x, y, stepSize, terrain, range);
            }
        }

        // Recursivamente reducir el tamaño de paso y el rango
        GenerateDiamondSquare(terrain, stepSize / 2, range * 0.5f);
    }

    private static void DiamondStep(int x, int y, int stepSize, float[,] terrain, float range)
    {
        float avg = (terrain[x - stepSize, y - stepSize] +
                     terrain[x + stepSize, y - stepSize] +
                     terrain[x + stepSize, y + stepSize] +
                     terrain[x - stepSize, y + stepSize]) / 4.0f;
        terrain[x, y] = avg + Random.Range(-range, range);
    }

    private static void SquareStep(int x, int y, int stepSize, float[,] terrain, float range)
    {
        int count = 0;
        float avg = 0;

        if (x - stepSize >= 0) { avg += terrain[x - stepSize, y]; count++; }
        if (x + stepSize < terrain.GetLength(0)) { avg += terrain[x + stepSize, y]; count++; }
        if (y - stepSize >= 0) { avg += terrain[x, y - stepSize]; count++; }
        if (y + stepSize < terrain.GetLength(1)) { avg += terrain[x, y + stepSize]; count++; }

        terrain[x, y] = (avg / count) + Random.Range(-range, range);
    }

    private static void InitializeBuffers()
    {
        // Crear y vincular el array de vértices
        vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        // Crear y llenar el buffer de vértices con los datos del terreno
        vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        // Crear los datos de vértices (posiciones y normales)
        var vertices = new System.Collections.Generic.List<float>();
        var indices = new System.Collections.Generic.List<uint>();

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                vertices.Add(x);
                vertices.Add(y);
                vertices.Add(terrain[x, y]);
            }
        }

        // Crear el buffer de índices para los triángulos
        for (int y = 0; y < size - 1; y++)
        {
            for (int x = 0; x < size - 1; x++)
            {
                uint i1 = (uint)(y * size + x);
                uint i2 = (uint)((y + 1) * size + x);
                uint i3 = (uint)((y + 1) * size + (x + 1));
                uint i4 = (uint)(y * size + (x + 1));

                indices.Add(i1);
                indices.Add(i2);
                indices.Add(i3);
                indices.Add(i1);
                indices.Add(i3);
                indices.Add(i4);
            }
        }

        GL.BufferData(BufferTargetARB.ArrayBuffer, vertices.ToArray(), BufferUsageARB.StaticDraw);

        ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTargetARB.ElementArrayBuffer, indices.ToArray(), BufferUsageARB.StaticDraw);

        // Establecer los punteros de los atributos de los vértices (posición)
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindVertexArray(0);
    }

    private static int CreateShaderProgram()
    {
        var vertexShaderSource = @"
        #version 330 core
        layout(location = 0) in vec3 inPosition;
        layout(location = 1) in vec3 inNormal;
        out vec3 fragNormal;
        out vec3 fragPosition;
        uniform mat4 modelViewProj;
        void main()
        {
            fragNormal = inNormal;
            fragPosition = inPosition;
            gl_Position = modelViewProj * vec4(inPosition, 1.0);
        }";

        var fragmentShaderSource = @"
        #version 330 core
        in vec3 fragNormal;
        in vec3 fragPosition;
        out vec4 fragColor;
        uniform vec3 lightPosition;
        uniform vec3 lightColor;
        uniform vec3 viewPosition;
        void main()
        {
            vec3 norm = normalize(fragNormal);
            vec3 lightDir = normalize(lightPosition - fragPosition);
            vec3 viewDir = normalize(viewPosition - fragPosition);
            float diff = max(dot(norm, lightDir), 0.0);
            vec3 reflectDir = reflect(-lightDir, norm);
            float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
            vec3 ambient = 0.1 * lightColor;
            vec3 diffuse = diff * lightColor;
            vec3 specular = spec * lightColor;
            fragColor = vec4(ambient + diffuse + specular, 1.0);
        }";

        // Compilar los shaders y crear el programa
        int vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
        int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

        int program = GL.CreateProgram();
        GL.AttachShader(program, vertexShader);
        GL.AttachShader(program, fragmentShader);
        GL.LinkProgram(program);

        return program;
    }

    private static int CompileShader(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);

        GL.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
        if (status == 0)
        {
            string log = GL.GetShaderInfoLog(shader);
            throw new Exception($"Shader compile failed: {log}");
        }

        return shader;
    }
}
