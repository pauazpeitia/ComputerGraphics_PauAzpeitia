# Documentation of the task "08-Fireworks"

## Author
Pau Azpeitia

## Command line arguments
-'w': "width"
-'h', "height"
-'p', "FirweorkParticles"
-'r', "rate"

## Input data
This program does not require any input data from the user. 

## Algorithm
First I tried to program from scratch but it was a challenge especially for the buffer adaptation. So what I have done is to take the presented skeleton and implement two different classes inspired by Particle and Simulation, respectively Firework Particle and Firework System.
class FirweorkParticle:
  The modes (Mode) in the FirweorkParticle class determine the type of particle and its behavior within the fireworks system. 0 (Normal) Base particle of the firework before exploding. 1 (Createflash) Bright particle generated after the explosion. 2 (CreateSecondaryflash) Particle from a secondary explosion.
  The SimulateTo(Vector3 wind, double zeit) method is responsible for updating the simulation of the particle up to a given time, applying physics such as gravity, air drag and wind.
  class FireworkSystem:
  Here the adapted version of SimulateTo() is used. Time update and dynamic wind: First, the method checks if the current time is greater than the previous simulation time. If the time has advanced, it calculates the time change (dt) and uppdates the simulation time. In adition, it generates a dynamic wind using trigonometric functions, which changes over time and affects the particle motion. The method runs through all firework particles, represented in the FirwoerkParticles list. For each particle tt simulates its motion, passing the current wind and time. If the particle dies, it is marked for deletion. If the particle is a rocket (Mode == 0) and has reached its peak, it is added to the list of primary explosions and removed. If the particle is an explosion (Mode == 1) and its velocity is low, a secondary explosion is considered to have ocurred. In this case, it is added to the list of secondary explosions and marked as exploded, then deleted. Dead particle removal: After processing all particles, the method removes particles that are no longer needed, i.e. those that have died or have been processed (primary or secondary explosions).

  Generation of new explosion particles: For each primary explosion position, a specific number of new “flash” (explosion) particles are created, using a random color. The same happens with secondary explosions: for each secondary explosion position, new secondary flash particles are generated.
  Launching new rockets: Finally, the method checks if it is the right time to launch a new rocket (If the current time is greater than or equal to the scheduled launch time (nextRocketLaunchTime)).  If the number of rockets set in the group (RocketsPerPop) has been launched, it waits for an interval before launching more. If not enough have been launched, another rocket is launched at the default interval (PopInterval).

  In Program.cs the OnLoad:  It initializes the particle system (FireworkSystem), loads the OpenGL shaders, and configures the camera (trackball). OnRender: The particle system is simulated up to the current time (sim.SimulateTo(nowSeconds)).Load the generated particles into the vertex buffer.The shader program and uniform variables, such as view matrix, projection, and lighting, are set. Finally, the particles are drawn using Gl.DrawArrays.

## Extra work / Bonuses
-Multiple rocket/particle types:
In the code, different particle modes are handled (Mode = 0, Mode = 1, Mode = 2), there are different particle types (rockets, primary explosions, and secondary explosions). 
-Multi-stage explosions:
The particle system handles secondary explosions (secondaryExplosions), there is a second stage of explosions after the initial explosion.
-Color/point-size changes during life of a particle/rocket:
In the SimulateTo method of the FirweorkParticle class, the color and size of the particles are adjusted according to their lifetime (lifeFactor). This fulfills the requirement of color and size changes during the lifetime of the particles. In addition, the initial rocket (mode 0) increases its size to be visible at the moment it is launched, and then shrinks to give way and importance to the explosion itself. (also "Visualization of rocket trajectories" criterion)
-Interactive fireworks control:
In the Program.cs file, user interaction is handled via the keyboard (e.g., Up, Down, R keys,...), which allows controlling the particle generation rate and restarting the simulation.

## Use of AI
-This task has been really difficult for my current programming skills with C# and OpenGl. I needed in many times the help of an artificial intelligence, which has guided me in the development of both classes and the OnLoad, OnRender structure.
