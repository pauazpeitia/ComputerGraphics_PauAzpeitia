using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
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

        [Option('p', "particles", Required = false, Default = 10000, HelpText = "Maximum number of particles.")]
        public int Particles { get; set; } = 10000;

        [Option('r', "rate", Required = false, Default = 1.0, HelpText = "Rocket generation rate per second.")]
        public double ParticleRate { get; set; } = 1.0;

        [Option('t', "texture", Required = false, Default = ":check:", HelpText = "User-defined texture.")]
        public string TextureFile { get; set; } = ":check:";
    }

    // Particle class with advanced physics, wind effect, and multiple explosion types.
    class Particle
    {
        private static Random rnd = new((int)DateTime.Now.Ticks);

        // Particle types:
        // 0 = rocket; 1 = primary spark; 2 = secondary spark; 3 = ring explosion spark.
        public int Type;
        public bool SecondaryExploded { get; set; } = false; // Only for primary sparks.

        public Vector3 Position { get; private set; }
        public Vector3 Velocity { get; private set; }
        public Vector3 Color { get; private set; }
        public float Size { get; private set; }
        public double Age { get; private set; }
        private double SimulatedTime;

        // Simulation constants.
        const float gravity = 9.81f;
        const float dragCoefficient = 0.1f;
        const double rocketLifetime = 5.0;          // Lifetime for rockets.
        const double sparkLifetime = 3.0;           // Lifetime for primary sparks.
        const double secondarySparkLifetime = 1.5;  // Lifetime for secondary sparks.
        const double ringLifetime = 2.0;            // Lifetime for ring explosion sparks.

        // Constructor for rockets (type 0) with inclined launch.
        public Particle(double now)
        {
            Type = 0;
            SimulatedTime = now;
            // Launch from Y = 10 with random X and Z offsets.
            float offsetX = (float)(rnd.NextDouble() * 20.0 - 10.0);
            float offsetZ = (float)(rnd.NextDouble() * 20.0 - 10.0);
            Position = new Vector3(offsetX, 10.0f, offsetZ);
            
            // Total speed between 20 and 30 m/s.
            float speed = (float)(20.0 + rnd.NextDouble() * 10.0);
            // Deviation angle: random between -20° and +20° (in radians).
            double deviation = (rnd.NextDouble() * 40.0 - 20.0) * (Math.PI / 180.0);
            // Random horizontal azimuth.
            double azimuth = rnd.NextDouble() * 2 * Math.PI;
            // Calculate velocity components.
            float vx = speed * (float)(Math.Sin(deviation) * Math.Cos(azimuth));
            float vy = speed * (float)Math.Cos(deviation);
            float vz = speed * (float)(Math.Sin(deviation) * Math.Sin(azimuth));
            Velocity = new Vector3(vx, vy, vz);

            Color = new Vector3(1.0f, 1.0f, 1.0f);
            Size = 4.0f;
            Age = rocketLifetime;
        }

        // Factory method for primary sparks (type 1).
        public static Particle CreateSpark(double now, Vector3 explosionPos, Vector3 baseColor)
        {
            Particle p = new Particle(now);
            p.Type = 1;
            p.Position = explosionPos;
            double theta = rnd.NextDouble() * 2 * Math.PI;
            double phi = Math.Acos(2 * rnd.NextDouble() - 1);
            // Bias phi towards 20° (0.349066 rad).
            double targetPhi = 0.349066;
            phi = phi * 0.7 + targetPhi * 0.3;
            float speed = (float)(5.0 + rnd.NextDouble() * 5.0);
            float vx = speed * (float)(Math.Sin(phi) * Math.Cos(theta));
            float vy = speed * (float)(Math.Sin(phi) * Math.Sin(theta));
            float vz = speed * (float)Math.Cos(phi);
            p.Velocity = new Vector3(vx, vy, vz);
            float factor = 0.9f + (float)(rnd.NextDouble() * 0.2f);
            p.Color = new Vector3(baseColor.X * factor, baseColor.Y * factor, baseColor.Z * factor);
            p.Size = 2.0f;
            p.Age = sparkLifetime;
            p.SimulatedTime = now;
            return p;
        }

        // Factory method for secondary sparks (type 2).
        public static Particle CreateSecondarySpark(double now, Vector3 explosionPos, Vector3 baseColor)
        {
            Particle p = new Particle(now);
            p.Type = 2;
            p.Position = explosionPos;
            double theta = rnd.NextDouble() * 2 * Math.PI;
            double phi = Math.Acos(2 * rnd.NextDouble() - 1);
            // Bias phi towards 20°.
            double targetPhi = 0.349066;
            phi = phi * 0.7 + targetPhi * 0.3;
            float speed = (float)(3.0 + rnd.NextDouble() * 3.0);
            float vx = speed * (float)(Math.Sin(phi) * Math.Cos(theta));
            float vy = speed * (float)(Math.Sin(phi) * Math.Sin(theta));
            float vz = speed * (float)Math.Cos(phi);
            p.Velocity = new Vector3(vx, vy, vz);
            float factor = 0.9f + (float)(rnd.NextDouble() * 0.2f);
            p.Color = new Vector3(baseColor.X * factor, baseColor.Y * factor, baseColor.Z * factor);
            p.Size = 1.5f;
            p.Age = secondarySparkLifetime;
            p.SimulatedTime = now;
            return p;
        }

        // Factory method for ring explosion sparks (type 3).
        public static Particle CreateRingParticle(double now, Vector3 explosionPos, Vector3 baseColor, int ringIndex, int totalParticles)
        {
            Particle p = new Particle(now);
            p.Type = 3;
            p.Position = explosionPos;
            // Compute angle for ring: evenly distributed around 360°.
            double angle = 2 * Math.PI * ringIndex / totalParticles;
            // Set speed mainly horizontal with a small upward component.
            float speed = 7.0f;
            float vx = speed * (float)Math.Cos(angle);
            float vz = speed * (float)Math.Sin(angle);
            float vy = speed * 0.2f; // Small upward lift.
            p.Velocity = new Vector3(vx, vy, vz);
            float factor = 0.9f + (float)(rnd.NextDouble() * 0.2f);
            p.Color = new Vector3(baseColor.X * factor, baseColor.Y * factor, baseColor.Z * factor);
            p.Size = 2.5f;
            p.Age = ringLifetime;
            p.SimulatedTime = now;
            return p;
        }

        // Simulate the particle using Euler's method, including wind.
        public bool SimulateTo(double time, Vector3 wind)
        {
            if (time <= SimulatedTime)
                return true;
            double dt = time - SimulatedTime;
            SimulatedTime = time;
            Age -= dt;
            if (Age <= 0.0)
                return false;
            // Gravity force.
            Vector3 gravityVec = new Vector3(0.0f, -gravity, 0.0f);
            // Drag force.
            Vector3 drag = Velocity * (-dragCoefficient);
            // Total acceleration includes gravity, drag, and wind.
            Vector3 acceleration = gravityVec + drag + wind;
            Velocity += acceleration * (float)dt;
            Position += Velocity * (float)dt;
            // Corrected: Added missing parenthesis.
            float lifeFactor = (float)(Age / (Type == 0 ? rocketLifetime : (Type == 1 ? sparkLifetime : (Type == 2 ? secondarySparkLifetime : ringLifetime))));
            if (Type != 0)
                Color *= lifeFactor;
            Size *= lifeFactor;
            return true;
        }

        public void FillBuffer(float[] buffer, ref int i)
        {
            buffer[i++] = Position.X;
            buffer[i++] = Position.Y;
            buffer[i++] = Position.Z;
            buffer[i++] = Color.X;
            buffer[i++] = Color.Y;
            buffer[i++] = Color.Z;
            buffer[i++] = 0.0f;
            buffer[i++] = 1.0f;
            buffer[i++] = 0.0f;
            buffer[i++] = 0.5f;
            buffer[i++] = 0.5f;
            buffer[i++] = Size;
        }
    }

    // Simulation class with wind dynamics and multiple explosion types.
    public class Simulation
    {
        private List<Particle> particles = new();
        public int MaxParticles { get; private set; }
        private double SimulatedTime;
        public double ParticleRate { get; set; }
        private double nextRocketLaunchTime;
        // Rockets (type 0) explode when vertical velocity <= 0 or upon reaching max height.
        const float maxExplosionHeight = 50.0f;
        const int SparksPerExplosion = 50;
        // Secondary sparks from primary sparks (type 1).
        const int SparksPerSecondaryExplosion = 20;
        // For ring explosions (type 3).
        const int RingParticles = 36;
        private static Random rnd = new Random();

        // Variables to control bursts.
        private int rocketsInBurst = 0;
        private const int RocketsPerBurst = 3;
        private const double BurstInterval = 0.2;
        private const double GapBetweenBursts = 2.0;

        // Wind vector updated over time.
        public Vector3 Wind { get; private set; }

        public Simulation(double now, double particleRate, int maxParticles, int initParticles)
        {
            SimulatedTime = now;
            ParticleRate = particleRate;
            MaxParticles = maxParticles;
            nextRocketLaunchTime = now;
            Wind = new Vector3(0.0f, 0.0f, 0.0f);
        }

        public void SimulateTo(double time)
        {
            if (time <= SimulatedTime)
                return;
            double dt = time - SimulatedTime;
            SimulatedTime = time;

            // Update wind based on time (sinusoidal variation in X and Z).
            float windX = 1.0f + 0.5f * (float)Math.Sin(0.5 * time);
            float windZ = 0.5f + 0.2f * (float)Math.Cos(0.3 * time);
            Wind = new Vector3(windX, 0.0f, windZ);

            List<int> toRemove = new();
            List<Vector3> explosions = new();        // Rocket explosion positions (type 0).
            List<Vector3> secondaryExplosions = new(); // Secondary explosion positions (from primary sparks, type 1).

            // Simulate each particle with wind influence.
            for (int i = 0; i < particles.Count; i++)
            {
                Particle p = particles[i];
                bool alive = p.SimulateTo(time, Wind);
                if (!alive)
                {
                    toRemove.Add(i);
                }
                else if (p.Type == 0 && (p.Velocity.Y <= 0 || p.Position.Y >= maxExplosionHeight))
                {
                    explosions.Add(new Vector3(p.Position.X, p.Position.Y, p.Position.Z));
                    toRemove.Add(i);
                }
                // For primary sparks: if their speed falls below a threshold, trigger secondary explosion.
                else if (p.Type == 1 && p.Velocity.Length < 2.0f && !p.SecondaryExploded)
                {
                    secondaryExplosions.Add(p.Position);
                    p.SecondaryExploded = true;
                    toRemove.Add(i);
                }
            }

            toRemove.Sort();
            toRemove.Reverse();
            foreach (int idx in toRemove)
                particles.RemoveAt(idx);

            // Process rocket explosions.
            foreach (var pos in explosions)
            {
                Vector3 baseColor = new Vector3(
                    (float)(0.5 + rnd.NextDouble() * 0.5),
                    (float)(0.5 + rnd.NextDouble() * 0.5),
                    (float)(0.5 + rnd.NextDouble() * 0.5)
                );
                if (rnd.NextDouble() < 0.5)
                {
                    // Ring explosion: generate ring particles.
                    for (int j = 0; j < RingParticles; j++)
                    {
                        if (particles.Count < MaxParticles)
                            particles.Add(Particle.CreateRingParticle(time, pos, baseColor, j, RingParticles));
                    }
                }
                else
                {
                    // Standard explosion: generate primary sparks.
                    for (int s = 0; s < SparksPerExplosion; s++)
                    {
                        if (particles.Count < MaxParticles)
                            particles.Add(Particle.CreateSpark(time, pos, baseColor));
                    }
                }
            }

            // Process secondary explosions.
            foreach (var pos in secondaryExplosions)
            {
                Vector3 baseColor = new Vector3(
                    (float)(0.5 + rnd.NextDouble() * 0.5),
                    (float)(0.5 + rnd.NextDouble() * 0.5),
                    (float)(0.5 + rnd.NextDouble() * 0.5)
                );
                for (int s = 0; s < SparksPerSecondaryExplosion; s++)
                {
                    if (particles.Count < MaxParticles)
                        particles.Add(Particle.CreateSecondarySpark(time, pos, baseColor));
                }
            }

            // Launch rockets in bursts.
            if (nextRocketLaunchTime <= time)
            {
                if (particles.Count < MaxParticles)
                {
                    particles.Add(new Particle(nextRocketLaunchTime));
                    rocketsInBurst++;
                    if (rocketsInBurst < RocketsPerBurst)
                    {
                        nextRocketLaunchTime = time + BurstInterval;
                    }
                    else
                    {
                        rocketsInBurst = 0;
                        nextRocketLaunchTime = time + GapBetweenBursts;
                    }
                }
            }
        }

        public int FillBuffer(float[] buffer)
        {
            int i = 0;
            foreach (var p in particles)
                p.FillBuffer(buffer, ref i);
            return particles.Count;
        }

        public void Reset()
        {
            particles.Clear();
            nextRocketLaunchTime = SimulatedTime;
        }
    }

    // Minimal FPS implementation.
    public class FPS
    {
        private double lastTime;
        private int frames;
        public double Fps { get; private set; }
        public double Pps { get; private set; }

        public FPS()
        {
            lastTime = NowInSeconds;
            frames = 0;
            Fps = 0;
            Pps = 0;
        }

        public static double NowInSeconds => DateTime.Now.Ticks / (double)TimeSpan.TicksPerSecond;

        public bool AddFrames()
        {
            frames++;
            double currentTime = NowInSeconds;
            if (currentTime - lastTime >= 1.0)
            {
                Fps = frames / (currentTime - lastTime);
                frames = 0;
                lastTime = currentTime;
                return true;
            }
            return false;
        }

        public void AddPrimitives(int primitives)
        {
            Pps = primitives;
        }

        public void Reset()
        {
            frames = 0;
            lastTime = NowInSeconds;
        }
    }

    // Minimal Trackball implementation for interactive 3D viewing.
    public class Trackball
    {
        public Matrix4 View { get; private set; }
        public Matrix4 Projection { get; private set; }
        public Vector3 Eye { get; private set; }
        public bool UsePerspective { get; set; }
        public float Zoom { get; set; }

        private Vector3 center;
        private float diameter;

        public Trackball(Vector3 center, float diameter)
        {
            this.center = center;
            this.diameter = diameter;
            Zoom = 1.0f;
            UsePerspective = true;
            Eye = new Vector3(0, center.Y, diameter);
            UpdateMatrices();
        }

        private void UpdateMatrices()
        {
            View = CreateLookAt(Eye, center, new Vector3(0, 1, 0));
            Projection = CreatePerspectiveFieldOfView((float)Math.PI / 4, 800f / 600f, 0.1f, 1000f);
        }

        public static Vector3 Normalize(Vector3 v)
        {
            float length = MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
            if (length == 0) return v;
            return new Vector3(v.X / length, v.Y / length, v.Z / length);
        }

        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        public static float Dot(Vector3 a, Vector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Matrix4 CreateLookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 f = Normalize(target - eye);
            Vector3 s = Normalize(Cross(f, up));
            Vector3 u = Cross(s, f);
            Matrix4 result = new Matrix4();
            result.M11 = s.X;
            result.M12 = u.X;
            result.M13 = -f.X;
            result.M14 = 0;
            result.M21 = s.Y;
            result.M22 = u.Y;
            result.M23 = -f.Y;
            result.M24 = 0;
            result.M31 = s.Z;
            result.M32 = u.Z;
            result.M33 = -f.Z;
            result.M34 = 0;
            result.M41 = -Dot(s, eye);
            result.M42 = -Dot(u, eye);
            result.M43 = Dot(f, eye);
            result.M44 = 1;
            return result;
        }

        public static Matrix4 CreatePerspectiveFieldOfView(float fov, float aspect, float near, float far)
        {
            float yScale = 1.0f / MathF.Tan(fov / 2);
            float xScale = yScale / aspect;
            float frustumLength = far - near;
            Matrix4 result = new Matrix4();
            result.M11 = xScale;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = yScale;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = -(far + near) / frustumLength;
            result.M34 = -1;
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = -(2 * near * far) / frustumLength;
            result.M44 = 0;
            return result;
        }

        public void Reset()
        {
            Eye = new Vector3(0, center.Y, diameter);
            Zoom = 1.0f;
            UpdateMatrices();
        }

        public bool KeyDown(IKeyboard keyboard, Key key, int code) => false;
        public bool KeyUp(IKeyboard keyboard, Key key, int code) => false;
        public void MouseDown(IMouse mouse, MouseButton btn) { }
        public void MouseUp(IMouse mouse, MouseButton btn) { }
        public void MouseMove(IMouse mouse, System.Numerics.Vector2 pos) { }
        public void MouseWheel(IMouse mouse, ScrollWheel scroll)
        {
            Zoom *= (1.0f + scroll.Y * 0.1f);
            Eye = new Vector3(Eye.X, Eye.Y, diameter * Zoom);
            UpdateMatrices();
        }
        public void ViewportChange(int width, int height, float near, float far)
        {
            Projection = CreatePerspectiveFieldOfView((float)Math.PI / 4, (float)width / height, near, far);
        }
    }

    internal class Program
    {
        private static IWindow? window;
        private static GL? Gl;
        private static object renderLock = new();
        private static float width;
        private static float height;
        private static Vector3 sceneCenter = new Vector3(0, 30, 0);
        private static float sceneDiameter = 40.0f;
        private const int MAX_VERTICES = 65536;
        private const int VERTEX_SIZE = 12;
        private static float[] vertexBuffer = new float[MAX_VERTICES * VERTEX_SIZE];
        private static int vertices = 0;
        public static int maxParticles = 0;
        public static double particleRate = 1.0;
        private static BufferObject<float>? Vbo;
        private static VertexArrayObject<float>? Vao;
        private static Util.Texture? texture;
        private static bool useTexture = false;
        private static string textureFile = ":check:";
        private const int TEX_SIZE = 128;
        private static bool usePhong = false;
        private static ShaderProgram? ShaderPrg;
        private static double nowSeconds = FPS.NowInSeconds;
        private static Simulation? sim;
        private static FPS fps = new FPS();
        private static Trackball tb = new Trackball(sceneCenter, sceneDiameter);

        private static string WindowTitle()
        {
            StringBuilder sb = new("08-Fireworks");
            if (sim != null)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, " [{0} of {1}], rate={2:f0}", sim.FillBuffer(vertexBuffer), maxParticles, particleRate));
            }
            sb.Append(string.Format(CultureInfo.InvariantCulture, ", fps={0:f1}", fps.Fps));
            if (window != null && window.VSync)
                sb.Append(" [VSync]");
            double pps = fps.Pps;
            if (pps > 0.0)
                sb.Append(pps < 5.0e5
                    ? string.Format(CultureInfo.InvariantCulture, ", pps={0:f1}k", pps * 1.0e-3)
                    : string.Format(CultureInfo.InvariantCulture, ", pps={0:f1}m", pps * 1.0e-6));
            if (tb != null)
            {
                sb.Append(tb.UsePerspective ? ", perspective" : ", orthographic");
                sb.Append(string.Format(CultureInfo.InvariantCulture, ", zoom={0:f2}", tb.Zoom));
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
                    textureFile = o.TextureFile;
                    maxParticles = Math.Min(MAX_VERTICES, o.Particles);
                    particleRate = o.ParticleRate;
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
                sim = new Simulation(nowSeconds, particleRate, maxParticles, maxParticles / 10);
                vertices = sim.FillBuffer(vertexBuffer);
                Vbo = new BufferObject<float>(Gl, vertexBuffer, BufferTargetARB.ArrayBuffer);
                Vao = new VertexArrayObject<float>(Gl, Vbo);
                VaoPointers();
                ShaderPrg = new ShaderProgram(Gl, "vertex.glsl", "fragment.glsl");
                if (textureFile.StartsWith(":"))
                {
                    texture = new(TEX_SIZE, TEX_SIZE, textureFile);
                    texture.GenerateTexture(Gl);
                }
                else
                {
                    texture = new(textureFile, textureFile);
                    texture.OpenglTextureFromFile(Gl);
                }
                tb = new Trackball(sceneCenter, sceneDiameter);
            }
            SetWindowTitle();
            SetupViewport();
        }

        private static float mouseCx = 0.001f;
        private static float mouseCy = -0.001f;
        private static void SetupViewport()
        {
            Gl?.Viewport(0, 0, (uint)width, (uint)height);
            tb?.ViewportChange((int)width, (int)height, 0.05f, 1000.0f);
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
            Debug.Assert(tb != null);
            Gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit);
            lock (renderLock)
            {
                nowSeconds = FPS.NowInSeconds;
                if (sim != null)
                {
                    sim.SimulateTo(nowSeconds);
                    vertices = sim.FillBuffer(vertexBuffer);
                }
                Gl.Enable(GLEnum.DepthTest);
                Gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
                Gl.Disable(GLEnum.CullFace);
                Gl.Enable(GLEnum.VertexProgramPointSize);
                VaoPointers();
                ShaderPrg.Use();
                ShaderPrg.TrySetUniform("view", tb.View);
                ShaderPrg.TrySetUniform("projection", tb.Projection);
                ShaderPrg.TrySetUniform("model", Matrix4.Identity);
                ShaderPrg.TrySetUniform("lightColor", 1.0f, 1.0f, 1.0f);
                ShaderPrg.TrySetUniform("lightPosition", -8.0f, 8.0f, 8.0f);
                ShaderPrg.TrySetUniform("eyePosition", tb.Eye);
                ShaderPrg.TrySetUniform("Ka", 0.1f);
                ShaderPrg.TrySetUniform("Kd", 0.7f);
                ShaderPrg.TrySetUniform("Ks", 0.3f);
                ShaderPrg.TrySetUniform("shininess", 60.0f);
                ShaderPrg.TrySetUniform("usePhong", usePhong);
                ShaderPrg.TrySetUniform("useTexture", useTexture);
                ShaderPrg.TrySetUniform("tex", 0);
                if (useTexture)
                    texture?.Bind(Gl);
                vertices = (sim != null) ? sim.FillBuffer(vertexBuffer) : 0;
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
            if (tb != null && tb.KeyDown(arg1, arg2, arg3))
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
                case Key.T:
                    useTexture = !useTexture;
                    if (useTexture)
                        Ut.Message($"Texture: {texture?.name}");
                    else
                        Ut.Message("Texturing off");
                    SetWindowTitle();
                    break;
                case Key.I:
                    usePhong = !usePhong;
                    Ut.Message("Phong shading: " + (usePhong ? "on" : "off"));
                    SetWindowTitle();
                    break;
                case Key.P:
                    if (tb != null)
                    {
                        tb.UsePerspective = !tb.UsePerspective;
                        SetWindowTitle();
                    }
                    break;
                case Key.C:
                    if (tb != null)
                    {
                        tb.Reset();
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
                        sim.ParticleRate *= 1.1;
                        SetWindowTitle();
                    }
                    break;
                case Key.Down:
                    if (sim != null)
                    {
                        sim.ParticleRate /= 1.1;
                        SetWindowTitle();
                    }
                    break;
                case Key.F1:
                    Ut.Message("T           toggle texture", true);
                    Ut.Message("I           toggle Phong shading", true);
                    Ut.Message("P           toggle perspective", true);
                    Ut.Message("V           toggle VSync", true);
                    Ut.Message("C           camera reset", true);
                    Ut.Message("R           reset the simulation", true);
                    Ut.Message("Up, Down    change particle generation rate", true);
                    Ut.Message("F1          print help", true);
                    Ut.Message("Esc         quit the program", true);
                    Ut.Message("Mouse.left  Trackball rotation", true);
                    Ut.Message("Mouse.wheel zoom in/out", true);
                    break;
                case Key.Escape:
                    window?.Close();
                    break;
            }
        }

        private static void KeyUp(IKeyboard arg1, Key arg2, int arg3)
        {
            if (tb != null && tb.KeyUp(arg1, arg2, arg3))
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
            if (tb != null)
                tb.MouseDown(mouse, btn);
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
            if (tb != null)
                tb.MouseUp(mouse, btn);
            if (btn == MouseButton.Right)
            {
                Ut.MessageInvariant($"Right button up: {mouse.Position}");
                dragging = false;
            }
        }

        private static void MouseMove(IMouse mouse, System.Numerics.Vector2 xy)
        {
            if (tb != null)
                tb.MouseMove(mouse, xy);
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
            if (tb != null)
            {
                tb.MouseWheel(mouse, wheel);
                SetWindowTitle();
            }
            Ut.MessageInvariant($"Mouse scroll: {wheel.Y}");
        }
    }
}