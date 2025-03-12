using Silk.NET.Maths;
namespace _08_Fireworks
{
    using Vector3 = Vector3D<float>;
    class FirweorkParticle
        {
            float gravityforce = 9.81f;
            float dragC = 0.2f;  
            private static Random rand = new((int)DateTime.Now.Ticks);
            public int Mode; 
            public bool SecondaryExploded { get; set; } = false; 
            public Vector3 Acceleration { get; private set; }
            public Vector3 Position { get; private set; }
            double pLifeSpan = 4;   
            const double flashLifeSpan = 15;           
            const double secondaryflashLifeSpan = 8; 
            const double ringLifetime = 4;
            public Vector3 Color { get; private set; }
            public float Size { get; private set; }
            public double Age { get; private set; }
            private double Simulatedzeit;
             
        
            public static FirweorkParticle CreateSecondaryflash(double now, Vector3 explosionPos, Vector3 baseColor)
            {
                FirweorkParticle p = new FirweorkParticle(now);
                p.Mode = 2;
                p.Position = explosionPos;
                double theta = rand.NextDouble() * 2 * Math.PI;
                double phi = Math.Acos(2 * rand.NextDouble() - 1);
                double targetPhi = 0.349066;
                phi = phi * 0.7 + targetPhi * 0.3;
                float velocity = (float)(3.0 + rand.NextDouble() * 3.0);
                float x = velocity * (float)(Math.Sin(phi) * Math.Cos(theta)) *3;
                float y = velocity * (float)(Math.Sin(phi) * Math.Sin(theta)) *3;
                float z = velocity * (float)Math.Cos(phi) * 3;
                p.Acceleration = new Vector3(x, y, z);
                float factor = 0.9f + (float)(rand.NextDouble() * 0.2f);
                p.Color = new Vector3(baseColor.X * factor, baseColor.Y * factor, baseColor.Z * factor);
                p.Size = 2f;
                p.Age = secondaryflashLifeSpan;
                p.Simulatedzeit = now;
                return p;
            }
            public FirweorkParticle(double now)
            {
                Mode = 0;
                Simulatedzeit = now;
                float offsetX = (float)(rand.NextDouble() * 50.0 - 10.0);
                float offsetZ = (float)(rand.NextDouble() * 50.0 - 10.0);
                Position = new Vector3(offsetX, 10.0f, offsetZ);
                double drift = (rand.NextDouble() * 40.0 - 20.0) * (Math.PI / 180.0);
                double azimuth = rand.NextDouble() * 2 * Math.PI;
                //Acceleration 
                float velocity = (float)(25.0 + rand.NextDouble() * 10.0);
                float x = velocity * (float)(Math.Sin(drift) * Math.Cos(azimuth));
                float y = velocity * (float)Math.Cos(drift);
                float z = velocity * (float)(Math.Sin(drift) * Math.Sin(azimuth));
                Acceleration = new Vector3(x, y, z);

                Color = new Vector3(1.0f, 1.0f, 1.0f); //white
                Size = 7f;
                Age = pLifeSpan;
            }
            public static FirweorkParticle Createflash(double now, Vector3 explosionPos, Vector3 baseColor)
            {
                FirweorkParticle p = new FirweorkParticle(now);
                p.Mode = 1;
                p.Position = explosionPos;
                double phi = Math.Acos(2 * rand.NextDouble() - 1);
                double theta = rand.NextDouble() * 2 * Math.PI;
                float velocity = (float)(5.0 + rand.NextDouble() * 5.0);
                float x = velocity * (float)(Math.Sin(phi) * Math.Cos(theta));
                float y = velocity * (float)(Math.Sin(phi) * Math.Sin(theta));
                float z = velocity * (float)Math.Cos(phi);
                p.Acceleration = new Vector3(x, y, z);
                float factor = 0.9f + (float)(rand.NextDouble() * 0.2f);
                p.Color = new Vector3(baseColor.X * factor, baseColor.Y * factor, baseColor.Z * factor);
                p.Size = 2f;
                p.Age = flashLifeSpan;
                p.Simulatedzeit = now;
                
                return p;
            }

            public static FirweorkParticle CreateRing(double now, Vector3 explosionPos, Vector3 baseColor, int ringIndex, int totalParticles)
            {
                FirweorkParticle p = new FirweorkParticle(now);
                p.Mode = 3;
                p.Position = explosionPos;
                // Compute angle for ring: evenly distributed around 360°.
                double angle = 2 * Math.PI * ringIndex / totalParticles;
                // Set speed mainly horizontal with a small upward component.
                float speed = 7.0f;
                float vx = speed * (float)Math.Cos(angle);
                float vz = speed * (float)Math.Sin(angle);
                float vy = speed * 0.2f; // Small upward lift.
                p.Acceleration = new Vector3(vx, vy, vz);
                float factor = 0.9f + (float)(rand.NextDouble() * 0.2f);
                p.Color = new Vector3(baseColor.X * factor, baseColor.Y * factor, baseColor.Z * factor);
                p.Size = 2.5f;
                p.Age = ringLifetime;
                p.Simulatedzeit = now;
                return p;
            }
            public bool SimulateTo(Vector3 wind, double zeit)
            {
                if (zeit <= Simulatedzeit) return true;
                double dt = zeit - Simulatedzeit;
                Simulatedzeit = zeit;
                Age -= dt;
                if (Age <= 0.0) return false;
                Vector3 gravityforceVec = new Vector3(0.0f, -gravityforce, 0.0f);
                Vector3 drag = Acceleration * (-dragC);
                Vector3 acceleration = gravityforceVec + drag + wind;
                Acceleration += acceleration * (float)dt;
                Position += Acceleration * (float)dt;
                float lifeFactor = (float)(Age / (Mode == 0 ? pLifeSpan : (Mode == 1 ? flashLifeSpan : (Mode == 2 ? secondaryflashLifeSpan : ringLifetime))));
                if (Mode != 0) Color *= lifeFactor; //Color Interpolation
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
}