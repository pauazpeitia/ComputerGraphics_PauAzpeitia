using Silk.NET.Maths;

namespace _08_Fireworks
{
    using Vector3 = Vector3D<float>;
    
    public class FireworkSystem
        {
            List<FirweorkParticle> FirweorkParticles = new();
            int flashsPerSecondaryExplosion = 40;
            public int MaxFirweorkParticles { get; set; }
            double SimulatedTime;
            public double FirweorkParticleRate { get; set; }
            double nextRocketLaunchTime;
            float maxExplosionHeight = 40.0f;
            int flashsPerExplosion = 100;
            int rocketsInPop = 0;
            int RocketsPerPop = 6;
            double GapBetweenPop = 2.0;
            public Vector3 Draft { get; set; }
            double PopInterval = 0.3;
            static Random rand = new Random();
            int RingFireworks = 32;

            public FireworkSystem(double now, double FirweorkParticleRate, int maxFirweorkParticles, int initFirweorkParticles)
            {
                SimulatedTime = now;
                MaxFirweorkParticles = maxFirweorkParticles;
                nextRocketLaunchTime = now;
                Draft = new Vector3(0.0f, 0.0f, 0.0f);
            }

            public void SimulateTo(double time)
            {
                if (time <= SimulatedTime)
                    return;
                double dt = time - SimulatedTime;
                SimulatedTime = time;
                float windX = 1.0f + 0.5f * (float)Math.Sin(0.5 * time);
                float windZ = 0.5f + 0.2f * (float)Math.Cos(0.3 * time);
                Draft = new Vector3(windX, 0.0f, windZ);

                List<int> toRemove = new();
                List<Vector3> explosions = new();        
                List<Vector3> secondaryExplosions = new(); 
                
                for (int i = 0; i < FirweorkParticles.Count; i++)
                {
                    FirweorkParticle p = FirweorkParticles[i];
                    bool alive = p.SimulateTo(Draft, time);
                    if (!alive)
                    {
                        toRemove.Add(i);
                    }
                    else if (p.Mode == 0 && (p.Acceleration.Y <= 0 || p.Position.Y >= maxExplosionHeight))
                    {
                        explosions.Add(new Vector3(p.Position.X, p.Position.Y, p.Position.Z));
                        toRemove.Add(i);
                    }
                    else if (p.Mode == 1 && p.Acceleration.Length < 2.0f && !p.SecondaryExploded)
                    {
                        secondaryExplosions.Add(p.Position);
                        p.SecondaryExploded = true;
                        toRemove.Add(i);
                    }
                }
                toRemove.Sort();
                toRemove.Reverse();
                foreach (int idx in toRemove)
                    FirweorkParticles.RemoveAt(idx);
                foreach (var position in explosions) // load
                {
                    Vector3 baseColor = new Vector3(
                        (float)(0.5 + rand.NextDouble() * 0.5),
                        (float)(0.5 + rand.NextDouble() * 0.5),
                        (float)(0.5 + rand.NextDouble() * 0.5)
                    );
                    for (int j = 0; j < flashsPerExplosion; j++)
                    {
                        if (FirweorkParticles.Count < MaxFirweorkParticles)
                            FirweorkParticles.Add(FirweorkParticle.Createflash(time, position, baseColor));
                    }
                }
                foreach (var position in secondaryExplosions)
                {
                    Vector3 baseColor = new Vector3(
                        (float)(0.5 + rand.NextDouble() * 0.5),
                        (float)(0.5 + rand.NextDouble() * 0.5),
                        (float)(0.5 + rand.NextDouble() * 0.5)
                    );
                    if (rand.NextDouble() < 0.5)
                    {
                    // Ring explosion: generate ring particles.
                        for (int j = 0; j < RingFireworks; j++)
                        {
                            if (FirweorkParticles.Count < MaxFirweorkParticles)
                                FirweorkParticles.Add(FirweorkParticle.CreateRing(time, position, baseColor, j, RingFireworks));
                        }
                    }
                    for (int s = 0; s < flashsPerSecondaryExplosion; s++)
                    {
                        if (FirweorkParticles.Count < MaxFirweorkParticles)
                            FirweorkParticles.Add(FirweorkParticle.CreateSecondaryflash(time, position, baseColor));
                    }
                }
                if (nextRocketLaunchTime <= time)
                {                // Launcher
                    if (FirweorkParticles.Count < MaxFirweorkParticles)
                    {
                        FirweorkParticles.Add(new FirweorkParticle(nextRocketLaunchTime));
                        rocketsInPop++;
                        if (rocketsInPop < RocketsPerPop)
                        {
                            nextRocketLaunchTime = time + PopInterval;
                        }
                        else
                        {
                            rocketsInPop = 0;
                            nextRocketLaunchTime = time + GapBetweenPop;
                        }
                    }
                }
            }
            public int BufferLoad(float[] buffer)
            {
                int i = 0;
                foreach (var p in FirweorkParticles)
                    p.FillBuffer(buffer, ref i);
                return FirweorkParticles.Count;
            }
            public void Reset()
            {
                FirweorkParticles.Clear();
                nextRocketLaunchTime = SimulatedTime;
            }
            public void LaunchRocket()
            {
                if (FirweorkParticles.Count < MaxFirweorkParticles)
                {
                    FirweorkParticles.Add(new FirweorkParticle(SimulatedTime));
                    rocketsInPop++;
                    if (rocketsInPop < RocketsPerPop)
                    {
                        nextRocketLaunchTime = SimulatedTime + PopInterval;
                    }
                    else
                    {
                        rocketsInPop = 0;
                        nextRocketLaunchTime = SimulatedTime + GapBetweenPop;
                    }
                }
            }
        }
}