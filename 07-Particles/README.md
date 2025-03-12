# Documentation of the task "07-Particles"

# OpenGL (Silk.NET) Pilot Project

## Features

- **Silk.NET.Windowing.Window-based main program**
- **Window title displays real-time status** (see `Program.WindowTitle()`)
- **Command-line arguments:**
  - `-w`, `-h` - Initial window size in pixels
  - `-p` - Maximum number of particles in the system
  - `-r` - Particle generator rate (particles per second, adjustable with Up/Down keys)
  - `-t` - Optional texture file (default: checkerboard)
- **Console window for messaging** (`Util.Ut.Message()`)
- **Trackball support**
- **FPS (Frames Per Second) and PPS (Primitives Per Second) measurement** (`Util.FPS`)
- **Vertical synchronization toggle (VSync) with `V` key**
- **Keyboard and mouse event handling:**
  - Keyboard: `KeyDown()`, `KeyUp()`
  - Mouse: `MouseDown()`, `MouseUp()`, `MouseDoubleClick()`, `MouseMove()`, `MouseScroll()`
- **Particle system simulation and rendering**
  - Particle attributes: Position, Color, Age, Size
  - `Simulation` class manages particle creation and destruction

## Simulation

- `SimulateTo(double time)`: Simulates the entire particle system until the given time.
- Particle updates are handled with `GL.BufferSubData()`.
- **Simulation logic:**
  - Keeps the number of particles close to `Simulation.MaxParticles`.
  - Retired particles are replaced with new ones.
  - Particle attributes (size, color) change with age.
  - Ensure particle count does not exceed the buffer limit to prevent overflow.

## Particle System Functions

### `SimulateTo(double time)`
- Updates the particle system per frame.
- Returns `false` when a particle is retired and should be removed.

### `FillBuffer(float[] buffer, ref int i)`
- Fills `VERTEX_SIZE` floats into the buffer at index `i`.
- Each particle uses 12 floats:
  - `(x, y, z, R, G, B, Nx, Ny, Nz, s, t, size)`

## Shaders

### `vertex.glsl`

#### Inputs:
```glsl
layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vColor;
layout (location = 2) in vec3 vNormal;
layout (location = 3) in vec2 vTxt;
layout (location = 4) in float vSize;
```

#### Uniforms:
```glsl
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
```

#### Outputs:
```glsl
out vec3 fColor;
out vec2 fTxt;
out vec3 fNormal;
out vec4 fWorld;
```

- Uses `gl_Position` and `gl_PointSize` for rendering.

### `fragment.glsl`

- Handles **texturing** and **Phong shading**.
- Outputs color using:
```glsl
out vec4 FragColor;
```
- Supports both `vec3 (RGB)` and `vec4 (RGBA)`.

For more details, check the `vertex.glsl` and `fragment.glsl` files.
