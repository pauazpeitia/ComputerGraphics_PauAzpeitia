using System;
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
      private static List<Launcher> launchers = new();  
      public static int numberLaunchers = 10; //Define Number of Launchers
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
        gl.BufferData<float>(BufferTargetARB.ArrayBuffer, (uint)(numberLaunchers * numberParticlesExplosion) * 3 * sizeof(float), null, BufferUsageARB.DynamicDraw); 
      }//How big should the buffer be?

      private static void OnRender(double deltaTime)
      {
        gl.Clear(ClearBufferMask.ColorBufferBit);  // Cler from previous render

        if (particles.Count < 1)
        {
          for (int i = 0; i < numberLaunchers; i++) 
          {
            launchers[i].Emit(particles);   //Launch particles
          }
        }   

        UpdateParticles((float)deltaTime);// Update particles until reach gravity = 0 position and then explode
        RenderParticles(); // Render
      }


      private static void UpdateParticles(float deltaTime)
      {
        Vector3D<float> gravity = new Vector3D<float>(0, -9.81f, 0);  // Gravity
        float dragCoefficient = 0.2f;  // Drag 

        List<Particle> newParticles = new List<Particle>();

        for (int i = 0; i < particles.Count; i++) // Every particle
        {
          Particle p = particles[i];
          if(p.Velocity.Y < 0){ //Empieza a bajar : AQUI DEBE EXPLOTAR!
            
            particles.RemoveAt(i);
            particles = p.Explosion(numberParticlesExplosion);
            break;
          }
          if(p.Age > p.LifeSpan){particles.RemoveAt(i);break;}
          else
          {
          p.Velocity += gravity * deltaTime; 
          Vector3D<float> dragForce = -dragCoefficient * p.Velocity; 
          p.Velocity += dragForce * deltaTime;

          p.Position += p.Velocity * deltaTime; // Update position
          p.Age += deltaTime;  

          particles[i] = p;//Update
          }
        }
      }
      private static void RenderParticles()
      {
          
          float[] particlePositions = new float[particles.Count * 3];  // 3 coordenates: (X, Y, Z)
          for (int i = 0; i < particles.Count; i++)
          {
              particlePositions[i * 3 + 0] = particles[i].Position.X;
              particlePositions[i * 3 + 1] = particles[i].Position.Y;
              particlePositions[i * 3 + 2] = particles[i].Position.Z;
          }

          // Upload to GPU
          gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
          gl.BufferSubData<float>(BufferTargetARB.ArrayBuffer, 0, particlePositions);

          // Clear screen
          gl.Clear(ClearBufferMask.ColorBufferBit);

          
          gl.PointSize(5.0f);  
          gl.BindVertexArray(vao);

          gl.EnableVertexAttribArray(0);  // Habilitar el atributo del vértice (posiciones)
          gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 0, 0);  // Definir cómo se organizan los datos de los vértices

          // Dibujar las partículas como puntos
          gl.DrawArrays(GLEnum.Points, 0, (uint)particles.Count);
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
  // Particle Structure
  public struct Particle
  {
      public Vector3D<float> Position;  
      public Vector3D<float> Velocity; 
      public float Age;                  
      public float LifeSpan;           
      //Constructor
      public Particle(Vector3D<float> position, Vector3D<float> velocity, float age, float lifeSpan)
      {
          Position = position;
          Velocity = velocity;
          Age = age;
          LifeSpan = lifeSpan;
      }
      //Explosion Attempt
      public List<Particle> Explosion(int numberParticlesExplosion)
      {
        List<Particle> explosionparticles = new List<Particle>();
        for(int j=0; j < numberParticlesExplosion; j++)
          {
            Random rand = new Random();
            Particle son = new Particle(Position, new Vector3D<float>(
              (float)rand.NextDouble() * (5 - (-2)) + (-2) ,
              (float)rand.NextDouble() * (5 - (-2)) + (-2), 
              (float)rand.NextDouble() * (5 - (-2)) + (-2)), 
              0f, 
              1f);
            explosionparticles.Add(son);
          }
          return explosionparticles;
      }
  }
  //Launcher Structure
  class Launcher
  {
      public Vector3D<float> Position;  
      public Vector3D<float> Velocity;
      private Random rand = new();      
      //Constructor
      public Launcher(Vector3D<float> position, Vector3D<float> velocity)
      {
        Position = position;
        Velocity = velocity;
      }
      public void Emit(List<Particle> particles) //Emit Particle
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