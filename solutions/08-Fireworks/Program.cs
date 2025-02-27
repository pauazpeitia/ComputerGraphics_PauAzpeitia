using System;
using System.Net;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;


namespace _08_Fireworks
{
  public class Program
  {
      private static IWindow? window;      
      private static GL? gl;               
      private static uint vao, vbo;        
      private static List<Particle> particles = new();
      private static List<Proyectil> proyectils = new();   
      private static List<Launcher> launchers = new();  

      public static int numberLaunchers = 1; //Define Number of Launchers
      public static int numberParticlesExplosion = 3;

      static void Main()
      {
          var options = WindowOptions.Default;  
          options.Size = new Vector2D<int>(1920, 1080);
          options.Title = "Fireworks simulation";

          window = Window.Create(options);      
          window.Load += OnLoad;                
          window.Render += OnRender;            
          window.Run();                        
      }
      private static void OnLoad()
      {
        float pmin = -0.9f, pmax = 0.9f, vmin = 4f, vmax = 7f;
        List<float> randomNumbersP = GenerateNumber(numberLaunchers, pmax, pmin); //Position of the Launcher
        List<float> randomNumbersV = GenerateNumber(numberLaunchers, vmax, vmin); //Velocity of the Launcher
        for (int i = 0; i < numberLaunchers; i++) 
        { //Create the Launchers
          launchers.Add(new Launcher(new Vector3D<float>(randomNumbersP[i], -1, 0), new Vector3D<float>(0, randomNumbersV[i], 0)));
        }

        gl = GL.GetApi(window);  
        gl.ClearColor(0f, 0.0f, 0.0f, 1.0f); 
        vao = gl.GenVertexArray();
        vbo = gl.GenBuffer();
        gl.BindVertexArray(vao);  
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo); 
        gl.BufferData<float>(  
          BufferTargetARB.ArrayBuffer,
          (uint)((numberLaunchers + numberLaunchers * numberParticlesExplosion) * 3 * sizeof(float)), 
          null,
          BufferUsageARB.DynamicDraw
        );
        
      }

      private static void OnRender(double deltaTime)
      {
        gl.Clear(ClearBufferMask.ColorBufferBit);  // Cler from previous render

        if (particles.Count < 1)
        {
          Console.WriteLine("NO HAY NINGUNA");
          for (int i = 0; i < numberLaunchers; i++) 
          {
            launchers[i].Emit(particles);   //Launch particles
          }
        }   
        Console.WriteLine($"Particles before update: {particles.Count}");
        UpdateParticles((float)deltaTime);// Update particles until reach gravity = 0 position and then explode
        RenderParticles(); 
        Console.WriteLine($"Particles after update: {particles.Count}");
      }


      private static void UpdateParticles(float deltaTime)
      {
        Vector3D<float> gravity = new Vector3D<float>(0, -9.81f, 0);  
        float dragCoefficient = 0.2f;  
        Random rand = new Random();  
        
        for (int i = particles.Count - 1; i >= 0; i--) 
        {
            Particle p = particles[i];
            if (p.Age > p.LifeSpan)
            {
                particles.RemoveAt(i);
                continue;
            }else if (p.Velocity.Y < 0)
            {
                particles.RemoveAt(i);  
                Console.WriteLine("DEBERIA ELIMINARSE");
                for (int j = 0; j < numberParticlesExplosion; j++) //Proyectils (explosion)
                {
                    float randomX = 0.5f + (float)rand.NextDouble();
                    float randomY = 0.5f + (float)rand.NextDouble();
                    float randomZ = 0.5f + (float)rand.NextDouble();

                    Proyectil pr = new Proyectil(
                        p.Position,
                        new Vector3D<float>(randomX, randomY, randomZ),
                        0,  
                        1.5f  //LifeSpan
                    );
                    proyectils.Add(pr);
                }
                continue; 
            }
            
            //Update particle
            p.Velocity += gravity * deltaTime;
            Vector3D<float> dragForce = -dragCoefficient * p.Velocity;
            p.Velocity += dragForce * deltaTime;
            p.Position += p.Velocity * deltaTime;
            p.Age += deltaTime;
            Console.WriteLine("Update Step");
            particles[i] = p;  
        }
        for (int i = proyectils.Count - 1; i >= 0; i--)
        {
            Proyectil pr = proyectils[i];
            pr.Age += deltaTime;
            if (pr.Age > pr.LifeSpan)
            {
                proyectils.RemoveAt(i);
            }
            else
            {
                proyectils[i] = pr;  
            }
        }
      }
      private static void RenderParticles()
      {
          int particleSize = particles.Count * 3;
          int projectileSize = proyectils.Count * 3;
          float[] allPositions = new float[particleSize + projectileSize];

          for (int i = 0; i < particles.Count; i++)
          {
              allPositions[i * 3 + 0] = particles[i].Position.X;
              allPositions[i * 3 + 1] = particles[i].Position.Y;
              allPositions[i * 3 + 2] = particles[i].Position.Z;
          }
          for (int i = 0; i < proyectils.Count; i++)
          {
            int index = particleSize + (i * 3);
            allPositions[index + 0] = proyectils[i].Position.X;
            allPositions[index + 1] = proyectils[i].Position.Y;
            allPositions[index + 2] = proyectils[i].Position.Z;
          }

          // Upload to GPU
          gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
          gl.BufferSubData<float>(BufferTargetARB.ArrayBuffer, 0, allPositions);
          
          // Clear screen
          gl.Clear(ClearBufferMask.ColorBufferBit);

          gl.BindVertexArray(vao);
          gl.EnableVertexAttribArray(0);  
          gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 0, 0); 

          gl.PointSize(5.0f);
          gl.DrawArrays(GLEnum.Points, 0, (uint)particles.Count);
          gl.PointSize(5.0f);
          gl.DrawArrays(GLEnum.Points, particles.Count, (uint)proyectils.Count);
          
      }

    private static List<float> GenerateNumber(int num, float max, float min) // Random number for launchers
    {
      Random rand = new Random((int)DateTime.Now.Ticks);
      List<float> randomNumbers = new List<float>();
      for (int i = 0; i < num; i++)
      {
        float randomFloat = (float)(rand.NextDouble() * (max - min) + min);  
        randomNumbers.Add(randomFloat);  
      }
      return randomNumbers;
    }

  }
  public struct Particle
  {
      public Vector3D<float> Position;  
      public Vector3D<float> Velocity; 
      public float Age;                  
      public float LifeSpan;           
      public Particle(Vector3D<float> position, Vector3D<float> velocity, float age, float lifeSpan)
      {
          Position = position;
          Velocity = velocity;
          Age = age;
          LifeSpan = lifeSpan;
      }
  }
  public struct Proyectil
  {
    public Vector3D<float> Position;  
    public Vector3D<float> Velocity; 
    public float Age;                  
    public float LifeSpan;           
    public Proyectil(Vector3D<float> position, Vector3D<float> velocity, float age, float lifeSpan)
    {
        Position = position;
        Velocity = velocity;
        Age = age;
        LifeSpan = lifeSpan;
    }
  }
  class Launcher
  {
      public Vector3D<float> Position;  
      public Vector3D<float> Velocity;     
      public Launcher(Vector3D<float> position, Vector3D<float> velocity)
      {
        Position = position;
        Velocity = velocity;
      }
      public void Emit(List<Particle> particles) 
      {
        for(int i=0; i < 1; i++)
        {
          particles.Add(new Particle
              {
                  Position = Position, 
                  Velocity = Velocity, 
                  Age = 0,  
                  LifeSpan = 3f  
              });
        }
      }
  }
}