using System.Diagnostics;
using System.Globalization;
using System.Text;
using CommandLine;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Util;
namespace _08_Fireworks
{
    using Vector3 = Vector3D<float>;
    using Matrix4 = Matrix4X4<float>;
    public class Options
    {
        [Option('w', "width", Required = false, Default = 800, HelpText = "Window width in pixels.")]
        public int WindowWidth { get; set; } = 800;

        [Option('h', "height", Required = false, Default = 600, HelpText = "Window height in pixels.")]
        public int WindowHeight { get; set; } = 600;

        [Option('p', "FirweorkParticles", Required = false, Default = 10000, HelpText = "Maximum number of FirweorkParticles.")]
        public int FirweorkParticles { get; set; } = 10000;

        [Option('r', "rate", Required = false, Default = 1.0, HelpText = "Rocket generation rate per second.")]
        public double FirweorkParticleRate { get; set; } = 1.0;

    }
    public class Program
    {
        private static Util.Texture? backgroundTexture;
        private static IWindow? window;
        private static GL? Gl;
        private static object renderLock = new();
        private static float width;
        private static float height;
        private static Trackball trackball = new Trackball(sceneCenter, sceneDiameter);
        private static Vector3 sceneCenter = new Vector3(0, 30, 0);
        private static float sceneDiameter = 40.0f;
        private const int MAX_VERTICES = 65536;
        private const int VERTEX_SIZE = 12;
        private static float[] vertexBuffer = new float[MAX_VERTICES * VERTEX_SIZE];
        private static int vertices = 0;
        public static int maxFirweorkParticles = 0;
        public static double FirweorkParticleRate = 1000.0;
        private static BufferObject<float>? Vbo;
        private static VertexArrayObject<float>? Vao;
        private static Util.Texture? texture;
        private static bool useTexture = false;
        private const int TEX_SIZE = 128;
        private static bool usePhong = false;
        private static ShaderProgram? ShaderPrg;
        private static double nowSeconds = FPS.NowInSeconds;
        private static FireworkSystem? sim;
        private static FPS fps = new FPS();
        const int RingFireworks = 36;
        private static string WindowTitle()
        {
            StringBuilder sb = new("08-Fireworks");
            if (sim != null)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, " [{0} of {1}], rate={2:f0}", sim.BufferLoad(vertexBuffer), maxFirweorkParticles, FirweorkParticleRate));
            }
            sb.Append(string.Format(CultureInfo.InvariantCulture, ", fps={0:f1}", fps.Fps));
            if (window != null && window.VSync)
                sb.Append(" [VSync]");
            double pps = fps.Pps;
            if (pps > 0.0)
                sb.Append(pps < 5.0e5
                    ? string.Format(CultureInfo.InvariantCulture, ", pps={0:f1}k", pps * 1.0e-3)
                    : string.Format(CultureInfo.InvariantCulture, ", pps={0:f1}m", pps * 1.0e-6));
            if (trackball != null)
            {
                sb.Append(trackball.UsePerspective ? ", perspective" : ", orthographic");
                sb.Append(string.Format(CultureInfo.InvariantCulture, ", zoom={0:f2}", trackball.Zoom));
            }
            if (useTexture && texture != null && texture.IsValid())
                sb.Append($", txt={texture.name}");
            else
                sb.Append(", no texture");
            if (usePhong)
                sb.Append(", Phong shading");
            return sb.ToString();
        }
        private static void SetWindowTitle()
        {
            if (window != null)
                window.Title = WindowTitle();
        }
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[] { "-w", "800", "-h", "600", "-p", "10000", "-r", "1" };
            }
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    WindowOptions options = WindowOptions.Default;
                    options.Size = new Vector2D<int>(o.WindowWidth, o.WindowHeight);
                    options.Title = WindowTitle();
                    options.PreferredDepthBufferBits = 24;
                    options.VSync = true;
                    window = Window.Create(options);
                    width = o.WindowWidth;
                    height = o.WindowHeight;
                    window.Load += OnLoad;
                    window.Render += OnRender;
                    window.Closing += OnClose;
                    window.Resize += OnResize;
                    maxFirweorkParticles = Math.Min(MAX_VERTICES, o.FirweorkParticles);
                    FirweorkParticleRate = o.FirweorkParticleRate;
                    window.Run();
                });
        }
        private static void VaoPointers()
        {
            Debug.Assert(Vao != null);
            Vao.Bind();
            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, VERTEX_SIZE, 0);
            Vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, VERTEX_SIZE, 3);
            Vao.VertexAttributePointer(2, 3, VertexAttribPointerType.Float, VERTEX_SIZE, 6);
            Vao.VertexAttributePointer(3, 2, VertexAttribPointerType.Float, VERTEX_SIZE, 9);
            Vao.VertexAttributePointer(4, 1, VertexAttribPointerType.Float, VERTEX_SIZE, 11);
        }
        private static void OnLoad()
        {
            //backgroundTexture = new Util.Texture("ruta/a/tu/imagen_de_fondo.jpg");
            //backgroundTexture.OpenglTextureFromFile(Gl);
            Debug.Assert(window != null);
            IInputContext input = window.CreateInput();
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += KeyDown;
                input.Keyboards[i].KeyUp += KeyUp;
            }
            for (int i = 0; i < input.Mice.Count; i++)
            {
                input.Mice[i].MouseDown += MouseDown;
                input.Mice[i].MouseUp += MouseUp;
                input.Mice[i].MouseMove += MouseMove;
                input.Mice[i].DoubleClick += MouseDoubleClick;
                input.Mice[i].Scroll += MouseScroll;
            }
            Gl = GL.GetApi(window);
            lock (renderLock)
            {
                sim = new FireworkSystem(nowSeconds, FirweorkParticleRate, maxFirweorkParticles, maxFirweorkParticles / 10);
                vertices = sim.BufferLoad(vertexBuffer);
                Vbo = new BufferObject<float>(Gl, vertexBuffer, BufferTargetARB.ArrayBuffer);
                Vao = new VertexArrayObject<float>(Gl, Vbo);
                VaoPointers();
                ShaderPrg = new ShaderProgram(Gl, "vertex.glsl", "fragment.glsl");
                trackball = new Trackball(sceneCenter, sceneDiameter);
            }
            SetWindowTitle();
            SetupViewport();
        }
        private static float mouseCx = 0.001f;
        private static float mouseCy = -0.001f;
        private static void SetupViewport()
        {
            Gl?.Viewport(0, 0, (uint)width, (uint)height);
            trackball?.ViewportChange((int)width, (int)height, 0.05f, 1000.0f);
            float minSize = Math.Min(width, height);
            mouseCx = sceneDiameter / minSize;
            mouseCy = -mouseCx;
        }
        private static void OnResize(Vector2D<int> newSize)
        {
            width = newSize[0];
            height = newSize[1];
            SetupViewport();
        }
        private static unsafe void OnRender(double obj)
        {
            Debug.Assert(Gl != null);
            Debug.Assert(ShaderPrg != null);
            Debug.Assert(trackball != null);
            Gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit);
            lock (renderLock)
            {
                nowSeconds = FPS.NowInSeconds;
                if (sim != null)
                {
                    sim.SimulateTo(nowSeconds);
                    vertices = sim.BufferLoad(vertexBuffer);
                }
                Gl.Enable(GLEnum.DepthTest);
                Gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
                Gl.Disable(GLEnum.CullFace);
                Gl.Enable(GLEnum.VertexProgramPointSize);
                VaoPointers();
                ShaderPrg.Use();
                ShaderPrg.TrySetUniform("view", trackball.View);
                ShaderPrg.TrySetUniform("projection", trackball.Projection);
                ShaderPrg.TrySetUniform("model", Matrix4.Identity);
                ShaderPrg.TrySetUniform("lightColor", 1.0f, 1.0f, 1.0f);
                ShaderPrg.TrySetUniform("lightPosition", -8.0f, 8.0f, 8.0f);
                ShaderPrg.TrySetUniform("eyePosition", trackball.Eye);
                ShaderPrg.TrySetUniform("Ka", 0.1f);
                ShaderPrg.TrySetUniform("Kd", 0.7f);
                ShaderPrg.TrySetUniform("Ks", 0.3f);
                ShaderPrg.TrySetUniform("shininess", 60.0f);
                ShaderPrg.TrySetUniform("usePhong", usePhong);
                ShaderPrg.TrySetUniform("useTexture", useTexture);
                ShaderPrg.TrySetUniform("tex", 0);
                if (useTexture)
                    texture?.Bind(Gl);
                vertices = (sim != null) ? sim.BufferLoad(vertexBuffer) : 0;
                if (Vbo != null && vertices > 0)
                {
                    Vbo.UpdateData(vertexBuffer, 0, vertices * VERTEX_SIZE);
                    Gl.DrawArrays((GLEnum)PrimitiveType.Points, 0, (uint)vertices);
                    fps.AddPrimitives(vertices);
                }
            }
            Gl.UseProgram(0);
            if (useTexture)
                Gl.BindTexture(TextureTarget.Texture2D, 0);
            if (fps.AddFrames())
                SetWindowTitle();
        }
        private static void OnClose()
        {
            Vao?.Dispose();
            ShaderPrg?.Dispose();
            texture?.Dispose();
        }
        private static int shiftDown = 0;
        private static int ctrlDown = 0;
        private static void KeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            if (trackball != null && trackball.KeyDown(arg1, arg2, arg3))
            {
                SetWindowTitle();
            }
            switch (arg2)
            {
                case Key.ShiftLeft:
                case Key.ShiftRight:
                    shiftDown++;
                    break;
                case Key.ControlLeft:
                case Key.ControlRight:
                    ctrlDown++;
                    break;
                case Key.P:
                    if (trackball != null)
                    {
                        trackball.UsePerspective = !trackball.UsePerspective;
                        SetWindowTitle();
                    }
                    break;
                case Key.C:
                    if (trackball != null)
                    {
                        trackball.Reset();
                        Ut.Message("Camera reset");
                    }
                    break;
                case Key.V:
                    if (window != null)
                    {
                        window.VSync = !window.VSync;
                        if (window.VSync)
                        {
                            Ut.Message("VSync on");
                            fps.Reset();
                        }
                        else
                            Ut.Message("VSync off");
                    }
                    break;
                case Key.R:
                    if (sim != null)
                    {
                        sim.Reset();
                        Ut.Message("Simulator reset");
                    }
                    break;
                case Key.Up:
                    if (sim != null)
                    {
                        sim.FirweorkParticleRate *= 2;
                        SetWindowTitle();
                    }
                    break;
                case Key.Down:
                    if (sim != null)
                    {
                        sim.FirweorkParticleRate /= 2;
                        SetWindowTitle();
                    }
                    break;
                case Key.F1:
                    Ut.Message("P           toggle perspective", true);
                    Ut.Message("V           toggle VSync", true);
                    Ut.Message("C           camera reset", true);
                    Ut.Message("R           reset the FireworkSystem", true);
                    Ut.Message("Up, Down    change FirweorkParticle generation rate", true);
                    Ut.Message("F1          print help", true);
                    Ut.Message("Esc         quit the program", true);
                    Ut.Message("Mouse.left  Trackball rotation", true);
                    Ut.Message("Mouse.wheel zoom in/out", true);
                    break;
                case Key.Escape:
                    window?.Close();
                    break;
                case Key.Space: // Agregar este caso para la barra de espacio
                if (sim != null)
                {
                    // Lanzar un cohete
                    sim.LaunchRocket();
                    Ut.Message("Rocket launched!");
                }
                break;
            }
        }
        private static void KeyUp(IKeyboard arg1, Key arg2, int arg3)
        {
            if (trackball != null && trackball.KeyUp(arg1, arg2, arg3))
                return;
            switch (arg2)
            {
                case Key.ShiftLeft:
                case Key.ShiftRight:
                    shiftDown--;
                    break;
                case Key.ControlLeft:
                case Key.ControlRight:
                    ctrlDown--;
                    break;
            }
        }
        private static float currentX = 0.0f;
        private static float currentY = 0.0f;
        private static bool dragging = false;
        private static void MouseDown(IMouse mouse, MouseButton btn)
        {
            if (trackball != null)
                trackball.MouseDown(mouse, btn);
            if (btn == MouseButton.Right)
            {
                Ut.MessageInvariant($"Right button down: {mouse.Position}");
                dragging = true;
                currentX = mouse.Position.X;
                currentY = mouse.Position.Y;
            }
        }

        private static void MouseUp(IMouse mouse, MouseButton btn)
        {
            if (trackball != null)
                trackball.MouseUp(mouse, btn);
            if (btn == MouseButton.Right)
            {
                Ut.MessageInvariant($"Right button up: {mouse.Position}");
                dragging = false;
            }
        }
        private static void MouseMove(IMouse mouse, System.Numerics.Vector2 xy)
        {
            if (trackball != null)
                trackball.MouseMove(mouse, xy);
            if (mouse.IsButtonPressed(MouseButton.Right))
            {
                Ut.MessageInvariant($"Mouse drag: {xy}");
            }
            if (dragging)
            {
                float newX = mouse.Position.X;
                float newY = mouse.Position.Y;
                if (newX != currentX || newY != currentY)
                {
                    currentX = newX;
                    currentY = newY;
                }
            }
        }
        private static void MouseDoubleClick(IMouse mouse, MouseButton btn, System.Numerics.Vector2 xy)
        {
            if (btn == MouseButton.Right)
            {
                Ut.Message("Closed by double-click.", true);
                window?.Close();
            }
        }
        private static void MouseScroll(IMouse mouse, ScrollWheel wheel)
        {
            if (trackball != null)
            {
                trackball.MouseWheel(mouse, wheel);
                SetWindowTitle();
            }
            Ut.MessageInvariant($"Mouse scroll: {wheel.Y}");
        }
    }
}